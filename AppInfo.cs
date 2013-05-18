using System;

namespace DailyEvents
{
  static public class AppInfo
  {
    static private dynamic info;
    
    static public bool DevMode()
    {
      return true;
    }

    static private dynamic Get()
    {
      if (info == null)
      {
        string response = new HttpClient(BackendUrl()).Get("meta/apps.info");
        info = Json.Deserialize(response);
      }
      return info;
    }

    static public string BackendUrl()
    {
      string target = "dailyevents";
      if (DevMode()) target += "-dev";
      return "http://" + target + ".parseapp.com/";
    }
    
    static public string ApiEntryPoint
    {
      get {
        return "https://api.parse.com/1/functions/";
      }
    }

    static public string CurrentVersion
    {
      get {
        return "0.9.0"; // Application.ProductVersion
      }
    }

    static public int CurrentVersionNumber
    {
      get {
        string version = CurrentVersion;
        return StringVersionToInt(version);
      }
    }
    
    static public string LatestVersion
    {
      get {
        return Get()["windows"]["latest_version"];
      }
    }

    static public int LatestVersionNumber
    {
      get {
        string version = LatestVersion;
        return StringVersionToInt(version);
      }
    }

    static private int StringVersionToInt(string version)
    {
      int number = 0;
      Int32.TryParse(version.Replace(".", ""), out number);
      return number;
    }
  }
}
