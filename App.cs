using System;
using System.Drawing;
using System.Windows.Forms;

namespace DailyEvents
{
  public class App : Form
  {
    private readonly string loggedUser = Environment.UserName;

    private readonly ApiClient api = new ApiClient();

    private NotifyIcon trayIcon;
    private ContextMenu trayMenu;

    [STAThread]
    static public void Main()
    {
      Application.Run(new App());
    }

    public App()
    {
      trayMenu = new ContextMenu();
      trayMenu.MenuItems.Add("Exit", OnExit);
      
      trayIcon = new NotifyIcon();
      trayIcon.Text = "Daily Events";
      trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
      
      trayIcon.ContextMenu = trayMenu;
      trayIcon.Visible = true;
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

    private void OnExit(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void OnReplyYes(object sender, EventArgs e)
    {
      try
      {
        MessageBox.Show("Attendance confirmed!");
      }
      catch (Exception ex)
      {
        ShowError(ex);
      }
    }
    
    private void OnReplyNo(object sender, EventArgs e)
    {
      try
      {
        MessageBox.Show("Attendance cancelled.");
      }
      catch (Exception ex)
      {
        ShowError(ex);
      }
    }

    private void OnNewComment(object sender, EventArgs e)
    {
      try
      {
        MessageBox.Show("Comment added.");
      }
      catch (Exception ex)
      {
        ShowError(ex);
      }
    }

    private void ShowError(Exception ex)
    {
      Console.Write(ex);
      MessageBox.Show("Network error. Please try again.");
    }
  }

  public static class Prompt
  {
    public static string ShowDialog(string text, string caption)
    {
      Form prompt = new Form();
      prompt.Width = 500;
      prompt.Height = 150;
      prompt.Text = caption;

      Label textLabel = new Label() { Left = 50, Top=20, Text=text };
      TextBox textBox = new TextBox() { Left = 50, Top=50, Width=400 };

      Button confirmation = new Button() { Text = "Ok", Left=350, Width=100, Top=70 };
      confirmation.Click += (sender, e) => { prompt.Close(); };

      prompt.Controls.Add(confirmation);
      prompt.Controls.Add(textLabel);
      prompt.Controls.Add(textBox);
      prompt.ShowDialog();

      return textBox.Text;
    }
  }
}
