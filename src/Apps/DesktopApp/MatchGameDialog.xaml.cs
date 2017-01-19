using Orbital7.Extensions;
using Orbital7.Extensions.Windows;
using Orbital7.Extensions.Windows.Desktop.WPF;
using Orbital7.MyGames;
using Orbital7.MyGames.Scraping;
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
        private CatalogEditor CatalogEditor { get; set; }
        private Game GameToMatch { get; set; }

        public Game MatchedGame { get; private set; }

        public MatchGameDialog(CatalogEditor catalogEditor, Game game)
        {
            InitializeComponent();
            App.SetWindowFont(this);
            this.CatalogEditor = catalogEditor;

            AsyncHelper.RunSync(() => LoadAsync(game));
        }

        public async Task LoadAsync(Game game)
        {
            // Record.
            this.GameToMatch = game;
            textQuery.Text = await ScraperEngine.GetGameNameAsync(game.Platform, game.GameFilename);
            comboPlatform.SelectedItem = game.Platform;

            // Load the scrapers.
            var scrapers = ScraperEngine.GatherScrapers(this.CatalogEditor.Config.FolderPath);
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
                var results = AsyncHelper.RunSync<List<Game>>(() => engine.SearchAsync((Scraper)comboScraper.SelectedItem, (Platform)comboPlatform.SelectedItem, 
                    textQuery.Text.Trim(), this.GameToMatch.GameFilename));
                if (results.Count > 0)
                    resultsView.Load(this.CatalogEditor, results);
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
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                if (this.MatchedGame != null)
                {
                    this.CatalogEditor.MatchGameAsync(this.GameToMatch, this.MatchedGame, editGameView.UpdatedGameImageContents);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBoxHelper.ShowError(this, "No game has been selected for Match");
                }
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

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void comboScraper_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            buttonSearch.IsEnabled = true;
        }

        private void resultsView_SelectionChanged()
        {
            this.MatchedGame = resultsView.SelectedGame;
            editGameView.Load(this.CatalogEditor, this.MatchedGame, false);
            buttonMatch.IsEnabled = this.MatchedGame != null;
        }
    }
}
