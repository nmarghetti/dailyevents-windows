using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace DailyEvents
{
  public class ApiClient
  {
    private static readonly Dictionary<string, string> developmentHeaders = new Dictionary<string, string>() {
      { "X-Parse-Application-Id", "ibFaVE62P4GmVgaEezNPwIVF8HAvsRZfAsEkm21V" },
      { "X-Parse-REST-API-Key", "rT0k9doFKkHByDuHluidcFjntmDLEWMpX3KlcmGC" }
    };

    private static readonly Dictionary<string, string> productionHeaders = new Dictionary<string, string>() {
      { "X-Parse-Application-Id", "OM6MTeOzj8Y5exBdXTCgCB97JqY1KaVOCFjEvcXJ" },
      { "X-Parse-REST-API-Key", "vLVQmSP1iKBVK5XyiAmXiROD0VfvW7SjT5afwcn9" }
    };

    public static Dictionary<string, string> GetApiHeaders()
    {
      return AppInfo.DevMode() ? developmentHeaders : productionHeaders;
    }

    private HttpClient http;

    public ApiClient()
    {
      InitHttpClient();
      GetClientId();
    }

    private void InitHttpClient()
    {
      var headers = AppInfo.DevMode() ? developmentHeaders : productionHeaders;
      this.http = new HttpClient(AppInfo.ApiEntryPoint, headers);
    }

    private string GetClientId()
    {
      if (Settings.ClientId.Length == 0)
      {
        Settings.ClientId = CallFunction("register", new Dictionary<string, string>() {
          { "environment", Environment.OSVersion + " .NET " + Environment.Version }
        }).id;
      }
      return Settings.ClientId;
    }

    public Result CreateGroup(string name)
    {
      Result result = CallFunction("createGroup", new Dictionary<string, string>() {
        { "name", name }
      });
      return result;
    }

    public Result GetGroupById(string id)
    {
      Result result = CallFunction("getGroupById", new Dictionary<string, string>() {
        { "id", id }
      });
      return result;
    }
    
    public Result GetGroupByCode(string code)
    {
      Result result = CallFunction("getGroupByCode", new Dictionary<string, string>() {
        { "code", code }
      });
      return result;
    }

    public Result SetStatus(string groupId, string participant, string reply)
    {
      Result result = CallFunction("setStatus", TimeParameters(new Dictionary<string, string>() {
        { "clientId", GetClientId() }, { "groupId", groupId }, { "participant", participant }, { "reply", reply }
      }));
      return result;
    }
    
    public Result AddComment(string groupId, string participant, string comment)
    {
      Result result = CallFunction("addComment", TimeParameters(new Dictionary<string, string>() {
        { "clientId", GetClientId() }, { "groupId", groupId }, { "participant", participant }, { "comment", comment }
      }));
      return result;
    }

    public Result GetEvent(string groupId)
    {
      Result result = CallFunction("getEvent", TimeParameters(new Dictionary<string, string>() {
        { "groupId", groupId }
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

    private dynamic TimeParameters(Dictionary<string, string> requestParams)
    {
      string timestamp = DateUtils.CurrentTimeMillis();
      string timezone  = DateUtils.GetUtcOffsetInMinutes(timestamp);

      Dictionary<string, string> parameters = new Dictionary<string, string>() {
        { "timestamp", timestamp },
        { "timezone", timezone }
      };
      foreach (var param in requestParams)
      {
        parameters.Add(param.Key, param.Value);
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
    public string name { get; set; }

    public List<Status> statuses { get; set; }
    public List<Comment> comments { get; set; }
  }
  
  public class RootObject
  {
    public Result result { get; set; }
  }
}
