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

    static public int LatestVersion
    {
      get {
        string version = GetInfo()["windows"]["latest_version"];
        int versionNumber = 0;
        Int32.TryParse(version.Replace(".", ""), out versionNumber);
        return versionNumber;
      }
    }

    static public string ApiEntryPoint
    {
      get {
        return GetInfo()["backend_url"] + "/api";
      }
    }
  }
}
