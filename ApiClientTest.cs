using System;
using NUnit.Framework;

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
      Assert.IsTrue(!participants.ContainsKey("gliguori"));
    }

    [Test()]
    public void should_add_comments_to_todays_event()
    {
      api.AddComment(TestGroup, "ewatanabe", "bora?");
      api.AddComment(TestGroup, "tfernandez", "saindo...");

      dynamic comments = api.GetComments(TestGroup);
      Assert.IsTrue(comments.Count >= 2);
    }
  }
}
