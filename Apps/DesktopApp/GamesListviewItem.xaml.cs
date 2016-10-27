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
using System.Drawing;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for GamesListviewItem.xaml
    /// </summary>
    public partial class GamesListviewItem : UserControl
    {
        private Game Game { get; set; }

        public bool Selected
        {
            set
            {
                if (value)
                {
                    border.Background = MediaHelper.GetVerticalGradientBrush("#EBFFFF", "#BEFFFF");
                    border.BorderBrush = System.Windows.Media.Brushes.Blue;
                }
                else
                {
                    border.Background = System.Windows.Media.Brushes.White;
                    border.BorderBrush = System.Windows.Media.Brushes.White;
                }
            }
        }

        public GamesListviewItem(Game game)
        {
            InitializeComponent();

            this.Game = game;
            UpdateView();
        }

        private void UpdateView()
        {
            textName.Text = String.IsNullOrEmpty(this.Game.Name) ? "[UNKNOWN]" : this.Game.Name;
            textFilename.Text = this.Game.GamePath;
            textGenre.Text = this.Game.Genre;
            textPublisher.Text = this.Game.Publisher;
            textDeveloper.Text = this.Game.Developer;
            textRating.Text = this.Game.Rating > 0 ? this.Game.Rating.ToString("0.0") : "n/a";
            textReleaseDate.Text = this.Game.ReleaseDate.ToShortDateString("n/a");
            //textEmulator.Text = this.Game.Emulator;
            textPlatform.Text = this.Game.Platform.ToDisplayString();
            textDescription.Text = this.Game.Description;
            image.Source = this.Game.Image.ToImageSource();
        }

        private void ShowGameDialog(Window dialog)
        {
            dialog.Owner = Window.GetWindow(this);
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
                UpdateView();
        }

        private void linkEdit_Click(object sender, RoutedEventArgs e)
        {
            ShowGameDialog(new EditGameDialog(this.Game));            
        }

        private void linkMatch_Click(object sender, RoutedEventArgs e)
        {
            ShowGameDialog(new MatchGameDialog(this.Game));
        }

        private void linkDelete_Click(object sender, RoutedEventArgs e)
        {
            // TODO.
        }
    }
}
