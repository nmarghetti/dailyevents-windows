using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DailyEvents
{
  public class App : Form
  {
    private readonly ApiClient api = new ApiClient();

    private readonly NotifyIcon trayIcon = new NotifyIcon();

    [STAThread]
    static public void Main()
    {
      Application.Run(new App());
    }

    public App()
    {
      InitTrayIcon();
      RebuildTrayMenu();
    }

    private void InitToolStrip()
    {
      Controls.Add(new ToolStrip() {
        ContextMenu = new ContextMenu()
      });
      MouseMove += OnRefreshGroup;
    }

    private void InitTrayIcon()
    {
      trayIcon.Text = "Daily Events";
      trayIcon.MouseDown += OnRefreshGroup;
      trayIcon.Visible = true;

      SetAppIcon();
    }

    private void RebuildTrayMenu()
    {
      RebuildTrayMenu(null, null);
    }

    private void RebuildTrayMenu(List<Status> statuses, List<Comment> comments)
    {
      ContextMenu menu = new ContextMenu();

      if (IsCurrentGroupSet())
      {
        menu.MenuItems.Add(BuildCurrentGroupMenu());
        menu.MenuItems.Add("Attending today: " + GetSizeOf(statuses));
        menu.MenuItems.Add("-");

        if (statuses != null && statuses.Count > 0)
        {
          foreach (var status in statuses)
          {
            string participant = status.participant;
            menu.MenuItems.Add(participant);
          }
          menu.MenuItems.Add("-");
        }
        // http://en.wikipedia.org/wiki/Western_Latin_character_sets_(computing)

        menu.MenuItems.Add("\u221A  I'm in", OnReplyYes);
        menu.MenuItems.Add("\u00D7  I'm out", OnReplyNo);
        menu.MenuItems.Add("-");
        menu.MenuItems.Add("Add comment", OnNewComment);
        menu.MenuItems.Add("-");

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
            menu.MenuItems.Add(commentLabel, (EventHandler) OnExistingComment);
          }
          menu.MenuItems.Add("-");
        }
      }

      menu.MenuItems.Add(BuildGroupsMenu());
      menu.MenuItems.Add(BuildSettingsMenu());

      menu.MenuItems.Add("-");
      menu.MenuItems.Add("About", OnAbout);
      menu.MenuItems.Add("Exit", OnExit);

      trayIcon.ContextMenu = menu;

      Refresh();
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
          menu.MenuItems.Add(group, (EventHandler)OnSwitchToGroup); // (EventHandler)((sender, e) => {})
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

    private void OnRefreshGroup(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
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
      string comment = GetCurrentMenuText(sender);
      Clipboard.SetText(comment);
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
