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
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private CatalogConfig Config { get; set; }

        public SettingsDialog(CatalogConfig config)
        {
            InitializeComponent();

            this.Config = XMLSerializationHelper.CloneObject<CatalogConfig>(config);
            this.DataContext = this.Config;
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
            string path = Orbital7.Extensions.Windows.Desktop.WinForms.CommonDialogsHelper.ShowFolderBrowseDialog(
                "Select ROMs Folder Path:");

            if (!String.IsNullOrEmpty(path))
                this.Config.RomsFolderPath = path;
        }

        private void buttonAddPlatformConfig_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonEditPlatformConfig_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonDeletePlatformConfig_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonAddDevice_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonEditDevice_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonDeleteDevice_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonExportDeviceConfig_Click(object sender, RoutedEventArgs e)
        {

        }

        private void listPlatformConfigs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void listDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
