using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace DailyEvents
{
  public class ApiClient
  {
    private readonly Dictionary<string, string> customHeaders = new Dictionary<string, string>() {
      /* Development */
      { "X-Parse-Application-Id", "uI57rIax4Tk31J5dI9EUKR3dCDhaeNphH2D0MmG1" },
      { "X-Parse-REST-API-Key", "kNPRXb7CGw0wkYiK9DtBnGWAtOgdyX6yqQqLMY2X" }

      /* Production
      { "X-Parse-Application-Id", "Puuy52CoyWk3c5yOIubf3NPecyNdrNw7h4AAU7Qt" },
      { "X-Parse-REST-API-Key", "eqWvo2PKDxQNnUPvXntTVIg8qYwJFVaPGwVXYtyy" } */
    };

    private readonly HttpClient http;

    public ApiClient()
    {
      this.http = new HttpClient(AppInfo.ApiEntryPoint, customHeaders);
    }

    public Result CreateGroup()
    {
      Result result = CallFunction("createGroup");
      return result;
    }

    public Result SetStatus(string group, string participant, string reply)
    {
      Result result = CallFunction("setStatus", TemporalParameters(new Dictionary<string, string>() {
        { "group", group }, { "participant", participant }, { "reply", reply }
      }));
      return result;
    }
    
    public Result AddComment(string group, string participant, string comment)
    {
      Result result = CallFunction("addComment", TemporalParameters(new Dictionary<string, string>() {
        { "group", group }, { "participant", participant }, { "comment", comment }
      }));
      return result;
    }

    public Result GetGroup(string id)
    {
      Result result = CallFunction("getGroup", new Dictionary<string, string>() {
        { "group", id }
      });
      return result;
    }

    public Result GetEvent(string group)
    {
      Result result = CallFunction("getEvent", TemporalParameters(new Dictionary<string, string>() {
        { "group", group }
      }));
      List<Status> statuses = result.statuses;
      foreach (Status status in statuses.FindAll(s => s.reply == "no"))
      {
        statuses.Remove(status);
      }
      return result;
    }

    private Result CallFunction(string path)
    {
      return CallFunction(path, new Dictionary<string, string>());
    }

    private Result CallFunction(string name, Dictionary<string, string> parameters)
    {
      string response = http.Post(name, parameters);
      return Deserialize(response);
    }

    private dynamic TemporalParameters(Dictionary<string, string> requestParams)
    {
      string timestamp = DateUtils.CurrentTimeMillis();
      string timezone  = DateUtils.GetUtcOffsetInMinutes(timestamp);

      Dictionary<string, string> parameters = new Dictionary<string, string>() {
        { "timestamp", timestamp },
        { "timezone", timezone }
      };
      foreach (var requestParam in requestParams)
      {
        parameters.Add(requestParam.Key, requestParam.Value);
      }
      return parameters;
    }

    private Result Deserialize(string json)
    {
      return Json.Deserialize<RootObject>(json).result;
    }
  }

  // cf. http://json2csharp.com/
  
  public class Status
  {
    public string participant { get; set; }
    public string reply { get; set; }
    public string timestamp { get; set; }
    public string timezone { get; set; }
  }
  
  public class Comment
  {
    public string participant { get; set; }
    public string comment { get; set; }
    public string timestamp { get; set; }
    public string timezone { get; set; }
  }
  
  public class Result
  {
    public string id { get; set; }
    public string code { get; set; }
    
    public List<Status> statuses { get; set; }
    public List<Comment> comments { get; set; }
  }
  
  public class RootObject
  {
    public Result result { get; set; }
  }
}
