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
using System.Drawing;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for GamesListviewItem.xaml
    /// </summary>
    public partial class GamesListviewItem : UserControl
    {
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

            textName.Text = String.IsNullOrEmpty(game.Name) ? "[UNKNOWN]" : game.Name;
            textFilename.Text = game.GamePath;
            textGenre.Text = game.Genre;
            textPublisher.Text = game.Publisher;
            textDeveloper.Text = game.Developer;
            textRating.Text = game.Rating > 0 ? game.Rating.ToString("0.0") : "n/a";
            textReleaseDate.Text = game.ReleaseDate.ToShortDateString("n/a");
            //textEmulator.Text = game.Emulator;
            textPlatform.Text = game.Platform.ToDisplayString();
            textDescription.Text = game.Description;
            image.Source = game.Image.ToImageSource();
        }
    }
}
