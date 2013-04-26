using System;

namespace DailyEvents
{
  static public class AppInfo
  {
    static private readonly string BaseUrl = "https://dl.dropboxusercontent.com/u/1210246/DailyEvents";

    static private dynamic info;

    static private dynamic GetInfo()
    {
      if (info == null)
      {
        info = new HttpClient(BaseUrl).Get("/app.info");
      }
      return info;
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
        return GetInfo()["windows"]["latest_version"];
      }
    }

    static public int LatestVersionNumber
    {
      get {
        string version = LatestVersion;
        return StringVersionToInt(version);
      }
    }

    static public string ApiEntryPoint
    {
      get {
        return GetInfo()["backend_url"] + "/api";
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
