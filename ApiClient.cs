using System;
using System.Collections.Generic;

namespace DailyEvents
{
  public class ApiClient
  {
    private readonly HttpClient http;

    private readonly Dictionary<string, string> developmentHeaders = new Dictionary<string, string>() {
      { "X-Parse-Application-Id", "uI57rIax4Tk31J5dI9EUKR3dCDhaeNphH2D0MmG1" },
      { "X-Parse-REST-API-Key", "kNPRXb7CGw0wkYiK9DtBnGWAtOgdyX6yqQqLMY2X" }
    };
    
    private readonly Dictionary<string, string> productionHeaders = new Dictionary<string, string>() {
      { "X-Parse-Application-Id", "Puuy52CoyWk3c5yOIubf3NPecyNdrNw7h4AAU7Qt" },
      { "X-Parse-REST-API-Key", "eqWvo2PKDxQNnUPvXntTVIg8qYwJFVaPGwVXYtyy" }
    };
    
    public ApiClient()
    {
      this.http = new HttpClient(AppInfo.ApiEntryPoint, developmentHeaders);
    }

    public dynamic CreateGroup()
    {
      dynamic response = http.Post("createGroup");
      return response["result"]["code"];
    }
    
    public dynamic SetStatus(string group, string participant, string reply)
    {
      dynamic response = http.Post("setStatus", RequestParameters(new Dictionary<string, string>() {
        { "group", group }, { "participant", participant }, { "reply", reply }
      }));
      return response;
    }
    
    public dynamic AddComment(string group, string participant, string comment)
    {
      dynamic response = http.Post("addComment", RequestParameters(new Dictionary<string, string>() {
        { "group", group }, { "participant", participant }, { "comment", comment }
      }));
      return response;
    }

    public dynamic GetDetails(string group)
    {
      dynamic response = http.Post("getDetails", RequestParameters(new Dictionary<string, string>() {
        { "group", group }
      }));
      return response["result"];
    }
    
    private dynamic RequestParameters(Dictionary<string, string> requestParams)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>() {
        { "timestamp", DateUtils.CurrentTimeMillis() },
        { "timezone", "-120" } // TODO Fix this
      };
      foreach (var requestParam in requestParams)
      {
        parameters.Add(requestParam.Key, requestParam.Value);
      }
      return parameters;
    }
  }
}
