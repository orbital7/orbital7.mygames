﻿using Orbital7.Extensions.Windows.Desktop.WPF;
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
    /// Interaction logic for MatchGamesDialog.xaml
    /// </summary>
    public partial class MatchGamesDialog : Window
    {
        private bool ContinueMatching = false;

        private List<Game> Games { get; set; }

        public MatchGamesDialog(List<Game> games)
        {
            InitializeComponent();

            this.Games = games;

            foreach (var scraper in ScraperEngine.GatherScrapers())
                comboScraper.Items.Add(scraper);
            if (comboScraper.Items.Count > 0)
                comboScraper.SelectedIndex = 0;
        }

        private void MatchNow()
        {
            try
            {
                var engine = new ScraperEngine();
                var scraper = comboScraper.SelectedItem as Scraper;
                var gamesToMatch = Catalog.IdentifyIncompleteGames(this.Games);

                // Show matching.
                this.ContinueMatching = true;
                buttonMatch.Content = "Stop";
                buttonDone.IsEnabled = false;

                // Match.
                int index = 0;
                textOutput.Clear();
                progress.Minimum = 0;
                progress.Maximum = gamesToMatch.Count;
                foreach (var game in gamesToMatch)
                {
                    index++;
                    textOutput.Text += "Matching " + index + "/" + gamesToMatch.Count + ": " + game.GameFilename + "...";
                    textOutput.ScrollToEnd();
                    try
                    {
                        var query = ScraperEngine.GetGameName(game.Platform, game.GameFilename);
                        var matchedGame = engine.SearchExact(scraper, game.Platform, query, game.GameFilename);
                        if (matchedGame != null)
                        {
                            game.Match(matchedGame);
                            textOutput.Text += "MATCHED\n";
                        }
                        else
                        {
                            textOutput.Text += "Not Matched\n";
                        }
                    }
                    catch(Exception ex)
                    {
                        textOutput.Text += "ERROR: " + ex.Message;
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
                MatchNow();
            }
            else
            {
                this.ContinueMatching = false;
                buttonMatch.IsEnabled = false;
            }
        }
    }
}
