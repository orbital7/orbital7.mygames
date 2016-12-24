using Orbital7.Extensions.Windows;
using Orbital7.Extensions.Windows.Desktop.WPF;
using Orbital7.MyGames;
using Orbital7.MyGames.Local;
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
        private CatalogEditor CatalogEditor { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            App.SetWindowFont(this);

            var config = LoadConfig();
            if (config != null)
            {
                this.CatalogEditor = new CatalogEditor(config);
                navigationTreeview.Load(this.CatalogEditor.Catalog);
            }
            else
            {
                this.Close();
            }
        }

        public CatalogConfig LoadConfig()
        {
            string configFolderPath = ReflectionHelper.GetExecutingAssemblyFolder();

            var accessProcessor = new LocalAccessProvider();
            var config = Config.Load<CatalogConfig>(accessProcessor, configFolderPath);
            if (config == null)
            {
                string romsFolderPath = Orbital7.Extensions.Windows.Desktop.WinForms.CommonDialogsHelper.ShowFolderBrowseDialog(
                    "Select the location of your ROMs folder:");
                if (!String.IsNullOrEmpty(romsFolderPath))
                {
                    config = new CatalogConfig(accessProcessor, configFolderPath, romsFolderPath);
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
                    gamesListview.Load(this.CatalogEditor, (from Game x in item.GameList select x).ToList());
                else if (item.Type == NavigationTreeviewModelItemType.IncompleteGames)
                    gamesListview.Load(this.CatalogEditor, this.CatalogEditor.GatherIncompleteGames());
            }    

            Mouse.OverrideCursor = null;
        }

        private void buttonMatchIncomplete_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MatchGamesDialog(this.CatalogEditor);
            dialog.Owner = this;
            dialog.ShowDialog();
            gamesListview.Update();
        }

        private void buttonSyncWithDevice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.CatalogEditor.Config.Devices.Count > 0)
                    new LocalSyncEngine().SyncWithDevice(this.CatalogEditor.Catalog, this.CatalogEditor.Config.Devices[0].DirectoryKey);
                else
                    throw new Exception("Configuration does not specify any devices");
            }
            catch(Exception ex)
            {
                MessageBoxHelper.ShowError(this, ex);
            }
        }
    }
}
