using System;
using System.Drawing;
using System.Windows.Forms;

namespace DailyEvents
{
  static public class About
  {
    static public void Show()
    {
      Form dialog = new Form()
      {
        StartPosition   = FormStartPosition.CenterScreen,
        FormBorderStyle = FormBorderStyle.FixedSingle,
        MaximizeBox     = false,
        Width           = 390,
        Height          = 200,
        Text            = "About"
      };

      Label textLabel = new Label()
      {
        Left   = 15,
        Top    = 20,
        Width  = 370,
        Height = 100,
        Text   = "Daily Events v" + AppInfo.CurrentVersion + " is brought to you by Tiago Fernandez.\n\n" +
                 "This app has been developed entirely on my wife's \"budget\" so far, " +
                 "so please consider donating to actually buy me some extra time to keep " +
                 "working on it.\n\nThanks!"
      };

      Button feedbackButton = new Button()
      {
        Left  = 15,
        Width = 80,
        Top   = 130,
        Text  = "Feedback"
      };
      feedbackButton.Click += (object sender, EventArgs e) =>
      {
        System.Diagnostics.Process.Start(AppInfo.FeedbackUrl());
        dialog.Close();
      };

      Button websiteButton = new Button()
      {
        Left  = 105,
        Width = 80,
        Top   = 130,
        Text  = "Website"
      };
      websiteButton.Click += (object sender, EventArgs e) =>
      {
        System.Diagnostics.Process.Start(AppInfo.MarketingUrl());
        dialog.Close();
      };

      Button donateButton = new Button()
      {
        Left  = 200,
        Width = 80,
        Top   = 130,
        Text  = "Donate"
      };
      donateButton.Click += (object sender, EventArgs e) =>
      {
        System.Diagnostics.Process.Start(AppInfo.DonationUrl());
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

      dialog.Controls.Add(textLabel);
      dialog.Controls.Add(feedbackButton);
      dialog.Controls.Add(websiteButton);
      dialog.Controls.Add(donateButton);
      dialog.Controls.Add(closeButton);
      dialog.ShowDialog();
    }
  }
}
