namespace TheXDS.Triton.Models;

/// <summary>
/// Modelo que representa a un usuario individual con facultad de iniciar sesión en un sistema que requiere autenticación.
/// </summary>
/// <param name="username">
/// Nombre de inicio de sesión a asociar con el usuario.
/// </param>
/// <param name="passwordHash">
/// Blob binario con el Hash a utilizar para autenticar al usuario.
/// </param>
public class LoginCredential(string username, byte[] passwordHash) : SecurityObject
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="LoginCredential"/>.
    /// </summary>
    public LoginCredential() : this(null!, null!)
    {
    }

    /// <summary>
    /// Obtiene o establece el nombre de inicio de sesión a asociar con esta
    /// entidad.
    /// </summary>
    public string Username { get; set; } = username;

    /// <summary>
    /// Obtiene o establece el Hash precomputado utilizado para autenticar al
    /// usuario.
    /// </summary>
    public byte[] PasswordHash { get; set; } = passwordHash;

    /// <summary>
    /// Obtiene o establece un valor que indica si la credencial está
    /// habilitada o no.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Obtiene o establece un valor que indica si se ha programado un cambio
    /// de contraseña para la credencial.
    /// </summary>
    public bool PasswordChangeScheduled { get; set; }

    /// <summary>
    /// Obtiene o establece la colección de sesiones activas para el usuario
    ///  representado por esta entidad.
    /// </summary>
    public virtual ICollection<Session> Sessions { get; set; } = [];

    /// <summary>
    /// Obtiene o establece la colección de objetos de autenticación en dos
    /// factores registrados para el usuario.
    /// </summary>
    public virtual ICollection<MultiFactorEntry> RegisteredMfa { get; set; } = [];
}
