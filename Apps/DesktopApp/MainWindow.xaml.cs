using Orbital7.Extensions.Windows;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Catalog Catalog { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            App.SetWindowFont(this);

            var config = LoadConfig();
            if (config != null)
            {
                this.Catalog = new Catalog(config);
                navigationTreeview.Load(this.Catalog);
            }
            else
            {
                this.Close();
            }
        }

        public Config LoadConfig()
        {
            string configFolderPath = ReflectionHelper.GetExecutingAssemblyFolder();

            var config = Config.Load(configFolderPath);
            if (config == null)
            {
                string gamesPath = Orbital7.Extensions.Windows.Desktop.WinForms.CommonDialogsHelper.ShowFolderBrowseDialog(
                    "Select the location of your games folder:");
                if (!String.IsNullOrEmpty(gamesPath))
                {
                    config = new Config(configFolderPath, gamesPath);
                    config.Save();
                }
            }

            return config;
        }

        private void navigationTreeview_NavigationTreeviewItemSelected(NavigationTreeviewModelItem item)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            gamesListview.Clear();

            if (item != null)
            {
                if (item.Type == NavigationTreeviewModelItemType.Platform && item.GameList != null)
                    gamesListview.Load((from Game x in item.GameList select x).ToList());
                else if (item.Type == NavigationTreeviewModelItemType.IncompleteGames)
                    gamesListview.Load(this.Catalog.GatherIncompleteGames());
            }    

            Mouse.OverrideCursor = null;
        }

        private void buttonMatchIncomplete_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MatchGamesDialog(this.Catalog.Config.FolderPath, this.Catalog.GatherIncompleteGames());
            dialog.Owner = this;
            dialog.ShowDialog();
            gamesListview.Update();
        }

        private void buttonSyncWithDevice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var config = Config.Load(this.Catalog.Config.FolderPath);
                if (config.Devices.Count > 0)
                {
                    var device = config.Devices[0];
                    this.Catalog.SyncWithDevice(device.DirectoryKey);
                    MessageBoxHelper.ShowMessage(this, device.Name + " synced successfully");
                }
                else
                {
                    throw new Exception("Configuration does not specify any devices");
                }
            }
            catch(Exception ex)
            {
                MessageBoxHelper.ShowError(this, ex);
            }
        }
    }
}
