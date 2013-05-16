using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DailyEvents
{
  public class App : Form
  {
    static private readonly bool SkipTrayIcon = false;

    static private readonly short MaxGroups = 5;

    static private readonly short UsernameMaxLength  = 20;
    static private readonly short GroupNameMaxLength = 30;
    static private readonly short GroupCodeMaxLength = 15;
    static private readonly short CommentMaxLength   = 70;

    private readonly ApiClient api = new ApiClient();

    private readonly ContextMenu trayMenu = new ContextMenu();
    private NotifyIcon trayIcon;

    private bool newVersionBalloonShown = false;

    [STAThread]
    static public void Main()
    {
      Application.Run(new App());
    }

    public App()
    {
      RebuildTrayMenu();

      if (SkipTrayIcon)
        InitToolStrip();
      else
        InitTrayIcon();
    }

    private void InitToolStrip()
    {
      Controls.Add(new ToolStrip() {
        ContextMenu = trayMenu
      });
      MouseMove += OnRefreshGroup;
    }

    private void InitTrayIcon()
    {
      trayIcon = new NotifyIcon();
      trayIcon.Text = "Daily Events";
      trayIcon.ContextMenu = trayMenu;
      trayIcon.MouseDown += OnRefreshGroup;
      trayIcon.Visible = true;
      trayIcon.BalloonTipClicked += OnBalloonTipClicked;

      SetAppIcon();

      if (AppInfo.CurrentVersionNumber < AppInfo.LatestVersionNumber)
      {
        newVersionBalloonShown = true;
        ShowInfo("New version available, grab it while it's hot!");
      }
    }

    private void RebuildTrayMenu()
    {
      RebuildTrayMenu(null, null);
    }

    private void RebuildTrayMenu(List<Status> statuses, List<Comment> comments)
    {
      Invalidate();
      trayMenu.MenuItems.Clear();

      if (IsCurrentGroupSet())
      {
        trayMenu.MenuItems.Add(BuildCurrentGroupMenu());
        trayMenu.MenuItems.Add("Attending today: " + GetSizeOf(statuses));
        trayMenu.MenuItems.Add("-");

        if (statuses != null && statuses.Count > 0)
        {
          foreach (var status in statuses)
          {
            string participant = status.participant;
            trayMenu.MenuItems.Add(participant);
          }
          trayMenu.MenuItems.Add("-");
        }
        trayMenu.MenuItems.Add("I'm in", OnReplyYes);
        trayMenu.MenuItems.Add("I'm out", OnReplyNo);
        trayMenu.MenuItems.Add("-");
        trayMenu.MenuItems.Add("Add comment", OnNewComment);
        trayMenu.MenuItems.Add("-");

        if (comments != null && comments.Count > 0)
        {
          foreach (var comment in comments)
          {
            string participant = comment.participant;
            string commentText = comment.comment;
            string timestamp   = comment.timestamp;
            string timezone    = comment.timezone;
            string localTime   = DateUtils.FormatTime(timestamp, timezone);

            trayMenu.MenuItems.Add(localTime + " " + participant + ": " + commentText);
          }
          trayMenu.MenuItems.Add("-");
        }
      }

      trayMenu.MenuItems.Add(BuildGroupsMenu());
      trayMenu.MenuItems.Add(BuildSettingsMenu());

      trayMenu.MenuItems.Add("-");
      trayMenu.MenuItems.Add("About", OnAbout);
      trayMenu.MenuItems.Add("Exit", OnExit);
    }

    private MenuItem BuildSettingsMenu()
    {
      MenuItem menu = new MenuItem("Settings");
      menu.MenuItems.Add("Change display name", OnChangeDisplayName);
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
        if (numberOfGroupsAdded == MaxGroups)
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
      Visible = SkipTrayIcon;
      ShowInTaskbar = false;
      base.OnLoad(e);
    }

    protected override void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        if (trayIcon != null)
          trayIcon.Dispose();
      }
      base.Dispose(isDisposing);
    }

    private void OnRefreshGroup(object sender, MouseEventArgs e)
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
        api.SetStatus(Settings.ClientId, Settings.CurrentGroup, Settings.DisplayName, "yes");
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
        api.SetStatus(Settings.ClientId, Settings.CurrentGroup, Settings.DisplayName, "no");
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
      string comment = Prompt.Show("Add Comment", "Enter your comment:", CommentMaxLength);

      if (comment.Length > 0)
      {
        try
        {
          SetLoadingIcon();
          api.AddComment(Settings.ClientId, Settings.CurrentGroup, Settings.DisplayName, comment);
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

    private void OnChangeDisplayName(object sender, EventArgs e)
    {
      string name = Prompt.Show("Change Display Name", "Enter the name you want to use from now on:", Settings.DisplayName, UsernameMaxLength);

      if (name.Length == 0)
        return;
      
      Settings.DisplayName = name;

      ShowInfo("Display name changed to \"" + name + "\"");
    }

    private void OnCreateGroup(object sender, EventArgs e)
    {
      if (!CanJoinOrCreateGroups())
        return;
      
      string groupName = Prompt.Show("Create Group", "Enter the group's name:", GroupNameMaxLength);
      
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
      
      string code = Prompt.Show("Join Group", "Enter the code you've received from a group member:", GroupCodeMaxLength);
      
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
      
      string newName = Prompt.Show("Rename Group", "Enter the name you want to use for this group:", groupName, GroupNameMaxLength);
      
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
      bool maxGroupsReached = Settings.Groups.Count >= MaxGroups;
      
      if (maxGroupsReached)
      {
        MessageBox.Show("Sorry, this client is limited to " + MaxGroups + " groups for now.", "Max Groups Reached");
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
      if (trayIcon != null)
        trayIcon.ShowBalloonTip(10000, "", message, ToolTipIcon.None);
    }
    
    private void ShowNetworkError(Exception ex)
    {
      Console.Write(ex);
      
      if (trayIcon != null)
        trayIcon.ShowBalloonTip(10000, "Network Error", "Please wait a bit and try again.", ToolTipIcon.Error);
    }
    
    private void SetAppIcon()
    {
      if (trayIcon != null)
        trayIcon.Icon = new Icon(GetEmbeddedResource("app.ico"));
    }
    
    private void SetLoadingIcon()
    {
      if (trayIcon != null)
        trayIcon.Icon = new Icon(GetEmbeddedResource("app-gray.ico"));
    }

    private void OnBalloonTipClicked(object sender, EventArgs e)
    {
      bool mouseButtonLeft = ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left);

      if (newVersionBalloonShown && mouseButtonLeft)
      {
        string downloadUrl = AppInfo.BackendUrl() + "download/DailyEvents-windows.zip";
        System.Diagnostics.Process.Start(downloadUrl);
        newVersionBalloonShown = false;
      }
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
