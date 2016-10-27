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
    /// Interaction logic for MatchGameDialog.xaml
    /// </summary>
    public partial class MatchGameDialog : Window
    {
        private Game Game { get; set; }

        public MatchGameDialog(Game game)
        {
            InitializeComponent();
            App.SetWindowFont(this);

            // Record.
            this.Game = game;
            textQuery.Text = game.GameFilename;

            // Load the scrapers dynamically through reflection.
            var scrapers = ReflectionHelper.GetTypeInstances<Scraper>(typeof(Scraper), 
                FileSystemHelper.GetExecutingAssemblyFolder());
            if (scrapers.Count > 0)
            {
                foreach (var scraper in scrapers)
                    comboScraper.Items.Add(scraper);
                comboScraper.SelectedIndex = 0;
            }
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                resultsView.Clear();

                var engine = new ScraperEngine();
                var results = engine.Search(comboScraper.SelectedItem as Scraper, this.Game.Platform, 
                    textQuery.Text.Trim(), this.Game.GameFilename);
                if (results.Count > 0)
                    resultsView.Load(results);
                else
                    resultsView.Clear();
            }
            catch(Exception ex)
            {
                MessageBoxHelper.ShowError(this, ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void buttonMatch_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Finish.
            this.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void comboScraper_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            buttonSearch.IsEnabled = true;
        }
    }
}
