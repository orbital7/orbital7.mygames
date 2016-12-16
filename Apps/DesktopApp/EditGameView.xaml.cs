using Orbital7.Extensions.Windows.Desktop.WPF;
using Orbital7.MyGames;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public byte[] GameImage
        {
            get
            {
                if (image.Source != null)
                    return ((BitmapImage)image.Source).ToBitmap().ToByteArray();
                else
                    return null;
            }
        }

        public void Load(CatalogEditor catalogEditor, Game game)
        {
            if (game != null)
            {

                WPFHelper.FillComboBox(comboEmulator, catalogEditor.GetAvailableEmulators(game.Platform), game.Emulator);
                WPFHelper.FillComboBox(comboGameConfig, catalogEditor.GetAvailableGameConfigs(game.Platform), game.GameConfig);

                panel.IsEnabled = true;
                this.Game = game;
                this.DataContext = game;
                UpdateImage(MediaHelper.GetBitmapImageSource(game.ImageFilePath));
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
                UpdateImage(bitmap.ToImageSource());
        }
    }
}
