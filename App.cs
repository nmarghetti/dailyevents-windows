using System;
using System.Drawing;
using System.Windows.Forms;

namespace DailyEvents
{
  public class App : Form
  {
    static private readonly string LoggedUser = Environment.UserName;

    static private readonly bool DebugEnabled = false;
    static private readonly short MaxGroups = 5;

    private readonly ApiClient api = new ApiClient();

    private readonly ContextMenu trayMenu = new ContextMenu();
    private NotifyIcon trayIcon;

    [STAThread]
    static public void Main()
    {
      Application.Run(new App());
    }

    public App()
    {
      RebuildTrayMenu();

      if (DebugEnabled)
        SetUpDebug();
      else
        InitTrayIcon();
    }

    private void SetUpDebug()
    {
      Controls.Add(new ToolStrip() {
        ContextMenu = trayMenu
      });
      MouseDown += OnRefreshGroup;
    }

    private void InitTrayIcon()
    {
      trayIcon = new NotifyIcon();
      trayIcon.Text = "Daily Events";
      trayIcon.ContextMenu = trayMenu;
      trayIcon.MouseDown += OnRefreshGroup;
      trayIcon.Visible = true;
      SetAppIcon();
    }

    private void RebuildTrayMenu()
    {
      RebuildTrayMenu(null, null);
    }

    private void RebuildTrayMenu(dynamic participants, dynamic comments)
    {
      if (participants != null || comments != null)
      {
        trayMenu.MenuItems.Clear();
      }
      if (IsCurrentGroupSet())
      {
        trayMenu.MenuItems.Add(Config.Groups[Config.CurrentGroup]);
        trayMenu.MenuItems.Add("-");

        if (participants != null && participants.Count > 0)
        {
          foreach (var participant in participants.Keys)
          {
            trayMenu.MenuItems.Add(participant);
          }
        } else
        {
          trayMenu.MenuItems.Add("Nobody is attending yet");
        }
        trayMenu.MenuItems.Add("-");
        trayMenu.MenuItems.Add("I'm in", OnReplyYes);
        trayMenu.MenuItems.Add("I'm out", OnReplyNo);
        trayMenu.MenuItems.Add("-");
        trayMenu.MenuItems.Add("Add comment", OnNewComment);
        trayMenu.MenuItems.Add("-");

        if (comments != null && comments.Count > 0)
        {
          foreach (var timestamp in comments.Keys)
          {
            dynamic entry = comments [timestamp];
            string user = entry["user"];
            string comment = entry["comment"];
            string localTime = DateUtils.FormatTime(timestamp);
            trayMenu.MenuItems.Add(localTime + " " + user + ": " + comment);
          }
          trayMenu.MenuItems.Add("-");
        }
      }
      else
      {
        trayMenu.MenuItems.Add("No group is set");
        trayMenu.MenuItems.Add("-");
      }
      trayMenu.MenuItems.Add(BuildGroupsMenu());

      trayMenu.MenuItems.Add("-");
      trayMenu.MenuItems.Add("Exit", OnExit);
    }

    MenuItem BuildGroupsMenu()
    {
      MenuItem menu = new MenuItem("Groups");

      dynamic joinedGroups = Config.Groups;
      short numberOfGroupsAdded = 0;

      foreach (var group in joinedGroups)
      {
        if (numberOfGroupsAdded == MaxGroups)
          break;

        MenuItem subMenu = new MenuItem(group.Value, (EventHandler)OnSwitchGroup); // (EventHandler)((sender, e) => {})

        subMenu.MenuItems.Add("Rename", OnRenameGroup);
        subMenu.MenuItems.Add("Invite people", OnInvitePeople);
        subMenu.MenuItems.Add("Leave", OnLeaveGroup);
        
        menu.MenuItems.Add(subMenu);
        // menu.MenuItems.Add(group.Value, (EventHandler)OnSwitchGroup); // (EventHandler)((sender, e) => {})

        numberOfGroupsAdded++;
      }
      if (joinedGroups.Count > 0)
      {
        menu.MenuItems.Add("-");
      }
      menu.MenuItems.Add("Join group", OnJoinGroup);
      menu.MenuItems.Add("Create group", OnCreateGroup);

      return menu;
    }

    protected override void OnLoad(EventArgs e)
    {
      Visible = DebugEnabled;
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
      if (!IsCurrentGroupSet())
      {
        return;
      }
      if (e.Button == MouseButtons.Right)
      {
        try
        {
          SetLoadingIcon();
          dynamic groups = api.GetGroup(Config.CurrentGroup);
          SetAppIcon();

          RebuildTrayMenu(groups["participants"], groups["comments"]);
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
        api.RSVP(Config.CurrentGroup, LoggedUser, "yes");
        SetAppIcon();

        ShowInfo("RSVP", "Attendance confirmed!");
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
        api.RSVP(Config.CurrentGroup, LoggedUser, "no");
        SetAppIcon();

        ShowInfo("RSVP", "Attendance cancelled.");
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
      string comment = Prompt.ShowDialog("Add Comment", "Enter your comment:", 70);

      if (comment.Length > 0)
      {
        try
        {
          SetLoadingIcon();
          api.AddComment(Config.CurrentGroup, LoggedUser, comment);
          SetAppIcon();

          ShowInfo("New Comment", "Comment added!");
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

    private void OnCreateGroup(object sender, EventArgs e)
    {
      if (!CanJoinOrCreateGroups())
        return;
      
      string name = Prompt.ShowDialog("Create Group", "Enter the group's name:", 20);
      
      if (name.Length == 0)
        return;
      
      if (Config.Groups.ContainsValue(name))
      {
        MessageBox.Show("Existing Group", "A group named '" + name + "' already exists, please enter another name.");
        OnCreateGroup(sender, e);
      }
      else
      {
        try
        {
          SetLoadingIcon();
          string code = api.CreateGroup();
          SetAppIcon();

          dynamic groups = Config.Groups;
          groups.Add(code, name);
          
          Config.Groups = groups;
          Config.CurrentGroup = code;
          
          ShowInfo("New Group", name + " created.");
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
    
    private void OnJoinGroup(object sender, EventArgs e)
    {
      if (!CanJoinOrCreateGroups())
        return;
      
      string code = Prompt.ShowDialog("Join Group", "Enter the group's code:", 15);
      
      if (code.Length == 0)
        return;
      
      string name = Prompt.ShowDialog("Join Group", "Enter the group's name:", 20);

      if (name.Length == 0)
        return;
      
      try
      {
        SetLoadingIcon();

        dynamic groups = Config.Groups;
        groups.Add(code, name);
        
        Config.Groups = groups;
        Config.CurrentGroup = code;
        
        ShowInfo("Group", "Joined " + name + "!");
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

    private void OnLeaveGroup(object sender, EventArgs e)
    {
    }

    private void OnRenameGroup(object sender, EventArgs e)
    {
    }
    
    private void OnInvitePeople(object sender, EventArgs e)
    {
    }
    
    private void OnSwitchGroup(object sender, EventArgs e)
    {
      string name = ((MenuItem)sender).Text;
      foreach (var group in Config.Groups)
      {
        if (group.Value == name)
        {
          Config.CurrentGroup = group.Key;
          break;
        }
      }
    }

    private void ShowInfo(string title, string message)
    {
      if (trayIcon != null)
        trayIcon.ShowBalloonTip(10000, title, message, ToolTipIcon.Info);
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
        trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
    }
    
    private void SetLoadingIcon()
    {
      if (trayIcon != null)
        trayIcon.Icon = new Icon(SystemIcons.Question, 40, 40);
    }

    private void OnExit(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private bool CanJoinOrCreateGroups()
    {
      bool maxGroupsReached = Config.Groups.Count >= MaxGroups;
      
      if (maxGroupsReached)
      {
        MessageBox.Show("Max Groups Reached", "Sorry, this client is limited to " + MaxGroups + " groups.");
      }
      return !maxGroupsReached;
    }

    private bool IsCurrentGroupSet()
    {
      string currentGroup = Config.CurrentGroup;
      return currentGroup.Length > 0 && Config.Groups.ContainsKey(currentGroup);
    }
  }
}
