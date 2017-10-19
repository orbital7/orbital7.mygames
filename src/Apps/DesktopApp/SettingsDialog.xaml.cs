using Orbital7.Extensions;
using Orbital7.Extensions.WPF;
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
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private CatalogConfig Config { get; set; }

        public SettingsDialog(CatalogConfig config)
        {
            InitializeComponent();
            App.SetWindowFont(this);

            this.Config = config.Clone<CatalogConfig>();
            this.DataContext = this.Config;

            UpdatePlatformConfigs();
            UpdateDevices();
        }

        private void UpdatePlatformConfigs()
        {
            WPFHelper.FillListBox(listPlatformConfigs, this.Config.PlatformConfigs, false);
            ValidatePlatformConfigsToolbar();
        }

        private void UpdateDevices()
        {
            WPFHelper.FillListBox(listDevices, this.Config.Devices, false);
            ValidateDevicesToolbar();
        }

        private void ValidatePlatformConfigsToolbar()
        {
            buttonEditPlatformConfig.IsEnabled = listPlatformConfigs.SelectedItem != null;
            buttonDeletePlatformConfig.IsEnabled = buttonEditPlatformConfig.IsEnabled;
        }

        private void ValidateDevicesToolbar()
        {
            buttonEditDevice.IsEnabled = listDevices.SelectedItem != null;
            buttonDeleteDevice.IsEnabled = buttonEditDevice.IsEnabled;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AsyncHelper.RunSync(() => this.Config.SaveAsync());
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError(this, ex);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonRomsPathBrowse_Click(object sender, RoutedEventArgs e)
        {
            string path = CommonDialogsHelper.ShowFolderBrowseDialog(
                "Select ROMs Folder Path:", this.Config.RomsFolderPath);

            if (!String.IsNullOrEmpty(path))
                this.Config.RomsFolderPath = path;
        }

        private void buttonAddPlatformConfig_Click(object sender, RoutedEventArgs e)
        {
            var platformConfig = new PlatformConfig();
            var dialog = new EditPlatformConfigDialog(platformConfig, "Add");
            dialog.Owner = this;

            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                this.Config.PlatformConfigs.Add(dialog.PlatformConfig);
                UpdatePlatformConfigs();
                listPlatformConfigs.SelectedItem = dialog.PlatformConfig;
            }
        }

        private void buttonEditPlatformConfig_Click(object sender, RoutedEventArgs e)
        {
            var platformConfig = (PlatformConfig)listPlatformConfigs.SelectedItem;
            if (platformConfig != null)
            {
                var dialog = new EditPlatformConfigDialog(platformConfig, "Edit");
                dialog.Owner = this;

                var result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    var index = this.Config.PlatformConfigs.IndexOf(platformConfig);
                    this.Config.PlatformConfigs.Remove(platformConfig);
                    this.Config.PlatformConfigs.Insert(index, dialog.PlatformConfig);
                    UpdatePlatformConfigs();
                    listPlatformConfigs.SelectedItem = dialog.PlatformConfig;
                }
            }
        }

        private void buttonDeletePlatformConfig_Click(object sender, RoutedEventArgs e)
        {
            var platformConfig = (PlatformConfig)listPlatformConfigs.SelectedItem;
            if (platformConfig != null)
            {
                if (MessageBoxHelper.AskQuestion(this, "Are you sure you want to delete the Platform Config \"" +
                    platformConfig.ToString() + "\"?", true))
                {
                    this.Config.PlatformConfigs.Remove(platformConfig);
                    UpdatePlatformConfigs();
                }
            }
        }

        private void buttonAddDevice_Click(object sender, RoutedEventArgs e)
        {
            var device = new Device();
            var dialog = new EditDeviceDialog(device, "Add");
            dialog.Owner = this;

            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                this.Config.Devices.Add(dialog.Device);
                UpdateDevices();
                listDevices.SelectedItem = dialog.Device;
            }
        }

        private void buttonEditDevice_Click(object sender, RoutedEventArgs e)
        {
            var device = (Device)listDevices.SelectedItem;
            if (device != null)
            {
                var dialog = new EditDeviceDialog(device, "Edit");
                dialog.Owner = this;

                var result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    var index = this.Config.Devices.IndexOf(device);
                    this.Config.Devices.Remove(device);
                    this.Config.Devices.Insert(index, dialog.Device);
                    UpdateDevices();
                    listDevices.SelectedItem = dialog.Device;
                }
            }
        }

        private void buttonDeleteDevice_Click(object sender, RoutedEventArgs e)
        {
            var device = (Device)listDevices.SelectedItem;
            if (device != null)
            {
                if (MessageBoxHelper.AskQuestion(this, "Are you sure you want to delete the Device \"" +
                    device.ToString() + "\"?", true))
                {
                    this.Config.Devices.Remove(device);
                    UpdateDevices();
                }
            }
        }
        
        private void listPlatformConfigs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidatePlatformConfigsToolbar();
        }

        private void listDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateDevicesToolbar();
        }
    }
}
