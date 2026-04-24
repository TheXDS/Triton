using NUnit.Framework;
using TheXDS.Triton.Tests.Models;

namespace TheXDS.Triton.Tests.EFCore.Tests;

internal partial class CrudOpsTests : TritonEfTestClass
{
    [Test]
    public void RelatedDataEagerLoadingTest()
    {
        var q = _srv.GetAllUsersFirst3Posts().ToList();

        /* -= Initial test database: =-
         * There are 3 users, and only the first user should have a
         * Post. The other users should not have any.
         *
         * Based on the way the query is constructed, only the first user
         * and their corresponding post should be retrieved.
         */

        Assert.That(q.Count, Is.EqualTo(1));
        Assert.That(q[0].Key.Id, Is.EqualTo("user1"));
        Assert.That(q[0].Count(), Is.EqualTo(1));
    }

    [Test]
    public void SimpleReadTransactionTest()
    {
        using var t = _srv.GetReadTransaction();

        Post? post = t.Read<Post, long>(1L);
        Assert.That(post, Is.InstanceOf<Post>());
        Assert.That(post!.Title, Is.EqualTo("Test"));

        Comment? comment = t.Read<Comment>(1L);
        Assert.That(comment, Is.InstanceOf<Comment>());
        Assert.That(comment!.Content, Is.EqualTo("It works!"));
    }

    [Test]
    public async Task FullyAsyncReadTransactionTest()
    {
        await using var t = _srv.GetReadTransaction();

        Post? post = await t.ReadAsync<Post, long>(1L);
        Assert.That(post, Is.InstanceOf<Post>());
        Assert.That(post!.Title, Is.EqualTo("Test"));

        Comment? comment = await t.ReadAsync<Comment>(1L);
        Assert.That(comment, Is.InstanceOf<Comment>());
        Assert.That(comment!.Content, Is.EqualTo("It works!"));
    }
}