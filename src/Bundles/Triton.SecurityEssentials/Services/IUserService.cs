using System.Security;
using TheXDS.MCART.Helpers;
using TheXDS.MCART.Security;
using TheXDS.Triton.Models;

namespace TheXDS.Triton.Services;

/// <summary>
/// Defines a set of members to be implemented by a type that exposes user management and security services.
/// </summary>
public interface IUserService : ITritonService
{
    /// <summary>
    /// Creates a new login credential by executing essential steps on it.
    /// </summary>
    /// <param name="username">
    /// The username to use for identifying the new credential.
    /// </param>
    /// <param name="password">
    /// The password to register with the new credential.
    /// </param>
    /// <param name="granted">
    /// Permission flags granted to the new credential.
    /// </param>
    /// <param name="revoked">
    /// Permission flags revoked from the new credential.
    /// </param>
    /// <param name="enabled">
    /// Indicates whether the new credential will be enabled.
    /// </param>
    /// <param name="passwordChangeScheduled">
    /// Indicates whether the new credential is scheduled to change its password upon login.
    /// </param>
    /// <param name="groups">
    /// The groups that the credential belongs to.
    /// </param>
    /// <returns>
    /// A task that, when completed, contains the result reported by the underlying service.
    /// </returns>
    Task<ServiceResult> AddNewLoginCredential(
        string username,
        SecureString password,
        PermissionFlags granted = PermissionFlags.None,
        PermissionFlags revoked = PermissionFlags.None,
        bool enabled = true,
        bool passwordChangeScheduled = false,
        params UserGroup[] groups)
    {
        return AddNewLoginCredential<Pbkdf2Storage>(
            username,
            password,
            granted,
            revoked,
            enabled,
            passwordChangeScheduled,
            groups);
    }

    /// <summary>
    /// Creates new login credentials by executing essential steps on them.
    /// </summary>
    /// <typeparam name="TAlg">
    /// Type of password storage algorithm to use.
    /// </typeparam>
    /// <param name="username">
    /// Login name to use for identifying the new credential.
    /// </param>
    /// <param name="password">
    /// Password to register with the new credential.
    /// </param>
    /// <param name="granted">Permission flags granted.</param>
    /// <param name="revoked">Permission flags revoked.</param>
    /// <param name="enabled">
    /// Indicates whether the new credential will be enabled.
    /// </param>
    /// <param name="passwordChangeScheduled">
    /// Indicates whether the new credential is scheduled to change its
    /// password upon login.
    /// </param>
    /// <param name="groups">
    /// Indicates the groups to which the credential belongs.
    /// </param>
    /// <returns>
    /// A task that, when completed, contains the result reported by the
    /// underlying service.
    /// </returns>
    async Task<ServiceResult> AddNewLoginCredential<TAlg>(string username, SecureString password, PermissionFlags granted = PermissionFlags.None, PermissionFlags revoked = PermissionFlags.None, bool enabled = true, bool passwordChangeScheduled = false, params UserGroup[] groups) where TAlg : IPasswordStorage, new()
    {
        await using var j = GetTransaction();
        var r = await j.SearchAsync<LoginCredential>(p => p.Username == username);
        if (!r.IsSuccessful) return r;
        if (r.Result!.Length != 0) return FailureReason.EntityDuplication;
        Guid id;
        do
        {
            id = Guid.NewGuid();
        } while ((await j.ReadAsync<LoginCredential, Guid>(id)).Result is not null);
        var newCred = new LoginCredential(username, await HashPasswordAsync<TAlg>(password))
        {
            Id = id,
            Granted = granted,
            Revoked = revoked,
            Enabled = enabled,
            PasswordChangeScheduled = passwordChangeScheduled,
        };
        foreach (var group in groups)
        {
            newCred.Membership.Add(new() { Group = group, Member = newCred });
        }
        j.Create(newCred);
        return await j.CommitAsync();
    }

    /// <summary>
    /// Executes an authentication operation with the provided credentials.
    /// </summary>
    /// <param name="username">Login name.</param>
    /// <param name="password">Password.</param>
    /// <returns>
    /// A task that, when completed, contains the result reported by the
    /// underlying service operation, including as the result value the new
    /// session of the user that has been authenticated. If it was not possible
    /// to authenticate the provided credentials, either because the user does
    /// not exist or because the password is invalid, the result value will be
    /// <see langword="null"/>.
    /// </returns>
    async Task<ServiceResult<Session?>> Authenticate(string username, SecureString password)
    {
        var r = await VerifyPassword(username, password);
        if (!(r.IsSuccessful && r.Result is { } result)) return r.CastUp<Session?>(null);

        if (result.Valid != true) return FailureReason.Forbidden;

        await using var j = GetWriteTransaction();
        Session s = new() { Timestamp = DateTime.UtcNow, Token = GenerateToken() };
        result.LoginCredential.Sessions.Add(s);
        j.Update(result.LoginCredential);
        j.Create(s);
        var retVal = await j.CommitAsync();
        s.Credential ??= result.LoginCredential;
        return retVal.CastUp(s);
    }

    /// <summary>
    /// Checks a user's access to a specific security context.
    /// </summary>
    /// <param name="username">Username to check.</param>
    /// <param name="context">Security context to check.</param>
    /// <param name="requested">Requested access flags.</param>
    /// <returns>
    /// <see langword="true"/> if the user has access to the resource,
    /// <see langword="false"/> if access has been explicitly denied,
    /// or <see langword="null"/> if no security object or flag is defined
    /// for the context.
    /// </returns>
    async Task<ServiceResult<bool?>> CheckAccess(string username, string context, PermissionFlags requested)
    {
        var r = await GetCredential(username);
        if (!r.IsSuccessful || r.Result is not { } c) return r.CastUp<bool?>(null);
        return CheckAccess(c, context, requested);
    }

    /// <summary>
    /// Checks a user's access to a specific security context.
    /// </summary>
    /// <param name="credential">Credential to check.</param>
    /// <param name="context">Security context to check.</param>
    /// <param name="requested">Requested access flags.</param>
    /// <returns>
    /// <see langword="true"/> if the user has access to the resource,
    /// <see langword="false"/> if access has been explicitly denied,
    /// or <see langword="null"/> if no security object or flag is defined
    /// for the context.
    /// </returns>
    ServiceResult<bool?> CheckAccess(SecurityObject credential, string context, PermissionFlags requested)
    {
        foreach (var j in new[] { credential }.Concat(credential.Membership.Select(p => p.Group)))
        {
            if (ChkAccessInternal(j, context, requested) is { } b) return b;
        }
        return new((bool?)null);
    }

    /// <summary>
    /// Gets an active session using its token.
    /// </summary>
    /// <param name="token">Session token to continue.</param>
    /// <returns>
    /// A task that contains the result of the asynchronous operation. The
    /// result may include a reference to the session to be continued, or
    /// <see langword="null"/> if the session does not exist, is invalid,
    /// or if a failure occurs in the underlying data service.
    /// </returns>
    async Task<ServiceResult<Session?>> ContinueSession(string token)
    {
        await using var j = GetReadTransaction();
        var query = j.All<Session>();
        if (query.IsSuccessful)
        {
            return await Task.Run(() => query.FirstOrDefault(p => p.Token == token)) is { } session && IsSessionValid(session) ? session : FailureReason.Forbidden;
        }
        else
        {
            return query.Reason!;
        }
    }

    /// <summary>
    /// Ends an open session.
    /// </summary>
    /// <param name="session">Session to end.</param>
    /// <returns>
    /// A task that, when completed, contains the result reported by the
    /// underlying service.
    /// </returns>
    async Task<ServiceResult> EndSession(Session session)
    {
        if (session.EndTimestamp.HasValue) return FailureReason.Idempotency;
        await using var t = GetWriteTransaction();
        session.EndTimestamp = DateTime.UtcNow;
        t.Update(session);
        return await t.CommitAsync();
    }

    /// <summary>
    /// Gets a registered login credential with the specified login name.
    /// </summary>
    /// <param name="username">
    /// Registered login name.
    /// </param>
    /// <returns>
    /// A task that, when completed, contains the result reported by the
    /// underlying service operation, including as the result value the
    /// entity obtained in the read operation. If no entity exists with the
    /// specified login name, the result value will be <see langword="null"/>.
    /// </returns>
    async Task<ServiceResult<LoginCredential?>> GetCredential(string username)
    {
        await using var j = GetReadTransaction();
        var r = await j.SearchAsync<LoginCredential>(p => p.Username == username);
        if (r.IsSuccessful)
        {
            return r.Result?.FirstOrDefault() is { } credential
                ? new ServiceResult<LoginCredential?>(credential)
                : FailureReason.NotFound;
        }
        return r.CastUp<LoginCredential?>(null);
    }

    /// <summary>
    /// Calculates the hash used to store password verification information.
    /// </summary>
    /// <param name="password">
    /// Password for which to generate a secure hash.
    /// </param>
    /// <returns>
    /// An array of bytes with the hash that was calculated from the provided
    /// password.
    /// </returns>
    byte[] HashPassword(SecureString password)
    {
        return HashPassword<Pbkdf2Storage>(password);
    }

    /// <summary>
    /// Calculates the hash used to store password verification information.
    /// </summary>
    /// <typeparam name="TAlg">Type of algorithm to use.</typeparam>
    /// <param name="password">
    /// Password for which to generate a secure hash.
    /// </param>
    /// <returns>
    /// An array of bytes with the hash that was calculated from the provided
    /// password.
    /// </returns>
    byte[] HashPassword<TAlg>(SecureString password) where TAlg : IPasswordStorage, new()
    {
        return PasswordStorage.CreateHash<TAlg>(password);
    }

    /// <summary>
    /// Calculates hash asynchronously for storing password verification info.
    /// </summary>
    /// <param name="password">
    /// Password for which to generate a secure hash.
    /// </param>
    /// <returns>
    /// A task that, when completed, will return an array of bytes with the
    /// hash calculated from the provided password.
    /// </returns>
    Task<byte[]> HashPasswordAsync(SecureString password)
    {
        return HashPasswordAsync<Pbkdf2Storage>(password);
    }

    /// <summary>
    /// Calculates hash asynchronously for storing password verification info.
    /// </summary>
    /// <typeparam name="TAlg">Type of algorithm to use.</typeparam>
    /// <param name="password">
    /// Password for which to generate a secure hash.
    /// </param>
    /// <returns>
    /// A task that, when completed, will return an array of bytes with the
    /// hash calculated from the provided password.
    /// </returns>
    Task<byte[]> HashPasswordAsync<TAlg>(SecureString password) where TAlg : IPasswordStorage, new()
    {
        return Task.Run(() => HashPassword<TAlg>(password));
    }

    /// <summary>
    /// Verifies that the provided credentials are valid.
    /// </summary>
    /// <param name="userId">Login name.</param>
    /// <param name="password">Password.</param>
    /// <returns>
    /// A task that, when completed, will return a tuple of values that indicates
    /// whether the verification was successful.
    /// </returns>
    /// <remarks>
    /// If you want to authenticate a user, use the <see cref="Authenticate(string, SecureString)"/>
    /// method, as it will create and persist an object representing the current session.
    /// </remarks>
    /// <seealso cref="Authenticate(string, SecureString)"/>
    async Task<ServiceResult<VerifyPasswordResult?>> VerifyPassword(string userId, SecureString password)
    {
        VerifyPasswordResult GetResult(byte[] expected, LoginCredential? user)
        {
            var success = PasswordStorage.VerifyPassword(password, expected) ?? false;
            return new VerifyPasswordResult(success, success ? user : null);
        }

        var r = await GetCredential(userId);
        return r switch
        {
            { IsSuccessful: false, Reason: FailureReason.NotFound } => VerifyPasswordResult.Invalid,
            { IsSuccessful: false } failure => failure.CastUp<VerifyPasswordResult?>(null),
            { IsSuccessful: true, Result: { PasswordHash: { } passwd, Enabled: true } user } => GetResult(passwd, user),
            _ => VerifyPasswordResult.Invalid
        };
    }

    private static bool? ChkAccessInternal(SecurityObject obj, string context, PermissionFlags requested)
    {
        return obj.Descriptors.FirstOrDefault(p => p.ContextId == context) is { } d
            && IsSet(d, requested) is { } b
            ? b
            : IsSet(obj, requested);
    }

    private static bool? IsSet(SecurityBase obj, PermissionFlags flags)
    {
        return obj.Granted.HasFlag(flags) ? true
            : obj.Revoked.HasFlag(flags) ? false
            : null;
    }

    private static string GenerateToken(int length = 128)
    {
        using var rnd = System.Security.Cryptography.RandomNumberGenerator.Create();
        var retVal = new byte[length];
        rnd.GetBytes(retVal);
        return Convert.ToBase64String(retVal);
    }

    private static bool IsSessionValid(Session session)
    {
        return session.EndTimestamp is null && session.Timestamp + TimeSpan.FromSeconds(session.TtlSeconds) > DateTime.UtcNow;
    }
}
