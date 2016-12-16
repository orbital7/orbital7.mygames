using Orbital7.Extensions;
using Orbital7.Extensions.Windows.Desktop.WPF;
using Orbital7.MyGames;
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
    /// Interaction logic for MatchGamesDialog.xaml
    /// </summary>
    public partial class MatchGamesDialog : Window
    {
        private bool ContinueMatching { get; set; } = false;

        private CatalogEditor CatalogEditor { get; set; }

        private List<Game> Games { get; set; }

        public MatchGamesDialog(CatalogEditor catalogEditor)
        {
            InitializeComponent();

            this.CatalogEditor = catalogEditor;
            this.Games = this.CatalogEditor.GatherIncompleteGames();

            foreach (var scraper in ScraperEngine.GatherScrapers(this.CatalogEditor.Config.FolderPath))
                comboScraper.Items.Add(scraper);
            if (comboScraper.Items.Count > 0)
                comboScraper.SelectedIndex = 0;
        }

        private async Task MatchNowAsync()
        {
            try
            {
                var engine = new ScraperEngine();
                var scraper = comboScraper.SelectedItem as Scraper;
                var gamesToMatch = CatalogEditor.IdentifyIncompleteGames(this.Games);

                // Show matching.
                this.ContinueMatching = true;
                buttonMatch.Content = "Stop";
                buttonDone.IsEnabled = false;

                // Match.
                int index = 0;
                textOutput.Clear();
                Console.SetOut(new TextBoxStreamWriter(textOutput));
                progress.Minimum = 0;
                progress.Maximum = gamesToMatch.Count;
                foreach (var game in gamesToMatch)
                {
                    index++;
                    Console.Write("Matching " + index + "/" + gamesToMatch.Count + ": " + game.GameFilename + "...");
                    textOutput.ScrollToEnd();
                    try
                    {
                        var query = await ScraperEngine.GetGameNameAsync(game.Platform, game.GameFilename);
                        var matchedGame = await engine.SearchExactAsync(scraper, game.Platform, query, game.GameFilename);
                        if (matchedGame != null)
                        {
                            this.CatalogEditor.MatchGame(game, matchedGame, 
                                await WebHelper.DownloadFileContentsAsync(matchedGame.ImageFilePath));
                            Console.WriteLine("MATCHED");
                        }
                        else
                        {
                            Console.WriteLine("Not Matched");
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("ERROR: " + ex.Message);
                    }
                    progress.Value++;

                    // TODO: All of this should be run on a background thread. This is a hack to quickly enable 
                    // start/stop functionality for testing.
                    System.Windows.Forms.Application.DoEvents();
                    if (!this.ContinueMatching)
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError(this, ex);
            }
            finally
            {
                this.ContinueMatching = false;
                buttonDone.IsEnabled = true;
                buttonMatch.Content = "Match Now";
                buttonMatch.IsEnabled = true;
            }

        }

        private void buttonDone_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonMatch_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ContinueMatching)
            {
                using (var worker = new BackgroundWorker())
                {
                    worker.DoWork += Worker_DoWork;
                    worker.RunWorkerAsync();
                }
            }
            else
            {
                this.ContinueMatching = false;
                buttonMatch.IsEnabled = false;
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            AsyncHelper.RunSync(() => MatchNowAsync());
        }
    }
}
