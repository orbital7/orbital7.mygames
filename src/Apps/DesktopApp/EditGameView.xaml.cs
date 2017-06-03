using Orbital7.Extensions.NETFramework.WPF;
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

        public byte[] UpdatedGameImageContents
        {
            get { return image.UpdatedGameImageContents; }
        }

        public EditGameView()
        {
            InitializeComponent();
        }

        public void Load(CatalogEditor catalogEditor, Game game, bool gameExists)
        {
            if (game != null)
            {

                WPFHelper.FillComboBox(comboEmulator, catalogEditor.GetAvailableEmulators(game.Platform), game.Emulator);
                WPFHelper.FillComboBox(comboGameConfig, catalogEditor.GetAvailableGameConfigs(game.Platform), game.GameConfig);

                panel.IsEnabled = true;
                this.Game = game;
                this.DataContext = game;
                image.Load(game, gameExists);
                UpdateVisibility(game.HasImage);
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
            image.Clear();
            image.Visibility = Visibility.Visible;
            blockPaste.Visibility = Visibility.Collapsed;
            this.Foreground = System.Windows.Media.Brushes.DarkGray;
        }

        private void UpdateVisibility(bool hasImage)
        {
            image.Visibility = hasImage ? Visibility.Visible : Visibility.Collapsed;
            blockPaste.Visibility = !hasImage ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateImage(BitmapSource source, bool isUpdated, string imageFileExtension)
        {
            bool hasImage = image.UpdateImage(source, isUpdated, imageFileExtension, 
                Convert.ToInt32(source.Width), Convert.ToInt32(source.Height));
            UpdateVisibility(hasImage);
        }

        private void menuItemCopy_Click(object sender, RoutedEventArgs e)
        {
            if (image.ImageSource != null)
                Clipboard.SetImage((BitmapSource)image.ImageSource);
        }

        private void menuItemPasteAsJpg_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
                UpdateImage(Clipboard.GetImage(), true, ".jpg");
        }

        private void menuItemPasteAsPng_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
                UpdateImage(Clipboard.GetImage(), true, ".png");
        }
    }
}
