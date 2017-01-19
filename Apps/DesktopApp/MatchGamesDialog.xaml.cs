using Orbital7.Extensions;
using Orbital7.Extensions.Windows.Desktop.WPF;
using Orbital7.MyGames;
using Orbital7.MyGames.Scraping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for MatchGamesDialog.xaml
    /// </summary>
    public partial class MatchGamesDialog : Window
    {
        private Scraper Scraper { get; set; }

        private BackgroundWorker BackgroundWorker { get; set; }

        private List<Game> GamesToMatch { get; set; }

        private CatalogEditor CatalogEditor { get; set; }

        private List<Game> Games { get; set; }

        public MatchGamesDialog(CatalogEditor catalogEditor)
        {
            InitializeComponent();
            App.SetWindowFont(this);
            this.Closing += MatchGamesDialog_Closing;

            this.CatalogEditor = catalogEditor;
            this.Games = this.CatalogEditor.GatherIncompleteGames();

            foreach (var scraper in ScraperEngine.GatherScrapers(this.CatalogEditor.Config.FolderPath))
                comboScraper.Items.Add(scraper);
            if (comboScraper.Items.Count > 0)
                comboScraper.SelectedIndex = 0;
        }

        private void MatchGamesDialog_Closing(object sender, CancelEventArgs e)
        {
            if (this.BackgroundWorker != null)
            {
                if (MessageBoxHelper.AskQuestion(this, "Bulk-Matching in progress; cancel?", true))
                    this.BackgroundWorker.CancelAsync();
                else
                    e.Cancel = true;
            }
        }

        private async Task MatchNowAsync(DoWorkEventArgs e)
        {
            int index = 0;
            var engine = new ScraperEngine();

            // Match.
            foreach (var game in this.GamesToMatch)
            {
                // Exit if cancellation pending.
                if (this.BackgroundWorker.CancellationPending)
                    break;

                index++;
                ReportProgress(index, "Matching " + index + "/" + this.GamesToMatch.Count + ": " + game.GameFilename + "...");
                try
                {
                    var query = await ScraperEngine.GetGameNameAsync(game.Platform, game.GameFilename);
                    var matchedGame = await engine.SearchExactAsync(this.Scraper, game.Platform, query, game.GameFilename);
                    if (matchedGame != null)
                    {
                        await this.CatalogEditor.MatchGameAsync(game, matchedGame, 
                            await HttpHelper.DownloadFileContentsAsync(matchedGame.ImageFilePath));
                        ReportProgress(index, "MATCHED\n");
                    }
                    else
                    {
                        ReportProgress(index, "Not Matched\n");
                    }
                }
                catch(Exception ex)
                {
                    ReportProgress(index, "ERROR: " + ex.Message + "\n");
                }
            }
        }

        private void ReportProgress(int index, string consoleOut)
        {
            this.BackgroundWorker.ReportProgress(Convert.ToInt32(index / this.GamesToMatch.Count * 100), consoleOut);
        }

        private void buttonDone_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonMatch_Click(object sender, RoutedEventArgs e)
        {
            if (this.BackgroundWorker == null)
            {
                this.Scraper = comboScraper.SelectedItem as Scraper;
                buttonMatch.Content = "Stop";
                buttonDone.IsEnabled = false;
                textOutput.Clear();
                Console.SetOut(new TextBoxStreamWriter(textOutput));
                this.GamesToMatch = CatalogEditor.IdentifyIncompleteGames(this.Games);
                progress.Minimum = 0;
                progress.Maximum = this.GamesToMatch.Count;

                this.BackgroundWorker = new BackgroundWorker();
                this.BackgroundWorker.WorkerReportsProgress = true;
                this.BackgroundWorker.WorkerSupportsCancellation = true;
                this.BackgroundWorker.DoWork += Worker_DoWork;
                this.BackgroundWorker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                this.BackgroundWorker.ProgressChanged += Worker_ProgressChanged;
                this.BackgroundWorker.RunWorkerAsync();
            }
            else
            {
                buttonMatch.Content = "Stopping...";
                buttonMatch.IsEnabled = false;
                this.BackgroundWorker.CancelAsync();
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress.Value = progress.Maximum * e.ProgressPercentage / 100;
            textOutput.ScrollToEnd();
            Console.Write(e.UserState.ToString());
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.BackgroundWorker = null;
            buttonDone.IsEnabled = true;
            buttonMatch.Content = "Match Now";
            buttonMatch.IsEnabled = true;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            AsyncHelper.RunSync(() => MatchNowAsync(e));
        }
    }
}
