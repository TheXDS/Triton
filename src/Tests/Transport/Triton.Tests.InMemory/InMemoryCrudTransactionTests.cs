#pragma warning disable CS1591

using NUnit.Framework;
using TheXDS.Triton.InMemory.Services;
using TheXDS.Triton.Tests.Models;
using TheXDS.Triton.Models.Base;
using Moq;
using TheXDS.Triton.Middleware;
using TheXDS.Triton.Services;
namespace TheXDS.Triton.Tests.InMemory;

public class InMemoryCrudTransactionTests
{
    [Test]
    public void All_T_gets_all_entities_of_model()
    {
        User user = new() { Id = "abc123" };
        Post post = new() { Id = 1234, Author = user };
        using var transaction = new InMemoryCrudTransaction([user, post]);
        var result = transaction.All<User>();
        Assert.That(result.Result, Is.EquivalentTo([user]));
    }

    [Test]
    public void All_gets_all_entities_of_model()
    {
        User user = new() { Id = "abc123" };
        Post post = new() { Id = 1234, Author = user };
        using var transaction = new InMemoryCrudTransaction([user, post]);
        var result = transaction.All(typeof(User));
        Assert.That(result.Result, Is.EquivalentTo([user]));
    }

    [Test]
    public void Read_reads_entities()
    {
        User user = new() { Id = "abc123" };
        using var transaction = new InMemoryCrudTransaction([user]);
        var result = transaction.Read(typeof(User), "abc123");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccessful, Is.True);
        Assert.That(result.Result, Is.SameAs(user));
    }

    [Test]
    public async Task Create_creates_new_entities()
    {
        var newUser = new User("CreateTest", "Test user");
        ICollection<Model> store = [];
        using var transaction = new InMemoryCrudTransaction(store);
        Assert.That(transaction.Create(newUser).IsSuccessful, Is.True);
        Assert.That((await transaction.CommitAsync()).IsSuccessful, Is.True);
        Assert.That(store.Single(), Is.SameAs(newUser));
    }

    [Test]
    public async Task Create_fails_on_existing_entity()
    {
        var existingUser = new User("abc123", "Existing user");
        var newUser = new User("abc123", "New user");
        ICollection<Model> store = [existingUser];
        using var transaction = new InMemoryCrudTransaction(store);
        var result = transaction.Create(newUser);
        Assert.That(result.IsSuccessful, Is.False);
        Assert.That(result.Reason, Is.EqualTo(FailureReason.EntityDuplication));
        Assert.That((await transaction.CommitAsync()).IsSuccessful, Is.True);
        Assert.That(store.Single(), Is.SameAs(existingUser));
    }

    [Test]
    public async Task Update_updates_entities()
    {
        var existingUser = new User("abc123", "Existing user");
        var newUserData = new User("abc123", "Modified user");
        ICollection<Model> store = [existingUser];
        using var transaction = new InMemoryCrudTransaction(store);
        var result = transaction.Update(newUserData);
        Assert.That(result.IsSuccessful, Is.True);
        Assert.That((await transaction.CommitAsync()).IsSuccessful, Is.True);
        Assert.That(((User)store.Single()).PublicName, Is.EqualTo("Modified user"));
    }

    [Test]
    public async Task Update_fails_on_not_found()
    {
        var newUserData = new User("abc123", "Modified user");
        ICollection<Model> store = [];
        using var transaction = new InMemoryCrudTransaction(store);
        var result = transaction.Update(newUserData);
        Assert.That(result.IsSuccessful, Is.False);
        Assert.That(result.Reason, Is.EqualTo(FailureReason.NotFound));
        Assert.That((await transaction.CommitAsync()).IsSuccessful, Is.True);
        Assert.That(store, Is.Empty);
    }

    [Test]
    public async Task Delete_with_entity_fails_on_not_found()
    {
        var newUserData = new User("abc123", "Modified user");
        ICollection<Model> store = [];
        using var transaction = new InMemoryCrudTransaction(store);
        var result = transaction.Delete(newUserData);
        Assert.That(result.IsSuccessful, Is.False);
        Assert.That(result.Reason, Is.EqualTo(FailureReason.NotFound));
        Assert.That((await transaction.CommitAsync()).IsSuccessful, Is.True);
        Assert.That(store, Is.Empty);
    }

    [Test]
    public async Task Delete_with_string_key_fails_on_not_found()
    {
        ICollection<Model> store = [];
        using var transaction = new InMemoryCrudTransaction(store);
        var result = transaction.Delete<User>("abc123");
        Assert.That(result.IsSuccessful, Is.False);
        Assert.That(result.Reason, Is.EqualTo(FailureReason.NotFound));
        Assert.That((await transaction.CommitAsync()).IsSuccessful, Is.True);
        Assert.That(store, Is.Empty);
    }

    [Test]
    public async Task Delete_with_TKey_fails_on_not_found()
    {
        ICollection<Model> store = [];
        using var transaction = new InMemoryCrudTransaction(store);
        var result = transaction.Delete<User, string>("abc123");
        Assert.That(result.IsSuccessful, Is.False);
        Assert.That(result.Reason, Is.EqualTo(FailureReason.NotFound));
        Assert.That((await transaction.CommitAsync()).IsSuccessful, Is.True);
        Assert.That(store, Is.Empty);
    }

    [Test]
    public async Task Delete_with_TKey_deletes_entity()
    {
        ICollection<Model> store = [new User("abc123", "abc 123")];
        using var transaction = new InMemoryCrudTransaction(store);
        var result = transaction.Delete<User, string>("abc123");
        Assert.That(result.IsSuccessful, Is.True);
        Assert.That((await transaction.CommitAsync()).IsSuccessful, Is.True);
        Assert.That(store, Is.Empty);
    }

    [Test]
    public async Task Delete_with_string_key_deletes_entity()
    {
        ICollection<Model> store = [new User("abc123", "abc 123")];
        using var transaction = new InMemoryCrudTransaction(store);
        var result = transaction.Delete<User>("abc123");
        Assert.That(result.IsSuccessful, Is.True);
        Assert.That((await transaction.CommitAsync()).IsSuccessful, Is.True);
        Assert.That(store, Is.Empty);
    }

    [Test]
    public async Task Delete_with_non_generic_model_deletes_entities()
    {
        User user = new() { Id = "abc123" };
        User user2 = new() { Id = "abc456" };
        Post post = new() { Id = 1234, Author = user };
        ICollection<Model> store = [user, post, user2];
        using var transaction = new InMemoryCrudTransaction(store);
        var result = transaction.Delete([user, post]);
        await transaction.CommitAsync();
        Assert.That(result.IsSuccessful, Is.True);
        Assert.That(store, Is.EquivalentTo([user2]));
    }


    [Test]
    public void Delete_with_non_generic_model_fails_if_not_found()
    {
        User user = new() { Id = "abc123" };
        User user2 = new() { Id = "abc456" };
        Post post = new() { Id = 1234, Author = user };
        ICollection<Model> store = [user, post];
        using var transaction = new InMemoryCrudTransaction(store);
        var result = transaction.Delete((Model[])[user2]);
        Assert.That(result.IsSuccessful, Is.False);
        Assert.That(result.Reason, Is.EqualTo(FailureReason.NotFound));
    }
    [Test]
    public async Task Delete_with_generic_model_deletes_entities()
    {
        User user = new() { Id = "abc123" };
        User user2 = new() { Id = "abc456" };
        Post post = new() { Id = 1234, Author = user };
        ICollection<Model> store = [user, post, user2];
        using var transaction = new InMemoryCrudTransaction(store);
        var result = transaction.Delete([user2]);
        await transaction.CommitAsync();
        Assert.That(result.IsSuccessful, Is.True);
        Assert.That(store, Is.EquivalentTo((Model[])[user, post]));
    }


    [Test]
    public void Delete_with_generic_model_fails_if_not_found()
    {
        User user = new() { Id = "abc123" };
        User user2 = new() { Id = "abc456" };
        Post post = new() { Id = 1234, Author = user };
        ICollection<Model> store = [user, post];
        using var transaction = new InMemoryCrudTransaction(store);
        var result = transaction.Delete([user2]);
        Assert.That(result.IsSuccessful, Is.False);
        Assert.That(result.Reason, Is.EqualTo(FailureReason.NotFound));
    }

    [Test]
    public void Discard_discards_changes()
    {
        var newUser = new User("CreateTest", "Test user");
        ICollection<Model> store = [];
        using (var transaction = new InMemoryCrudTransaction(store))
        {
            transaction.Create(newUser);
            Assert.That(transaction.Discard().IsSuccessful, Is.True);
        }
        Assert.That(store, Is.Empty);
    }

    [Test]
    public void Commit_does_nothing_if_no_chages()
    {
        ICollection<Model> store = [];
        using (var transaction = new InMemoryCrudTransaction(store))
        {
            Assert.That(() => transaction.Commit(), Throws.Nothing);
        }
        Assert.That(store, Is.Empty);
    }
}