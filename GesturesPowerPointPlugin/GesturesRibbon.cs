using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using Microsoft.Gestures.Samples.GesturesPowerPointPlugin.Properties;

namespace Microsoft.Gestures.Samples.GesturesPowerPointPlugin
{
    public partial class GesturesRibbon
    {
        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {
            txtTo.Text = Settings.Default.DefaultTo;
            txtSubject.Text = Settings.Default.DefaultSubject;
            txtMessage.Text = Settings.Default.DefaultMessage;
        }

        private void OnToChanged(object sender, RibbonControlEventArgs e)
        {
            Settings.Default.DefaultTo = txtTo.Text;
            Settings.Default.Save();
        }

        private void OnSubjectChanged(object sender, RibbonControlEventArgs e) 
        {
            Settings.Default.DefaultSubject = txtSubject.Text;
            Settings.Default.Save();
        }

        private void OnMessageChanged(object sender, RibbonControlEventArgs e)
        {
            Settings.Default.DefaultMessage = txtMessage.Text;
            Settings.Default.Save();
        }

        private async void OnEnableExplodeClicked(object sender, RibbonControlEventArgs e) => await Globals.ThisAddIn.ExplodeGestureDetection(chkEnableExplode.Checked);

        private async void chkEnableTapClicked(object sender, RibbonControlEventArgs e) => await Globals.ThisAddIn.TapGestureDetection(chkEnableTap.Checked);

        private async void OnShareEnabledClick(object sender, RibbonControlEventArgs e) => await Globals.ThisAddIn.ShareGestureDetection(chkEnableShare.Checked);
    }
}
