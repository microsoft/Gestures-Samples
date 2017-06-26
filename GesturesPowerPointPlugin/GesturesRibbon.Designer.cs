namespace Microsoft.Gestures.Samples.GesturesPowerPointPlugin
{
    partial class GesturesRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public GesturesRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tab1 = this.Factory.CreateRibbonTab();
            this.grpDetectionService = this.Factory.CreateRibbonGroup();
            this.chkEnableExplode = this.Factory.CreateRibbonCheckBox();
            this.chkEnableTap = this.Factory.CreateRibbonCheckBox();
            this.chkEnableShare = this.Factory.CreateRibbonCheckBox();
            this.txtStatus = this.Factory.CreateRibbonEditBox();
            this.grpDefaultMailSettings = this.Factory.CreateRibbonGroup();
            this.txtTo = this.Factory.CreateRibbonEditBox();
            this.txtSubject = this.Factory.CreateRibbonEditBox();
            this.txtMessage = this.Factory.CreateRibbonEditBox();
            this.separator1 = this.Factory.CreateRibbonSeparator();
            this.tab1.SuspendLayout();
            this.grpDetectionService.SuspendLayout();
            this.grpDefaultMailSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // tab1
            // 
            this.tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tab1.Groups.Add(this.grpDetectionService);
            this.tab1.Groups.Add(this.grpDefaultMailSettings);
            this.tab1.Label = "Gestures";
            this.tab1.Name = "tab1";
            // 
            // grpDetectionService
            // 
            this.grpDetectionService.Items.Add(this.chkEnableExplode);
            this.grpDetectionService.Items.Add(this.chkEnableTap);
            this.grpDetectionService.Items.Add(this.chkEnableShare);
            this.grpDetectionService.Items.Add(this.separator1);
            this.grpDetectionService.Items.Add(this.txtStatus);
            this.grpDetectionService.Label = "Gesture Detection Service";
            this.grpDetectionService.Name = "grpDetectionService";
            // 
            // chkEnableExplode
            // 
            this.chkEnableExplode.Checked = true;
            this.chkEnableExplode.Label = "[Explode] to start presentation";
            this.chkEnableExplode.Name = "chkEnableExplode";
            this.chkEnableExplode.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.OnEnableExplodeClicked);
            // 
            // chkEnableTap
            // 
            this.chkEnableTap.Checked = true;
            this.chkEnableTap.Label = "[Tap] to advance slides";
            this.chkEnableTap.Name = "chkEnableTap";
            this.chkEnableTap.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.chkEnableTapClicked);
            // 
            // chkEnableShare
            // 
            this.chkEnableShare.Checked = true;
            this.chkEnableShare.Label = "[Three Unpinch] to share by mail";
            this.chkEnableShare.Name = "chkEnableShare";
            this.chkEnableShare.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.OnShareEnabledClick);
            // 
            // txtStatus
            // 
            this.txtStatus.Enabled = false;
            this.txtStatus.Label = "Status";
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.SizeString = "XXXXXXXXXXXX";
            this.txtStatus.Text = null;
            // 
            // grpDefaultMailSettings
            // 
            this.grpDefaultMailSettings.Items.Add(this.txtTo);
            this.grpDefaultMailSettings.Items.Add(this.txtSubject);
            this.grpDefaultMailSettings.Items.Add(this.txtMessage);
            this.grpDefaultMailSettings.Label = "Default Mail Settings for Share Gesture";
            this.grpDefaultMailSettings.Name = "grpDefaultMailSettings";
            // 
            // txtTo
            // 
            this.txtTo.Label = "To";
            this.txtTo.Name = "txtTo";
            this.txtTo.SizeString = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
            this.txtTo.Text = null;
            this.txtTo.TextChanged += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.OnToChanged);
            // 
            // txtSubject
            // 
            this.txtSubject.Label = "Subject";
            this.txtSubject.Name = "txtSubject";
            this.txtSubject.SizeString = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
            this.txtSubject.Text = null;
            this.txtSubject.TextChanged += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.OnSubjectChanged);
            // 
            // txtMessage
            // 
            this.txtMessage.Label = "Message";
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.SizeString = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
            this.txtMessage.Text = null;
            this.txtMessage.TextChanged += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.OnMessageChanged);
            // 
            // separator1
            // 
            this.separator1.Name = "separator1";
            // 
            // GestureShareRibbon
            // 
            this.Name = "GestureShareRibbon";
            this.RibbonType = "Microsoft.PowerPoint.Presentation";
            this.Tabs.Add(this.tab1);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.Ribbon1_Load);
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.grpDetectionService.ResumeLayout(false);
            this.grpDetectionService.PerformLayout();
            this.grpDefaultMailSettings.ResumeLayout(false);
            this.grpDefaultMailSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpDefaultMailSettings;
        internal Microsoft.Office.Tools.Ribbon.RibbonEditBox txtTo;
        internal Microsoft.Office.Tools.Ribbon.RibbonEditBox txtSubject;
        internal Microsoft.Office.Tools.Ribbon.RibbonEditBox txtMessage;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup grpDetectionService;
        internal Microsoft.Office.Tools.Ribbon.RibbonEditBox txtStatus;
        internal Office.Tools.Ribbon.RibbonCheckBox chkEnableExplode;
        internal Office.Tools.Ribbon.RibbonCheckBox chkEnableTap;
        internal Office.Tools.Ribbon.RibbonCheckBox chkEnableShare;
        internal Office.Tools.Ribbon.RibbonSeparator separator1;
    }

    partial class ThisRibbonCollection
    {
        internal GesturesRibbon GesturesRibbon
        {
            get { return this.GetRibbon<GesturesRibbon>(); }
        }
    }
}
