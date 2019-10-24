using System;
using System.ComponentModel;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Tooling.CrmConnectControl;

namespace Unchase.Dynamics365.ConnectedService.View
{
    /// <summary>
    /// Interaction logic for CRMInteractiveLogin.xaml
    /// </summary>
    public partial class CRMInteractiveLogin : Window
    {
        /// <summary>
        /// CRM Connection Manager
        /// </summary>
        internal CrmConnectionManager CrmConnectionMgr => this.mgr;

        /// <summary>
        /// Host Name to use.
        /// </summary>
        public string HostApplicationNameOverride { get; set; }

        /// <summary>
        /// Profile Name to use
        /// </summary>
        public string HostProfileName { get; set; }

        /// <summary>
        /// When set to true, forces a user login,
        /// </summary>
        public bool ForceDirectLogin { get; set; }

        /// <summary>
        /// Raised when a connection to CRM has completed.
        /// </summary>
        public event EventHandler ConnectionToCrmCompleted;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CRMInteractiveLogin()
        {
            this.InitializeXrmUiStyles();
            this.InitializeComponent();
            this.HostApplicationNameOverride = null;
            this.HostProfileName = "default";
            this.ForceDirectLogin = false;
        }

        private void InitializeXrmUiStyles()
        {
            var dummy = new Microsoft.Xrm.Tooling.Ui.Styles.SortAdorner(new UIElement(), ListSortDirection.Ascending);
        }

        /// <summary>
        /// Raised when the window loads for the first time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.bIsConnectedComplete = false;
            this.ExecuteLoginProcess();
        }

        private void SetupLoginControl()
        {
            this.bIsConnectedComplete = false;
            this.mgr = new CrmConnectionManager
            {
                ClientId = "2ad88395-b77d-4561-9441-d0e40824f9bc",
                RedirectUri = new Uri("app://5d3e90d6-aa8e-48a8-8f2c-58b45cc67315"),
                ParentControl = this.CrmLoginCtrl,
                UseUserLocalDirectoryForConfigStore = true,
                HostApplicatioNameOveride = this.HostApplicationNameOverride,
                ProfileName = this.HostProfileName
            };
            this.CrmLoginCtrl.SetGlobalStoreAccess(this.mgr);
            this.CrmLoginCtrl.SetControlMode(ServerLoginConfigCtrlMode.FullLoginPanel);
            this.CrmLoginCtrl.ConnectionCheckBegining += this.CrmLoginCtrl_ConnectionCheckBeginning;
            this.CrmLoginCtrl.ConnectErrorEvent += this.CrmLoginCtrl_ConnectErrorEvent;
            this.CrmLoginCtrl.ConnectionStatusEvent += this.CrmLoginCtrl_ConnectionStatusEvent;
            this.CrmLoginCtrl.UserCancelClicked += this.CrmLoginCtrl_UserCancelClicked;
        }

        /// <summary>
        /// Run Login process.
        /// </summary>
        public void ExecuteLoginProcess()
        {
            if (this.mgr == null)
            {
                this.SetupLoginControl();
            }
            if (!this.ForceDirectLogin && !this.mgr.RequireUserLogin())
            {
                this.CrmLoginCtrl.IsEnabled = false;
                this.mgr.ServerConnectionStatusUpdate += this.mgr_ServerConnectionStatusUpdate;
                this.mgr.ConnectionCheckComplete += this.mgr_ConnectionCheckComplete;
                this.mgr.ConnectToServerCheck();
                this.CrmLoginCtrl.ShowMessageGrid();
            }
        }

        /// <summary>
        /// Updates from the Auto Login process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mgr_ServerConnectionStatusUpdate(object sender, ServerConnectStatusEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                this.Title = (string.IsNullOrWhiteSpace(e.StatusMessage) ? e.ErrorMessage : e.StatusMessage);
            });
        }

        /// <summary>
        /// Complete Event from the Auto Login process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mgr_ConnectionCheckComplete(object sender, ServerConnectStatusEventArgs e)
        {
            ((CrmConnectionManager)sender).ConnectionCheckComplete -= this.mgr_ConnectionCheckComplete;
            ((CrmConnectionManager)sender).ServerConnectionStatusUpdate -= this.mgr_ServerConnectionStatusUpdate;
            if (!e.Connected)
            {
                MessageBox.Show(
                    e.MultiOrgsFound
                        ? "Unable to Login to CRM using cached credentials. Org Not found"
                        : "Unable to Login to CRM using cached credentials", "Login Failure");
                this.resetUiFlag = true;
                this.CrmLoginCtrl.GoBackToLogin();
                ThreadHelper.JoinableTaskFactory.Run(async delegate {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    base.Title = "Failed to Login with cached credentials.";
                    MessageBox.Show(base.Title, "Notification from CRM ConnectionManager", MessageBoxButton.OK, MessageBoxImage.Hand);
                    this.CrmLoginCtrl.IsEnabled = true;
                });
                this.resetUiFlag = false;
                return;
            }
            if (e.Connected && !this.bIsConnectedComplete)
            {
                this.ProcessSuccess();
            }
        }

        /// <summary>
        ///  Login control connect check starting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CrmLoginCtrl_ConnectionCheckBeginning(object sender, EventArgs e)
        {
            this.bIsConnectedComplete = false;
            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                base.Title = "Starting Login Process. ";
                this.CrmLoginCtrl.IsEnabled = true;
            });
        }

        /// <summary>
        /// Login control connect check status event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CrmLoginCtrl_ConnectionStatusEvent(object sender, ConnectStatusEventArgs e)
        {
            if (e.ConnectSucceeded && !this.bIsConnectedComplete)
            {
                this.ProcessSuccess();
            }
        }

        /// <summary>
        /// Login control Error event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CrmLoginCtrl_ConnectErrorEvent(object sender, ConnectErrorEventArgs e)
        {
        }

        /// <summary>
        /// Login Control Cancel event raised.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CrmLoginCtrl_UserCancelClicked(object sender, EventArgs e)
        {
            if (!this.resetUiFlag)
            {
                base.Close();
            }
        }

        /// <summary>
        /// This raises and processes Success
        /// </summary>
        private void ProcessSuccess()
        {
            this.resetUiFlag = true;
            this.bIsConnectedComplete = true;
            this.CrmSvc = this.mgr.CrmSvc;
            this.CrmLoginCtrl.GoBackToLogin();
            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                base.Title = "Login Complete.";
                this.CrmLoginCtrl.IsEnabled = true;
            });
            ConnectionToCrmCompleted?.Invoke(this, null);
            this.resetUiFlag = false;
            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                base.DialogResult = true;
                base.Close();
            });
        }

        /// <summary>
        /// Microsoft.Xrm.Tooling.Connector services
        /// </summary>
        private CrmServiceClient CrmSvc;

        /// <summary>
        /// Bool flag to determine if there is a connection
        /// </summary>
        private bool bIsConnectedComplete;

        /// <summary>
        /// CRM Connection Manager component.
        /// </summary>
        private CrmConnectionManager mgr;

        /// <summary>
        ///  This is used to allow the UI to reset w/out closing
        /// </summary>
        private bool resetUiFlag;
    }
}
