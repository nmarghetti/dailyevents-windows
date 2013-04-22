namespace DailyEvents.Properties {
  
  internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
  {
    
    static private Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
    
    static public Settings Default
    {
      get {
        return defaultInstance;
      }
    }

    [global::System.Configuration.UserScopedSettingAttribute()]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Configuration.DefaultSettingValueAttribute("http://dailyevents.cloudfoundry.com/api")]
    public string ApiEntryPoint
    {
      get {
        return ((string)(this["ApiEntryPoint"]));
      }
      set {
        this["ApiEntryPoint"] = value;
      }
    }
    
    [global::System.Configuration.UserScopedSettingAttribute()]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Configuration.DefaultSettingValueAttribute("{}")]
    public string Groups
    {
      get {
        return ((string)(this["Groups"]));
      }
      set {
        this["Groups"] = value;
      }
    }
    
    [global::System.Configuration.UserScopedSettingAttribute()]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Configuration.DefaultSettingValueAttribute("")]
    public string CurrentGroup
    {
      get {
        return ((string)(this["CurrentGroup"]));
      }
      set {
        this["CurrentGroup"] = value;
      }
    }
  }
}
