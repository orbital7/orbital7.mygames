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
        public MainWindow()
        {
            InitializeComponent();
            App.SetWindowFont(this);

            var config = LoadConfig();
            if (config != null)
            {
                var catalog = new Catalog(config.GamesFolderPath);
                navigationTreeview.Load(catalog);
            }
            else
            {
                this.Close();
            }
        }

        public Config LoadConfig()
        {
            var config = Config.Load();
            if (config == null)
            {
                string gamesPath = Orbital7.Extensions.Windows.Desktop.WinForms.CommonDialogsHelper.ShowFolderBrowseDialog(
                    "Select the location of your games folder:");
                if (!String.IsNullOrEmpty(gamesPath))
                {
                    config = new Config(gamesPath);
                    config.Save();
                }
            }

            return config;
        }

        private void navigationTreeview_NavigationTreeviewItemSelected(NavigationTreeviewModelItem item)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            if (item != null && item.GameList != null)
                gamesListview.Load((from Game x in item.GameList select x).ToList());
            else
                gamesListview.Clear();

            Mouse.OverrideCursor = null;
        }
    }
}
