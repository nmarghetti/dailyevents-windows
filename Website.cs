using System;
using System.Drawing;
using System.Windows.Forms;

namespace DailyEvents
{
  static public class Website
  {
    static public void Show(string group, string name)
    {
      Form dialog = new Form()
      {
        StartPosition   = FormStartPosition.CenterScreen,
        FormBorderStyle = FormBorderStyle.FixedSingle,
        MaximizeBox     = false,
        Width           = 410,
        Height          = 200,
        Text            = "Online version"
      };

      RichTextBox textBox = new RichTextBox()
      {
        Left = 15,
        Top = 20,
        Width = 360,
        Height = 100,
        ReadOnly = true,
        BackColor = Color.LightGray,
        Text = "You can use the following url to check this group online:\n" + AppInfo.OnlineUrl(group, name) + "\n\nOr click on the \"Online version\" button below for direct access."
      };

      Button websiteButton = new Button()
      {
        Left  = 140,
        Width = 120,
        Top   = 130,
        Text  = "Online version"
      };
      websiteButton.Click += (object sender, EventArgs e) =>
      {
        System.Diagnostics.Process.Start(AppInfo.OnlineUrl(group, name));
        dialog.Close();
      };

      Button closeButton = new Button()
      {
        Left  = 290,
        Width = 80,
        Top   = 130,
        Text  = "Close"
      };
      closeButton.Click += (object sender, EventArgs e) =>
      {
        dialog.Close();
      };

      dialog.Controls.Add(textBox);
      dialog.Controls.Add(websiteButton);
      dialog.Controls.Add(closeButton);
      dialog.ShowDialog();
    }
  }
}
