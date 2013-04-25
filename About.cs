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
        Width           = 400,
        Height          = 180,
        Text            = "About"
      };

      Label textLabel = new Label()
      {
        Left   = 15,
        Top    = 20,
        Width  = 370,
        Height = 80,
        Text   = "Daily Events for Windows is brought to you by Tiago Fernandez.\n\n" +
          "This app has been developed entirely on my wife's budget so far, so please consider donating to actually buy me some extra time to keep working on it.\n\n" +
          "Thanks! :)"
      };

      Button feedbackButton = new Button()
      {
        Left  = 15,
        Width = 80,
        Top   = 110,
        Text  = "Feedback"
      };
      feedbackButton.Click += (object sender, EventArgs e) =>
      {
        System.Diagnostics.Process.Start("mailto:tiago.fernandez@gmail.com");
        dialog.Close();
      };

      Button donateButton = new Button()
      {
        Left  = 210,
        Width = 80,
        Top   = 110,
        Text  = "Donate"
      };
      donateButton.Click += (object sender, EventArgs e) =>
      {
        System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=3NLLLDBPUFAT4&lc=FR&item_name=Tiago%20Fernandez&item_number=Daily%20Events&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donate_LG%2egif%3aNonHosted");
        dialog.Close();
      };
      
      Button notNowButton = new Button()
      {
        Left  = 300,
        Width = 80,
        Top   = 110,
        Text  = "Not Now"
      };
      notNowButton.Click += (object sender, EventArgs e) =>
      {
        dialog.Close();
      };
      
      dialog.Controls.Add(textLabel);
      dialog.Controls.Add(feedbackButton);
      dialog.Controls.Add(donateButton);
      dialog.Controls.Add(notNowButton);
      dialog.ShowDialog();
    }
  }
}
