using System;
using System.Collections.Generic;

namespace DailyEvents
{
  public class ApiClient
  {
    private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly HttpClient http;

    public ApiClient()
    {
      this.http = new HttpClient("http://dailyevents.cloudfoundry.com/api");
    }

    public dynamic CreateGroup()
    {
      dynamic result = http.Post("/groups");
      return result["id"];
    }

    public dynamic RSVP(string group, string user, string reply)
    {
      string url = CreateUrl("/rsvp/", group);
      return http.Post(url, new Dictionary<string, string>() {
        { "user", user }, { "reply", reply }
      });
    }

    public dynamic GetParticipants(string group)
    {
      string url = CreateUrl("/participants/", group);
      return http.Get(url);
    }

    public dynamic AddComment(string group, string user, string comment)
    {
      string url = CreateUrl("/comments/", group);
      return http.Post(url, new Dictionary<string, string>() {
        { "user", user }, { "comment", comment }
      });
    }
    
    public dynamic GetComments(string group)
    {
      string url = CreateUrl("/comments/", group);
      return http.Get(url);
    }

    private string CreateUrl(string path, string group)
    {
      return path + group + "?timestamp=" + CurrentTimeMillis();
    }

    private static string CurrentTimeMillis()
    {
      return (DateTime.UtcNow - Jan1st1970).TotalMilliseconds.ToString();
    }
  }
}
