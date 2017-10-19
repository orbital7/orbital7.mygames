using Orbital7.Extensions;
using Orbital7.Extensions.WPF;
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

            if (!UpdateView())
                this.Close();
        }

        private bool UpdateView()
        {
            var config = AsyncHelper.RunSync(() => LoadConfigAsync());
            if (config != null)
            {
                this.CatalogEditor = new CatalogEditor(config);
                navigationTreeview.Load(this.CatalogEditor.Catalog);
                gamesListview.Clear();
                return true;
            }

            return false;
        }

        private async Task<CatalogConfig> LoadConfigAsync()
        {
            string configFolderPath = ReflectionHelper.GetExecutingAssemblyFolderPath();

            var accessProcessor = new LocalAccessProvider();
            var config = await Config.LoadAsync<CatalogConfig>(accessProcessor, configFolderPath);
            if (config == null)
            {
                string romsFolderPath = CommonDialogsHelper.ShowFolderBrowseDialog(
                    "Select the location of your ROMs folder:");
                if (!String.IsNullOrEmpty(romsFolderPath))
                {
                    config = new CatalogConfig(accessProcessor, configFolderPath, romsFolderPath);
                    await config.SaveAsync();
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
            var dialog = new SyncDeviceDialog(this.CatalogEditor.Catalog);
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            UpdateView();
        }

        private void buttonOpenROMsFolder_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.OpenFileViaShell(this.CatalogEditor.Config.RomsFolderPath);
        }

        private void buttonSettings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SettingsDialog(this.CatalogEditor.Config);
            dialog.Owner = this;
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
                UpdateView();
        }
    }
}
