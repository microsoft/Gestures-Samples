using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Outlook = Microsoft.Office.Interop.Outlook;
using Microsoft.Gestures.Endpoint;
using Microsoft.Gestures.Stock.Gestures;
using Microsoft.Gestures.Samples.GesturesPowerPointPlugin.Properties;
using System.Threading.Tasks;

namespace Microsoft.Gestures.Samples.GesturesPowerPointPlugin
{
    public partial class ThisAddIn
    {
        private const int MailItemPopTimeout = 5000;

        private GesturesServiceEndpoint _gesturesService;
        private ThreeUnpinchGesture _shareGesture;
        private TapGesture _tapGesture;
        private ExplodeGesture _explodeGesture;

        public async Task ShareGestureDetection(bool enable) => await SetGetureDetection(_shareGesture, enable);
        public async Task TapGestureDetection(bool enable) => await SetGetureDetection(_tapGesture, enable);
        public async Task ExplodeGestureDetection(bool enable) => await SetGetureDetection(_explodeGesture, enable);

        private async Task SetGetureDetection(Gesture gesture, bool enable)
        {
            if (gesture == null) return;
            if (enable) await _gesturesService.RegisterGesture(gesture);
            else await _gesturesService.UnregisterGesture(gesture);
        }

        private async void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            Globals.Ribbons.GesturesRibbon.txtStatus.Text = EndpointStatus.Disconnected.ToString();

            _gesturesService = GesturesServiceEndpointFactory.Create();
            _gesturesService.StatusChanged += (s, args) => Globals.Ribbons.GesturesRibbon.txtStatus.Text = args.Status.ToString();
            await _gesturesService.ConnectAsync();
            
            // Share Gesture
            _shareGesture = new ThreeUnpinchGesture("ShareAsAttachment");
            _shareGesture.Triggered += OnShareWithGesture;
            await _gesturesService.RegisterGesture(_shareGesture);

            // Start Presentation
            _explodeGesture = new ExplodeGesture("StartPresentation");
            _explodeGesture.Triggered += OnStartPresentation;
            await _gesturesService.RegisterGesture(_explodeGesture);

            // Advance to next Slide Gesture
            _tapGesture = new TapGesture("NextSlide");
            _tapGesture.Triggered += OnNextSlide;
            await _gesturesService.RegisterGesture(_tapGesture);
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            _gesturesService?.Dispose();
        }

        private void OnNextSlide(object sender, GestureSegmentTriggeredEventArgs e)
        {
            Globals.ThisAddIn.Application.ActivePresentation?.SlideShowWindow?.View?.Next();
        }

        private void OnStartPresentation(object sender, GestureSegmentTriggeredEventArgs e)
        {
            Globals.ThisAddIn.Application.ActivePresentation?.SlideShowSettings?.Run();
        }

        private void OnShareWithGesture(object sender, GestureSegmentTriggeredEventArgs e)
        {
            Globals.ThisAddIn.Application.CommandBars.ExecuteMso("FileSendAsAttachment");

            Outlook.MailItem mailItem = null;
            for (int attempt = 0; attempt < MailItemPopTimeout / 500; attempt++)
            {
                Task.Delay(500).Wait();

                Outlook.Application outlook = new Outlook.Application();
                mailItem = outlook.ActiveInspector().CurrentItem as Outlook.MailItem;
                if (mailItem != null) break;
            }

            mailItem.To = Settings.Default.DefaultTo;
            mailItem.Subject = Settings.Default.DefaultSubject;
            mailItem.Body = Settings.Default.DefaultMessage;
        }


        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion
    }
}
