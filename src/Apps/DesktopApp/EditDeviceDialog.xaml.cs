using Orbital7.Extensions;
using Orbital7.Extensions.Windows.Desktop.WPF;
using Orbital7.MyGames;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for EditDeviceDialog.xaml
    /// </summary>
    public partial class EditDeviceDialog : Window
    {
        public Device Device { get; private set; }

        public EditDeviceDialog(Device device, string action)
        {
            InitializeComponent();
            App.SetWindowFont(this);
            this.Title = action + " Device";

            this.Device = XMLSerializationHelper.CloneObject<Device>(device);
            this.DataContext = this.Device;

            UpdateSyncSelections();
        }

        private void UpdateSyncSelections()
        {
            WPFHelper.FillListBox(listSyncSelections, this.Device.SyncPlatformSelections, false);
            ValidateSyncSelectionsToolbar();
        }

        private void ValidateSyncSelectionsToolbar()
        {
            buttonDeleteSyncSelection.IsEnabled = listSyncSelections.SelectedItem != null;
        }

        private void listSyncSelections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateSyncSelectionsToolbar();
        }
        
        private void buttonDone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Device.Validate();
                this.DialogResult = true;
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBoxHelper.ShowError(this, ex);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonAddSyncSelection_Click(object sender, RoutedEventArgs e)
        {
            Platform? platform = (Platform?)CommonDialogsHelper.ShowInputComboBoxDialog(this, "Select Sync Platform:", 
                EnumHelper.EnumToList<Platform>(), "Add Sync Selection", null, true);
            if (platform.HasValue)
            {
                this.Device.SyncPlatformSelections.Add(platform.Value);
                UpdateSyncSelections();
                listSyncSelections.SelectedItem = platform.Value;
            }
        }
        
        private void buttonDeleteSyncSelection_Click(object sender, RoutedEventArgs e)
        {
            if (listSyncSelections.SelectedItem != null)
            {
                Platform platform = (Platform)listSyncSelections.SelectedItem;
                if (MessageBoxHelper.AskQuestion(this, "Are you sure you want to delete the Sync Selection \"" + platform + "\"?"))
                {
                    this.Device.SyncPlatformSelections.Remove(platform);
                    UpdateSyncSelections();
                }
            }
        }
    }
}
