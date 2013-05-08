using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DailyEvents
{
  [TestFixture()]
  public class ApiClientTest
  {
    private readonly ApiClient api;

    public ApiClientTest()
    {
      this.api = new ApiClient();
    }

    [Test()]
    public void should_create_group()
    {
      string group = api.CreateGroup().id; 

      Assert.NotNull(group);
      Assert.NotNull(api.GetGroup(group).code);
    }

    [Test()]
    public void should_confirm_and_cancel_attendance()
    {
      string group = api.CreateGroup().id;

      api.SetStatus(group, "tfernandez", "yes");
      api.SetStatus(group, "ewatanabe", "yes");
      api.SetStatus(group, "gliguori", "no");

      List<Status> statuses = api.GetGroupDetails(group).statuses;
      Assert.AreEqual(2, statuses.Count);
    }

    [Test()]
    public void should_add_comments()
    {
      string group = api.CreateGroup().id;

      api.AddComment(group, "ewatanabe", "first comment");
      api.AddComment(group, "tfernandez", "second comment");

      List<Comment> comments = api.GetGroupDetails(group).comments;
      Assert.AreEqual(2, comments.Count);
    }
  }
}
