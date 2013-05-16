using System;
using System.Collections.Generic;

namespace DailyEvents
{
  static public class Settings
  {
    static public string ClientId
    {
      get {
        string id = Properties.Settings.Default.ClientId;
        if (id.Length == 0)
        {
          id = RemoveSpaces(Environment.OSVersion) + "+" +
               RemoveSpaces(Environment.Version) + "::" +
               System.Guid.NewGuid().ToString();

          Properties.Settings.Default.ClientId = id;
          Properties.Settings.Default.Save();
        }
        return id;
      }
    }

    static public string DisplayName
    {
      get {
        string name = Properties.Settings.Default.DisplayName;
        if (name.Length == 0)
        {
          name = Environment.UserName;
          Properties.Settings.Default.DisplayName = name;
          Properties.Settings.Default.Save();
        }
        return name;
      }
      set {
        Properties.Settings.Default.DisplayName = value;
        Properties.Settings.Default.Save();
      }
    }
    
    static public string CurrentGroup
    {
      get {
        return Properties.Settings.Default.CurrentGroup;
      }
      set {
        Properties.Settings.Default.CurrentGroup = value;
        Properties.Settings.Default.Save();
      }
    }
    
    static public string CurrentGroupName
    {
      get {
        string groupId = CurrentGroup;
        dynamic groups = Groups;

        if (groups.ContainsKey(groupId))
          return groups[groupId];
        else
          return null;
      }
    }

    static public dynamic Groups
    {
      get {
        string json = Properties.Settings.Default.Groups;
        return Json.Deserialize(json);
      }
      set {
        string json = Json.Serialize(value);
        Properties.Settings.Default.Groups = json;
        Properties.Settings.Default.Save();
      }
    }
    
    static public dynamic SortedGroups
    {
      get {
        List<string> groups = new List<string>();
        foreach (var group in Groups.Values)
        {
          groups.Add(group);
        }
        groups.Sort();
        return groups;
      }
    }

    static private string RemoveSpaces(dynamic value)
    {
      return value.ToString().Replace(" ", String.Empty);
    }
  }
}