using NUnit.Framework;
using System;

namespace DailyEvents
{
  [TestFixture()]
  public class ApiClientTest
  {
    private static readonly string TestGroup = "BR4ZUC4S";

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
    public void should_join_and_quit_todays_event()
    {
      api.RSVP(TestGroup, "tfernandez", "yes");
      api.RSVP(TestGroup, "ewatanabe", "yes");
      api.RSVP(TestGroup, "gliguori", "no");

      dynamic participants = api.GetParticipants(TestGroup);

      Assert.AreEqual(2, participants.Count);
      Assert.IsTrue(participants.ContainsKey("tfernandez"));
      Assert.IsTrue(participants.ContainsKey("ewatanabe"));
    }

    [Test()]
    public void should_add_comments_to_todays_event()
    {
      api.AddComment(TestGroup, "ewatanabe", "bora?");
      api.AddComment(TestGroup, "tfernandez", "saindo...");

      dynamic comments = api.GetComments(TestGroup);
      Assert.IsTrue(comments.Count >= 2);
    }

    [Test()]
    public void should_get_participants_and_comments_for_todays_event()
    {
      dynamic group = api.GetGroup(TestGroup);
      Assert.AreEqual(2, group.Count);
      Assert.AreEqual(2, group["participants"].Count);
      Assert.IsTrue(group["comments"].Count >= 2);
    }
  }
}
