using System;
using System.Drawing;
using System.Windows.Forms;

namespace DailyEvents
{
  public class App : Form
  {
    static private readonly string LoggedUser = Environment.UserName;

    static private readonly bool DebugEnabled = true;

    static private readonly short MaxGroups = 5;
    static private readonly short GroupNameMaxLength = 30;
    static private readonly short GroupCodeMaxLength = 15;
    static private readonly short CommentMaxLength = 70;

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
        trayMenu.MenuItems.Add(GetCurrentGroupName());
        trayMenu.MenuItems.Add("-");

        if (participants != null && participants.Count > 0)
        {
          foreach (var participant in participants.Keys)
          {
            trayMenu.MenuItems.Add(participant);
          }
        } else
        {
          trayMenu.MenuItems.Add("(nobody's attending yet)");
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
        trayMenu.MenuItems.Add("(no group is set)");
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

        // menu.MenuItems.Add(group.Value, (EventHandler)OnSwitchGroup); // (EventHandler)((sender, e) => {})

        MenuItem subMenu = new MenuItem(group.Value);

        subMenu.MenuItems.Add("Switch to", OnSwitchToGroup);
        subMenu.MenuItems.Add("Invite people", OnInvitePeople);
        subMenu.MenuItems.Add("Rename group", OnRenameGroup);
        subMenu.MenuItems.Add("Leave group", OnLeaveGroup);
        
        menu.MenuItems.Add(subMenu);

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

        ShowInfo("Attendance confirmed!");
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

        ShowInfo("Attendance cancelled.");
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
      string comment = Prompt.ShowDialog("Add Comment", "Enter your comment:", CommentMaxLength);

      if (comment.Length > 0)
      {
        try
        {
          SetLoadingIcon();
          api.AddComment(Config.CurrentGroup, LoggedUser, comment);
          SetAppIcon();

          ShowInfo("Comment added!");
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
      
      string name = Prompt.ShowDialog("Create Group", "Enter the group's name:", GroupNameMaxLength);
      
      if (name.Length == 0)
        return;
      
      if (Config.Groups.ContainsValue(name))
      {
        MessageBox.Show("A group named '" + name + "' already exists, please enter another name.", "Existing Group");
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
          groups[code] = name;
          
          Config.Groups = groups;
          Config.CurrentGroup = code;
          
          ShowInfo("Group created.");
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
      
      string code = Prompt.ShowDialog("Join Group", "Group code:", GroupCodeMaxLength);
      
      if (code.Length == 0)
        return;
      
      string name = Prompt.ShowDialog("Join Group", "Group name:", GroupNameMaxLength);

      if (name.Length == 0)
        return;

      if (Config.Groups.ContainsValue(name))
      {
        MessageBox.Show("A group named '" + name + "' already exists, please enter another name.", "Existing Group");
        OnJoinGroup(sender, e);
      }
      else
      {
        try
        {
          SetLoadingIcon();

          dynamic groups = Config.Groups;
          groups [code] = name;
          
          Config.Groups = groups;
          Config.CurrentGroup = code;
          
          ShowInfo("Joined existing group!");
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

    private void OnSwitchToGroup(object sender, EventArgs e)
    {
      String name = GetParentMenuText(sender);
      Config.CurrentGroup = GetGroupCode(name);

      ShowInfo("Switched group.");
    }

    private void OnInvitePeople(object sender, EventArgs e)
    {
      String name = GetParentMenuText(sender);
      string code = GetGroupCode(name);

      Clipboard.SetText(code);

      MessageBox.Show("Send this code to guests: " + code + "\n\nThe code was just copied to your clipboard.", "Invite People");
    }

    private void OnRenameGroup(object sender, EventArgs e)
    {
      String currentName = GetParentMenuText(sender);
      string code = GetGroupCode(currentName);
      
      string newName = Prompt.ShowDialog("Rename Group", "Enter the group's new name:", GroupNameMaxLength);
      
      dynamic groups = Config.Groups;
      groups[code] = newName;
      
      Config.Groups = groups;

      ShowInfo("Group renamed.");
    }

    private void OnLeaveGroup(object sender, EventArgs e)
    {
      String name = GetParentMenuText(sender);
      string code = GetGroupCode(name);

      dynamic groups = Config.Groups;
      groups.Remove(code);
      
      Config.Groups = groups;

      if (Config.CurrentGroup == code)
        Config.CurrentGroup = "";

      ShowInfo("Group left.");
    }

    private string GetParentMenuText(object sender)
    {
      return ((MenuItem)((MenuItem)sender).Parent).Text;
    }

    private bool CanJoinOrCreateGroups()
    {
      bool maxGroupsReached = Config.Groups.Count >= MaxGroups;
      
      if (maxGroupsReached)
      {
        MessageBox.Show("Sorry, this client is limited to " + MaxGroups + " groups.", "Max Groups Reached");
      }
      return !maxGroupsReached;
    }

    private bool IsCurrentGroupSet()
    {
      string currentGroup = Config.CurrentGroup;
      return currentGroup.Length > 0 && Config.Groups.ContainsKey(currentGroup);
    }

    private string GetCurrentGroupName()
    {
      return Config.Groups[Config.CurrentGroup];
    }

    private string GetGroupCode(string name)
    {
      foreach (var group in Config.Groups)
      {
        if (group.Value == name)
        {
          return group.Key;
        }
      }
      throw new ApplicationException("Group not found: " + name);
    }

    private void ShowInfo(string message)
    {
      if (trayIcon != null)
        trayIcon.ShowBalloonTip(10000, "Info", message, ToolTipIcon.Info);
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
  }
}
