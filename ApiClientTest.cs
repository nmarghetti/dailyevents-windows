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
      Result group = api.CreateGroup("Xamarin Test");
      string groupId = group.id;

      Assert.NotNull(groupId);

      string groupCode = api.GetGroupById(group.id).code;
      Assert.NotNull(groupCode);
      
      groupId = api.GetGroupByCode(groupCode).id;
      Assert.NotNull(groupId);
    }

    [Test()]
    public void should_confirm_and_cancel_attendance()
    {
      string group = api.CreateGroup("Xamarin Test").id;

      api.SetStatus("tfernandez-device", group, "tfernandez", "yes");
      api.SetStatus("ewatanabe-device", group, "ewatanabe", "yes");
      api.SetStatus("gliguori-device", group, "gliguori", "no");

      List<Status> statuses = api.GetEvent(group).statuses;
      Assert.AreEqual(2, statuses.Count);
    }

    [Test()]
    public void should_add_comments()
    {
      string group = api.CreateGroup("Xamarin Test").id;

      api.AddComment("ewatanabe-device", group, "ewatanabe", "first comment");
      api.AddComment("tfernandez-device", group, "tfernandez", "second comment");
      api.AddComment("gliguori-device", group, "gliguori", "third comment");

      List<Comment> comments = api.GetEvent(group).comments;
      Assert.AreEqual(3, comments.Count);
    }
  }
}
