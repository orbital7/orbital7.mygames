using Orbital7.Extensions;
using Orbital7.Extensions.Windows.Desktop.WPF;
using Orbital7.MyGames;
using Orbital7.MyGames.Local;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for SyncDeviceDialog.xaml
    /// </summary>
    public partial class SyncDeviceDialog : Window
    {
        private Catalog Catalog { get; set; }

        private Device Device { get; set; }

        private BackgroundWorker BackgroundWorker { get; set; }

        private LocalDevicePushSyncEngine SyncEngine { get; set; }

        public SyncDeviceDialog(Catalog catalog)
        {
            InitializeComponent();
            App.SetWindowFont(this);
            this.Closing += SyncDeviceDialog_Closing;

            this.Catalog = catalog;

            this.SyncEngine = new LocalDevicePushSyncEngine();
            this.SyncEngine.Progress += SyncEngine_Progress;

            foreach (var device in ((CatalogConfig)catalog.Config).Devices)
                comboDevice.Items.Add(device);

            if (comboDevice.Items.Count > 0)
            {
                comboDevice.SelectedIndex = 0;
                buttonSync.IsEnabled = true;
            }
        }

        private void SyncEngine_Progress(DeviceSyncEngineProgress updatedProgress)
        {
            this.BackgroundWorker.ReportProgress(updatedProgress.ProgressPercent, 
                updatedProgress.Description);
        }
        
        private void SyncDeviceDialog_Closing(object sender, CancelEventArgs e)
        {
            if (this.BackgroundWorker != null)
            {
                if (MessageBoxHelper.AskQuestion(this, "Device Sync in progress; cancel?", true))
                    this.SyncEngine.StopSync();
                else
                    e.Cancel = true;
            }
        }

        private void buttonDone_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonSync_Click(object sender, RoutedEventArgs e)
        {
            if (this.BackgroundWorker == null)
            {
                try
                {
                    AsyncHelper.RunSync(() => this.SyncEngine.LoadAsync(this.Catalog, 
                        comboDevice.SelectedItem as Device));

                    buttonSync.Content = "Stop";
                    buttonDone.IsEnabled = false;
                    textOutput.Clear();
                    Console.SetOut(new TextBoxStreamWriter(textOutput));
                    progress.Minimum = 0;
                    progress.Maximum = this.Catalog.GameLists.Count;

                    this.BackgroundWorker = new BackgroundWorker();
                    this.BackgroundWorker.WorkerReportsProgress = true;
                    this.BackgroundWorker.DoWork += Worker_DoWork;
                    this.BackgroundWorker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                    this.BackgroundWorker.ProgressChanged += Worker_ProgressChanged;
                    this.BackgroundWorker.RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    MessageBoxHelper.ShowError(this, ex);
                }
            }
            else
            {
                buttonSync.Content = "Stopping...";
                buttonSync.IsEnabled = false;
                this.SyncEngine.StopSync();
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress.Value = progress.Maximum * e.ProgressPercentage / 100;
            textOutput.ScrollToEnd();
            Console.Write(e.UserState.ToString());
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.BackgroundWorker = null;
            buttonDone.IsEnabled = true;
            buttonSync.Content = "Sync Now";
            buttonSync.IsEnabled = true;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            AsyncHelper.RunSync(() => this.SyncEngine.StartSyncAsync());
        }
    }
}
