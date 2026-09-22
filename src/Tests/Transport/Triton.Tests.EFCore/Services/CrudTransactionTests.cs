#pragma warning disable 1591

using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TheXDS.Triton.EFCore.Services;
using TheXDS.Triton.Models.Base;
using TheXDS.Triton.Services;
using TheXDS.Triton.Tests.EFCore.Models;
using TheXDS.Triton.Tests.Models;

namespace TheXDS.Triton.Tests.EFCore.Services;

public class CrudTransactionTests
{
    private static CrudTransaction<BlogContext> GetTestTransaction() => new(((IMiddlewareConfigurator)new TransactionConfiguration()).GetRunner(), null);

    [Test]
    public void CrudTransaction_class_contains_Context_property()
    {
        var t = GetTestTransaction();

        Assert.That(t.Context, Is.Not.Null);
        Assert.That(t.Context, Is.InstanceOf<BlogContext>());
    }

    [Test]
    public void All_T_gets_DbSet()
    {
        using var t = GetTestTransaction();
        var result = t.All<User>();
        Assert.That(result, Is.InstanceOf<QueryServiceResult<User>>());
        Assert.That(result.Result, Is.InstanceOf<DbSet<User>>());
    }

    [Test]
    public void All_T_reads_from_DbSet()
    {
        using var t = GetTestTransaction();
        var result = t.All<User>().FirstOrDefault(p => p.Id == "user1");
        Assert.That(result, Is.InstanceOf<User>());
    }

    [Test]
    public void All_gets_DbSet()
    {
        using var t = GetTestTransaction();
        var result = t.All(typeof(User));
        Assert.That(result, Is.InstanceOf<QueryServiceResult<Model>>());
        Assert.That(result.Result, Is.AssignableTo<DbSet<User>>());
    }

    [Test]
    public void All_reads_from_DbSet()
    {
        using var t = GetTestTransaction();
        var result = t.All(typeof(User)).ToList().FirstOrDefault(p => p.Metadata.IdAsString == "user1");
        Assert.That(result, Is.InstanceOf<User>());
    }

    [Test]
    public async Task CrudTransaction_async_ops_commit_automatically_test()
    {
        await using (var t = GetTestTransaction())
        {
            var r = t.Create(new User("user0123", "User 0-1-2-3"));
            Assert.That(r.IsSuccessful, Is.True);
        }

        await using (var t = GetTestTransaction())
        {
            var r = t.Read<User, string>("user0123", out var u);
            Assert.That(r.IsSuccessful, Is.True);
            Assert.That(u, Is.Not.Null);
        }
    }

    [Test]
    public void CrudTransaction_can_read_data_test_1()
    {
        using var t = GetTestTransaction();
        var r = t.Read<User, string>("user1");
        Assert.That(r.IsSuccessful, Is.True);
        Assert.That(r.Result, Is.Not.Null);
    }

    [Test]
    public async Task CrudTransaction_can_read_data_async_test_1()
    {
        await using var t = GetTestTransaction();
        var r = t.Read<User, string>("user1");
        Assert.That(r.IsSuccessful, Is.True);
        Assert.That(r.Result, Is.Not.Null);
    }

    [Test]
    public void CrudTransaction_can_read_data_test_2()
    {
        using var t = GetTestTransaction();
        var r = t.Read<User, string>("user1", out var u);
        Assert.That(r.IsSuccessful, Is.True);
        Assert.That(u, Is.Not.Null);
    }

    [Test]
    public async Task CrudTransaction_can_read_data_async_test_2()
    {
        await using var t = GetTestTransaction();
        var r = t.Read<User, string>("user1", out var u);
        Assert.That(r.IsSuccessful, Is.True);
        Assert.That(u, Is.Not.Null);
    }

    [Test]
    public void CrudTransaction_write_new_data_and_verify_test()
    {
        using (var t = GetTestTransaction())
        {
            var r = t.Create(new User("user123", "User 1-2-3"));
            Assert.That(r.IsSuccessful, Is.True);
        }

        using (var t = GetTestTransaction())
        {
            var r = t.Read<User, string>("user123", out var u);
            Assert.That(r.IsSuccessful, Is.True);
            Assert.That(u, Is.Not.Null);
        }
    }

    [Test]
    public async Task CrudTransaction_write_new_data_and_verify_async_test()
    {
        await using (var t = GetTestTransaction())
        {
            var r = t.Create(new User("user123", "User 1-2-3"));
            Assert.That(r.IsSuccessful, Is.True);
        }

        await using (var t = GetTestTransaction())
        {
            var r = t.Read<User, string>("user123", out var u);
            Assert.That(r.IsSuccessful, Is.True);
            Assert.That(u, Is.Not.Null);
        }
    }

    [Test]
    public void CrudTransaction_write_data_delete_and_verify_test_1()
    {
        using (var t = GetTestTransaction())
        {
            t.Create(new User("user456", "User 4-5-6"));
        }

        using (var t = GetTestTransaction())
        {
            var r = t.Delete<User, string>("user456");
            Assert.That(r.IsSuccessful, Is.True);
        }

        using (var t = GetTestTransaction())
        {
            var r = t.Read<User, string>("user456", out var u);
            Assert.That(r.Reason, Is.EqualTo(FailureReason.NotFound));
            Assert.That(u, Is.Null);
        }
    }

    [Test]
    public async Task CrudTransaction_write_data_delete_and_verify_async_test_1()
    {
        await using (var t = GetTestTransaction())
        {
            t.Create(new User("user456", "User 4-5-6"));
            await t.CommitAsync();
        }

        await using (var t = GetTestTransaction())
        {
            var r = t.Delete<User, string>("user456");
            Assert.That(r.IsSuccessful, Is.True);
            await t.CommitAsync();
        }

        await using (var t = GetTestTransaction())
        {
            var r = t.Read<User, string>("user456", out var u);
            Assert.That(r.Reason, Is.EqualTo(FailureReason.NotFound));
            Assert.That(u, Is.Null);
        }
    }

    [Test]
    public void CrudTransaction_write_data_delete_and_verify_test_2()
    {
        using (var t = GetTestTransaction())
        {
            t.Create(new User("user456", "User 4-5-6"));
        }

        using (var t = GetTestTransaction())
        {
            var u = t.Read<User, string>("user456").Result!;
            var r = t.Delete(u);
            Assert.That(r.IsSuccessful, Is.True);
        }

        using (var t = GetTestTransaction())
        {
            var r = t.Read<User, string>("user456", out var u);
            Assert.That(r.Reason, Is.EqualTo(FailureReason.NotFound));
            Assert.That(u, Is.Null);
        }
    }

    [Test]
    public async Task CrudTransaction_write_data_delete_and_verify_async_test_2()
    {
        await using (var t = GetTestTransaction())
        {
            t.Create(new User("user456", "User 4-5-6"));
            await t.CommitAsync();
        }

        await using (var t = GetTestTransaction())
        {
            var u = t.Read<User, string>("user456").Result!;
            var r = t.Delete(u);
            Assert.That(r.IsSuccessful, Is.True);
            await t.CommitAsync();
        }

        await using (var t = GetTestTransaction())
        {
            var r = t.Read<User, string>("user456", out var u);
            Assert.That(r.Reason, Is.EqualTo(FailureReason.NotFound));
            Assert.That(u, Is.Null);
        }
    }

    [Test]
    public void CrudTransaction_update_and_verify_test()
    {
        using (var t = GetTestTransaction())
        {
            var r = t.Create(new User("user123", "User 1-2"));
            Assert.That(r.IsSuccessful, Is.True);
        }

        using (var t = GetTestTransaction())
        {
            t.Read<User, string>("user123", out var u);
            u!.PublicName = "User 1-2-3";
            var r = t.Update(u);
            Assert.That(r.IsSuccessful, Is.True);
        }

        using (var t = GetTestTransaction())
        {
            var r = t.Read<User, string>("user123", out var u);
            Assert.That(r.IsSuccessful, Is.True);
            Assert.That(u, Is.Not.Null);
            Assert.That(u!.PublicName, Is.EqualTo("User 1-2-3"));
        }
    }

    [Test]
    public async Task CrudTransaction_update_and_verify_async_test()
    {
        await using (var t = GetTestTransaction())
        {
            var r = t.Create(new User("user789", "User 1-2"));
            Assert.That(r.IsSuccessful, Is.True);
            Assert.That((await t.CommitAsync()).IsSuccessful, Is.True);
        }

        await using (var t = GetTestTransaction())
        {
            var u = (await t.ReadAsync<User, string>("user789")).Result;
            u!.PublicName = "User 1-2-3";
            Assert.That(t.Update(u).IsSuccessful, Is.True);
            Assert.That((await t.CommitAsync()).IsSuccessful, Is.True);
        }

        await using (var t = GetTestTransaction())
        {
            var r = t.Read<User, string>("user789", out var u);
            Assert.That(r.IsSuccessful, Is.True);
            Assert.That(u, Is.Not.Null);
            Assert.That(u!.PublicName, Is.EqualTo("User 1-2-3"));
        }
    }

    [Test]
    public async Task CrudTransaction_ReadAsync_test()
    {
        await using (var t = GetTestTransaction())
        {
            var r = t.Create(new User("user987", "User 9-8-7"));
            Assert.That(r.IsSuccessful, Is.True);
        }

        await using (var t = GetTestTransaction())
        {
            var r = await t.ReadAsync<User, string>("user987");
            Assert.That(r.IsSuccessful, Is.True);
            Assert.That(r.Result, Is.Not.Null);
        }
    }

    [Test]
    public async Task SearchAsync_test()
    {
        await using var t = GetTestTransaction();
        var r = (await t.SearchAsync<User>(p => p.PublicName != null)).Result!;
        Assert.That(r, Is.Not.Null);
        Assert.That(r.Length, Is.Not.Zero);
    }

    [Test]
    public void CreateOrUpdate_creates_new_entity_test()
    {
        using (var t = GetTestTransaction())
        {
            var r = t.CreateOrUpdate(new User("userNew01", "New User 01"));
            Assert.That(r.IsSuccessful, Is.True);
        }

        using (var t = GetTestTransaction())
        {
            var r = t.Read<User, string>("userNew01", out var u);
            Assert.That(r.IsSuccessful, Is.True);
            Assert.That(u, Is.Not.Null);
            Assert.That(u!.PublicName, Is.EqualTo("New User 01"));
        }
    }

    [Test]
    public async Task CreateOrUpdate_creates_new_entity_async_test()
    {
        await using (var t = GetTestTransaction())
        {
            var r = t.CreateOrUpdate(new User("userNew02", "New User 02"));
            Assert.That(r.IsSuccessful, Is.True);
            await t.CommitAsync();
        }

        await using (var t = GetTestTransaction())
        {
            var r = await t.ReadAsync<User, string>("userNew02");
            Assert.That(r.IsSuccessful, Is.True);
            Assert.That(r.Result, Is.Not.Null);
            Assert.That(r.Result!.PublicName, Is.EqualTo("New User 02"));
        }
    }

    [Test]
    public void CreateOrUpdate_updates_existing_entity_test()
    {
        using (var t = GetTestTransaction())
        {
            var r = t.CreateOrUpdate(new User("userUpd01", "Original Name"));
            Assert.That(r.IsSuccessful, Is.True);
        }

        using (var t = GetTestTransaction())
        {
            t.Read<User, string>("userUpd01", out var u);
            u!.PublicName = "Updated Name";
            var r = t.CreateOrUpdate(u);
            Assert.That(r.IsSuccessful, Is.True);
        }

        using (var t = GetTestTransaction())
        {
            var r = t.Read<User, string>("userUpd01", out var u);
            Assert.That(r.IsSuccessful, Is.True);
            Assert.That(u, Is.Not.Null);
            Assert.That(u!.PublicName, Is.EqualTo("Updated Name"));
        }
    }

    [Test]
    public async Task CreateOrUpdate_updates_existing_entity_async_test()
    {
        await using (var t = GetTestTransaction())
        {
            var r = t.CreateOrUpdate(new User("userUpd02", "Original Name"));
            Assert.That(r.IsSuccessful, Is.True);
            await t.CommitAsync();
        }

        await using (var t = GetTestTransaction())
        {
            t.Read<User, string>("userUpd02", out var u);
            u!.PublicName = "Updated Name Async";
            var r = t.CreateOrUpdate(u);
            Assert.That(r.IsSuccessful, Is.True);
            await t.CommitAsync();
        }

        await using (var t = GetTestTransaction())
        {
            var r = t.Read<User, string>("userUpd02", out var u);
            Assert.That(r.IsSuccessful, Is.True);
            Assert.That(u, Is.Not.Null);
            Assert.That(u!.PublicName, Is.EqualTo("Updated Name Async"));
        }
    }

    [Test]
    public void CreateOrUpdate_handles_multiple_entities_mixed_test()
    {
        using (var t = GetTestTransaction())
        {
            var existing = t.Read<User, string>("user1").Result!;
            existing!.PublicName = "Updated via Mixed";
            var r = t.CreateOrUpdate(
                new User("userNew03", "Brand New"),
                existing
            );
            Assert.That(r.IsSuccessful, Is.True);
        }

        using (var t = GetTestTransaction())
        {
            var newResult = t.Read<User, string>("userNew03", out var newU);
            Assert.That(newResult.IsSuccessful, Is.True);
            Assert.That(newU, Is.Not.Null);
            Assert.That(newU!.PublicName, Is.EqualTo("Brand New"));

            var updResult = t.Read<User, string>("user1", out var updU);
            Assert.That(updResult.IsSuccessful, Is.True);
            Assert.That(updU, Is.Not.Null);
            Assert.That(updU!.PublicName, Is.EqualTo("Updated via Mixed"));
        }
    }

    [Test]
    public async Task CreateOrUpdate_handles_multiple_entities_mixed_async_test()
    {
        await using (var t = GetTestTransaction())
        {
            var r = t.CreateOrUpdate(
                new User("userNew04", "Brand New Async"),
                new User("userUpd03", "Original")
            );
            Assert.That(r.IsSuccessful, Is.True);
            await t.CommitAsync();
        }

        await using (var t = GetTestTransaction())
        {
            t.Read<User, string>("userUpd03", out var u);
            u!.PublicName = "Updated via Mixed Async";
            var r = t.CreateOrUpdate(
                new User("userNew05", "Another New"),
                u
            );
            Assert.That(r.IsSuccessful, Is.True);
            await t.CommitAsync();
        }

        await using (var t = GetTestTransaction())
        {
            var r1 = await t.ReadAsync<User, string>("userNew04");
            Assert.That(r1.IsSuccessful, Is.True);
            Assert.That(r1.Result, Is.Not.Null);
            Assert.That(r1.Result!.PublicName, Is.EqualTo("Brand New Async"));

            var r2 = await t.ReadAsync<User, string>("userUpd03");
            Assert.That(r2.IsSuccessful, Is.True);
            Assert.That(r2.Result, Is.Not.Null);
            Assert.That(r2.Result!.PublicName, Is.EqualTo("Updated via Mixed Async"));

            var r3 = await t.ReadAsync<User, string>("userNew05");
            Assert.That(r3.IsSuccessful, Is.True);
            Assert.That(r3.Result, Is.Not.Null);
            Assert.That(r3.Result!.PublicName, Is.EqualTo("Another New"));
        }
    }
}