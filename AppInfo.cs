using System;

namespace DailyEvents
{
  static public class AppInfo
  {
    static private dynamic Metadata;

    static AppInfo()
    {
      string response = new HttpClient(BackendUrl(), ApiClient.GetApiHeaders()).Get("meta/apps.info");
      Metadata = Json.Deserialize(response);
    }
    
    static public bool DevMode()
    {
      return false;
    }

    static public string BackendUrl()
    {
      return "https://parseapi.back4app.com/";
    }

    static public string MarketingUrl()
    {
      return Metadata["marketing_url"];
    }

    static public string OnlineUrl(string group, string name)
    {
      return Metadata["marketing_url"] + "mobile/?code=" + group + "&name=" + name;
    }

    static public string DonationUrl()
    {
      return Metadata["donation_url"];
    }

    static public string FeedbackUrl()
    {
      return Metadata["feedback_url"];
    }
    
    static public string ApiEntryPoint
    {
      get {
        return "https://parseapi.back4app.com/functions/";
      }
    }

    static public string CurrentVersion
    {
      get {
        return "1.0.6"; // Application.ProductVersion
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
        return Metadata["windows"]["latest_version"];
      }
    }

    static public int LatestVersionNumber
    {
      get {
        string version = LatestVersion;
        return StringVersionToInt(version);
      }
    }

    static public bool IsOutdated
    {
      get {
        return AppInfo.CurrentVersionNumber < AppInfo.LatestVersionNumber;
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
