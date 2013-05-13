using System;
using System.Drawing;
using System.Windows.Forms;

namespace DailyEvents
{
  static public class Prompt
  {
    static public string Show(string title, string text, int maxLength)
    {
      return Show(title, text, String.Empty, maxLength);
    }

    static public string Show(string title, string text, string placeholder, int maxLength)
    {
      Form dialog = new Form()
      {
        StartPosition   = FormStartPosition.CenterScreen,
        FormBorderStyle = FormBorderStyle.FixedSingle,
        ControlBox      = false,
        Width           = 350,
        Height          = 150,
        Text            = title
      };
      Label textLabel = new Label()
      {
        Left  = 15,
        Top   = 20,
        Width = 280,
        Text  = text
      };
      Label countLabel = new Label()
      {
        Left      = 300,
        Top       = 20,
        Width     = 30,
        Text      = (maxLength - placeholder.Length).ToString(),
        ForeColor = Color.Gray
      };
      TextBox inputBox = new TextBox()
      {
        Left      = 15,
        Top       = 45,
        Width     = 310,
        Text      = placeholder,
        MaxLength = maxLength
      };
      Button okButton = new Button()
      {
        Left  = 155,
        Width = 80,
        Top   = 80,
        Text  = "OK"
      };
      Button cancelButton = new Button()
      {
        Left  = 245,
        Width = 80,
        Top   = 80,
        Text  = "Cancel"
      };

      inputBox.KeyPress += (object sender, KeyPressEventArgs e) =>
      {
        switch (e.KeyChar)
        {
          case (char) 13: // ENTER
            okButton.PerformClick();
            break;
          
          case (char) 27: // ESC
            cancelButton.PerformClick();
            break;
          
          default:
            break;
        }
      };
      inputBox.TextChanged += (object sender, EventArgs e) =>
      {
        countLabel.Text = (maxLength - ((TextBox) sender).Text.Length).ToString();
      };
      okButton.Click += (object sender, EventArgs e) =>
      {
        dialog.Close();
      };
      cancelButton.Click += (object sender, EventArgs e) =>
      {
        inputBox.Text = "";
        dialog.Dispose();
      };

      dialog.Controls.Add(textLabel);
      dialog.Controls.Add(countLabel);
      dialog.Controls.Add(inputBox);
      dialog.Controls.Add(okButton);
      dialog.Controls.Add(cancelButton);
      dialog.ShowDialog();
      
      return inputBox.Text.Trim();
    }
  }
}
