using Orbital7.Extensions.Windows.Desktop.WPF;
using Orbital7.MyGames;
using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Interaction logic for EditGameView.xaml
    /// </summary>
    public partial class EditGameView : UserControl
    {
        public Game Game { get; private set; }

        public EditGameView()
        {
            InitializeComponent();
        }

        public void Load(Game game)
        {
            if (game != null)
            {

                WPFHelper.FillComboBox(comboEmulator, game.GetAvailableEmulators(), game.Emulator);
                WPFHelper.FillComboBox(comboGameConfig, game.GetAvailableGameConfigs(), game.GameConfig);

                panel.IsEnabled = true;
                this.Game = game;
                this.DataContext = game;
                UpdateImage(game.Image.ToImageSource());
                this.Foreground = System.Windows.Media.Brushes.White;
            }
            else
            {
                Clear();
            }
        }

        public void Clear()
        {
            panel.IsEnabled = false;
            this.Game = null;
            this.DataContext = null;
            image.Source = null;
            blockPaste.Visibility = Visibility.Collapsed;
            this.Foreground = System.Windows.Media.Brushes.DarkGray;
        }

        private void UpdateImage(ImageSource imageSource)
        {
            image.Source = imageSource;
            blockPaste.Visibility = image.Source == null ? Visibility.Visible : Visibility.Collapsed;
        }

        private void menuItemCopy_Click(object sender, RoutedEventArgs e)
        {
            if (image.Source != null)
                System.Windows.Forms.Clipboard.SetImage(((BitmapSource)image.Source).ToBitmap());
        }

        private void menuItemPaste_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = System.Windows.Forms.Clipboard.GetImage() as Bitmap;
            if (bitmap != null)
            {
                this.Game.UpdateImage(bitmap);
                UpdateImage(this.Game.Image.ToImageSource());
            }
        }
    }
}
