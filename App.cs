using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DailyEvents
{
  public class App : Form
  {
    private readonly ApiClient api = new ApiClient();

    private readonly NotifyIcon trayIcon = new NotifyIcon();

    [STAThread]
    static public void Main(string[] args)
    {
      //Properties.Settings.Default.Reset();
      Application.Run(new App());
    }

    public App()
    {
      InitTrayIcon();
      RebuildTrayMenu();
    }

    private void InitTrayIcon()
    {
      trayIcon.Text = "Daily Events";
      trayIcon.Visible = true;
      SetAppIcon();
    }

    private void RebuildTrayMenu()
    {
      RebuildTrayMenu(null, null);
    }

    private void RebuildTrayMenu(List<Status> statuses, List<Comment> comments)
    {
      trayIcon.ContextMenu = new ContextMenu();
      trayIcon.ContextMenu.Popup += OnRefreshGroup;

      MenuItem.MenuItemCollection menuItems = trayIcon.ContextMenu.MenuItems;

      if (IsCurrentGroupSet())
      {
        menuItems.Add(BuildCurrentGroupMenu());
        menuItems.Add("Attending today: " + GetSizeOf(statuses));
        menuItems.Add("-");

        if (statuses != null && statuses.Count > 0)
        {
          foreach (var status in statuses)
          {
            string participant = status.participant;
            menuItems.Add(participant);
          }
          menuItems.Add("-");
        }
        // http://en.wikipedia.org/wiki/Western_Latin_character_sets_(computing)

        menuItems.Add("\u221A  I'm in", OnReplyYes);
        menuItems.Add("\u00D7  I'm out", OnReplyNo);
        menuItems.Add("-");
        menuItems.Add("\u002B Add comment", OnNewComment);
        menuItems.Add("-");

        if (comments != null && comments.Count > 0)
        {
          foreach (var comment in comments)
          {
            string participant = comment.participant;
            string commentText = comment.comment;
            string timestamp   = comment.timestamp;
            string timezone    = comment.timezone;
            string localTime   = DateUtils.FormatTime(timestamp, timezone);

            string commentLabel = localTime + " " + participant + ": " + commentText;
            menuItems.Add(commentLabel, (EventHandler) OnExistingComment);
          }
          menuItems.Add("-");
        }
      }

      menuItems.Add(BuildGroupsMenu());
      menuItems.Add(BuildSettingsMenu());

      menuItems.Add("-");
      menuItems.Add("About", OnAbout);
      menuItems.Add("Exit", OnExit);
    }

    private MenuItem BuildSettingsMenu()
    {
      MenuItem menu = new MenuItem("Settings" + (AppInfo.IsOutdated ? " (!)" : ""));
      menu.MenuItems.Add("Change display name", OnChangeDisplayName);

      if (AppInfo.IsOutdated)
      {
        menu.MenuItems.Add("Download new version (!)", OnDownloadNewVersion);
      }
      return menu;
    }

    private MenuItem BuildCurrentGroupMenu()
    {
      MenuItem menu = new MenuItem(Settings.CurrentGroupName);
      menu.MenuItems.Add("Invite people", OnInvitePeople);
      menu.MenuItems.Add("Online version", OnOnlineVersion);
      menu.MenuItems.Add("Rename group", OnRenameGroup);
      menu.MenuItems.Add("Leave group", OnLeaveGroup);
      return menu;
    }

    private MenuItem BuildGroupsMenu()
    {
      MenuItem menu = new MenuItem("Groups");

      dynamic groups = Settings.SortedGroups;
      short numberOfGroupsAdded = 0;

      string currentGroup = Settings.CurrentGroupName;

      foreach (var group in groups)
      {
        if (numberOfGroupsAdded == Constraints.MaxGroups)
          break;

        if (group != currentGroup)
        {
          menu.MenuItems.Add(group, (EventHandler) OnSwitchToGroup); // (EventHandler)((sender, e) => {})
        }
        numberOfGroupsAdded++;
      }
      if (menu.MenuItems.Count > 0)
      {
        menu.MenuItems.Add("-");
      }
      menu.MenuItems.Add("Create group", OnCreateGroup);
      menu.MenuItems.Add("Join group", OnJoinGroup);

      return menu;
    }

    protected override void OnLoad(EventArgs e)
    {
      Visible = false;
      ShowInTaskbar = false;
      base.OnLoad(e);
    }

    protected override void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        trayIcon.Dispose();
      }
      base.Dispose(isDisposing);
    }

    private void OnRefreshGroup(object sender, EventArgs e)
    {
      // if (e.Button == MouseButtons.Right)

      try
      {
        if (IsCurrentGroupSet())
        {
          SetLoadingIcon();
          Result result = api.GetEvent(Settings.CurrentGroup);
          RebuildTrayMenu(result.statuses, result.comments);
        }
        else
        {
          RebuildTrayMenu();
        }
      }
      catch (Exception ex)
      {
        ShowNetworkError(ex);
      }
      finally
      {
        SetAppIcon();
      }
    }

    private void OnReplyYes(object sender, EventArgs e)
    {
      try
      {
        SetLoadingIcon();
        api.SetStatus(Settings.CurrentGroup, Settings.DisplayName, "yes");
        ShowInfo("Attendance confirmed");
      }
      catch (Exception ex)
      {
        ShowNetworkError(ex);
      }
      finally
      {
        SetAppIcon();
      }
    }
    
    private void OnReplyNo(object sender, EventArgs e)
    {
      try
      {
        SetLoadingIcon();
        api.SetStatus(Settings.CurrentGroup, Settings.DisplayName, "no");
        ShowInfo("Attendance cancelled");
      }
      catch (Exception ex)
      {
        ShowNetworkError(ex);
      }
      finally
      {
        SetAppIcon();
      }
    }

    private void OnNewComment(object sender, EventArgs e)
    {
      string comment = Prompt.Show("Add Comment", "Enter your comment:", Constraints.CommentMaxLength);

      if (comment.Length > 0)
      {
        try
        {
          SetLoadingIcon();
          api.AddComment(Settings.CurrentGroup, Settings.DisplayName, comment);
          ShowInfo("Comment added");
        }
        catch (Exception ex)
        {
          ShowNetworkError(ex);
        }
        finally
        {
          SetAppIcon();
        }
      }
    }

    private void OnExistingComment(object sender, EventArgs e)
    {
      /*
      \b       -matches a word boundary (spaces, periods..etc)
      (?:      -define the beginning of a group, the ?: specifies not to specifically capture the data within this group.
      http://  -literal string, match http://
      https://  -literal string, match https://
      |        -OR
      www\.    -literal string, match www. (the \. means a literal ".")
      )        -end group
      \S+      -match a series of non-whitespace characters.
      \b       -match the closing word boundary.
      */
      Regex linkParser = new Regex(
        @"\b(?:http://|https://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase
      );
      string comment = GetCurrentMenuText(sender);

      foreach(Match match in linkParser.Matches(comment))
        System.Diagnostics.Process.Start(match.Value);
    }

    private void OnChangeDisplayName(object sender, EventArgs e)
    {
      string name = Prompt.Show(
        "Change Display Name", "Enter the name you want to use from now on:",
        Settings.DisplayName, Constraints.UsernameMaxLength
      );

      if (name.Length == 0)
        return;
      
      Settings.DisplayName = name;

      ShowInfo("Display name changed to \"" + name + "\"");
    }

    private void OnDownloadNewVersion(object sender, EventArgs e)
    {
      // bool mouseButtonLeft = ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left);

      string downloadUrl = AppInfo.MarketingUrl() + "/download/DailyEvents-windows.zip";
      System.Diagnostics.Process.Start(downloadUrl);
    }

    private void OnCreateGroup(object sender, EventArgs e)
    {
      if (!CanJoinOrCreateGroups())
        return;
      
      string groupName = Prompt.Show(
        "Create Group", "Enter the group's name:",
        Constraints.GroupNameMaxLength
      );
      
      if (groupName.Length == 0)
        return;
      
      try
      {
        SetLoadingIcon();
        string id = api.CreateGroup(groupName).id;

        dynamic groups = Settings.Groups;
        groups[id] = groupName;
        
        Settings.Groups = groups;
        Settings.CurrentGroup = id;
        
        ShowInfo("Created \"" + groupName + "\"");
      }
      catch (Exception ex)
      {
        ShowNetworkError(ex);
      }
      finally
      {
        SetAppIcon();
      }
    }
    
    private void OnJoinGroup(object sender, EventArgs e)
    {
      if (!CanJoinOrCreateGroups())
        return;
      
      string code = Prompt.Show(
        "Join Group", "Enter the code you've received from a group member:",
        Constraints.GroupCodeMaxLength
      );
      
      if (code.Length == 0)
        return;
      
      try
      {
        SetLoadingIcon();

        Result group     = api.GetGroupByCode(code);
        string groupId   = group.id;
        string groupName = group.name;

        if (groupId == null) {
          MessageBox.Show("The entered code is invalid, please try again.", "Invalid Code");
        }
        else {
          dynamic groups  = Settings.Groups;
          groups[groupId] = groupName;
          
          Settings.Groups = groups;
          Settings.CurrentGroup = groupId;
          
          ShowInfo("Joined \"" + groupName + "\"");
        }
      }
      catch (Exception ex)
      {
        ShowNetworkError(ex);
      }
      finally
      {
        SetAppIcon();
      }
    }

    private void OnSwitchToGroup(object sender, EventArgs e)
    {
      string groupName = GetCurrentMenuText(sender);

      foreach (var group in Settings.Groups)
      {
        if (group.Value == groupName)
        {
          Settings.CurrentGroup = group.Key;
          break;
        }
      }
      ShowInfo("Switched to \"" + groupName + "\"");
    }

    private void OnInvitePeople(object sender, EventArgs e)
    {
      try
      {
        SetLoadingIcon();

        string groupId = Settings.CurrentGroup;
        string groupCode = api.GetGroupById(groupId).code;

        Clipboard.SetText(groupCode);
        MessageBox.Show("Send this code to guests: " + groupCode + ".\nThe code was just copied to your clipboard.", "Invite People");
      }
      catch (Exception ex)
      {
        ShowNetworkError(ex);
      }
      finally
      {
        SetAppIcon();
      }
    }

    private void OnOnlineVersion(object sender, EventArgs e)
    {
      try
      {
        SetLoadingIcon();

        string groupId = Settings.CurrentGroup;
        string groupCode = api.GetGroupById(groupId).code;

        Clipboard.SetText(groupCode);
        Website.Show(groupCode, Settings.DisplayName);
      }
      catch (Exception ex)
      {
        ShowNetworkError(ex);
      }
      finally
      {
        SetAppIcon();
      }
     
    }
    
    private void OnRenameGroup(object sender, EventArgs e)
    {
      string groupName = GetParentMenuText(sender);
      string groupId   = Settings.CurrentGroup;
      
      string newName = Prompt.Show(
        "Rename Group", "Enter the name you want to use for this group:",
        groupName, Constraints.GroupNameMaxLength
      );
      
      if (newName.Length == 0)
        return;

      dynamic groups  = Settings.Groups;
      groups[groupId] = newName;
      
      Settings.Groups = groups;

      ShowInfo("Group renamed to \"" + newName + "\"");
    }

    private void OnLeaveGroup(object sender, EventArgs e)
    {
      string groupName = GetParentMenuText(sender);
      string groupId   = Settings.CurrentGroup;

      dynamic groups = Settings.Groups;
      groups.Remove(groupId);
      
      Settings.Groups = groups;

      foreach (var key in groups.Keys)
      {
        Settings.CurrentGroup = key;
        break;
      }
      ShowInfo("Left \"" + groupName + "\"");
    }
    
    private string GetCurrentMenuText(object sender)
    {
      return ((MenuItem)sender).Text;
    }

    private string GetParentMenuText(object sender)
    {
      return ((MenuItem)((MenuItem)sender).Parent).Text;
    }

    private bool CanJoinOrCreateGroups()
    {
      bool maxGroupsReached = Settings.Groups.Count >= Constraints.MaxGroups;
      
      if (maxGroupsReached)
      {
        MessageBox.Show("Sorry, this client is limited to " + Constraints.MaxGroups + " groups for now.", "Max Groups Reached");
      }
      return !maxGroupsReached;
    }

    private bool IsCurrentGroupSet()
    {
      string currentGroup = Settings.CurrentGroup;
      return currentGroup.Length > 0 && Settings.Groups.ContainsKey(currentGroup);
    }

    private void ShowInfo(string message)
    {
      trayIcon.ShowBalloonTip(10000, "", message, ToolTipIcon.None);
    }
    
    private void ShowNetworkError(Exception ex)
    {
      Console.Write(ex);
      
      trayIcon.ShowBalloonTip(10000, "Network Error", "Please wait a bit and try again.", ToolTipIcon.Error);
    }
    
    private void SetAppIcon()
    {
      trayIcon.Icon = new Icon(GetEmbeddedResource("app-blue.ico"));
    }
    
    private void SetLoadingIcon()
    {
      trayIcon.Icon = new Icon(GetEmbeddedResource("app-gray.ico"));
    }

    private void OnAbout(object sender, EventArgs e)
    {
      About.Show();
    }

    private void OnExit(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private System.IO.Stream GetEmbeddedResource(string name)
    {
      System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
      return assembly.GetManifestResourceStream(name);
    }

    private int GetSizeOf(dynamic list)
    {
      return list != null ? list.Count : 0;
    }
  }
}
