using NUnit.Framework;
using System;

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
    public void should_create_a_group()
    {
      Assert.NotNull(api.CreateGroup());
    }

    [Test()]
    public void should_join_and_quit_event()
    {
      string group = api.CreateGroup();

      api.SetStatus(group, "tfernandez", "yes");
      api.SetStatus(group, "ewatanabe", "yes");
      api.SetStatus(group, "gliguori", "no");

      dynamic statuses = api.GetDetails(group)["statuses"];
      Assert.AreEqual(2, statuses.Length);
    }

    [Test()]
    public void should_add_comments_to_event()
    {
      string group = api.CreateGroup();

      api.AddComment(group, "ewatanabe", "bora?");
      api.AddComment(group, "tfernandez", "saindo...");

      dynamic comments = api.GetDetails(group)["comments"];
      Assert.AreEqual(2, comments.Length);
    }
  }
}
