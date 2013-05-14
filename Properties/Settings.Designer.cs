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
    [global::System.Configuration.DefaultSettingValueAttribute("")]
    public string ClientId
    {
      get {
        return ((string)(this["ClientId"]));
      }
      set {
        this["ClientId"] = value;
      }
    }

    [global::System.Configuration.UserScopedSettingAttribute()]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Configuration.DefaultSettingValueAttribute("")]
    public string DisplayName
    {
      get {
        return ((string)(this["DisplayName"]));
      }
      set {
        this["DisplayName"] = value;
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
  }
}
