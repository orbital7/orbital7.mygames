using Orbital7.Extensions.Windows;
using Orbital7.Extensions.Windows.Desktop.WPF;
using Orbital7.MyGames;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for GameImageView.xaml
    /// </summary>
    public partial class GameImageView : UserControl
    {
        public Game Game { get; private set; }

        private bool ImageUpdated { get; set; }

        private string ImageFileExtension { get; set; }

        public byte[] UpdatedGameImageContents
        {
            get
            {
                if (image.Source != null)
                    return ((BitmapSource)image.Source).ToByteArray(this.ImageFileExtension);
                else
                    return null;
            }
        }

        public ImageSource ImageSource
        {
            get { return image.Source; }
        }

        public GameImageView()
        {
            InitializeComponent();
        }

        public void Load(Game game, bool gameExists)
        {
            this.Game = game;

            if (game.HasImage)
            {
                UpdateImage(MediaHelper.LoadBitmapImage(game.ImageFilePath), !gameExists,
                    System.IO.Path.GetExtension(game.ImagePath), game.ImageWidth, game.ImageHeight);

                
            }
            else
            {
                Clear();
            }
        }

        public void Clear()
        {
            image.Source = null;
        }

        internal bool UpdateImage(BitmapSource source, bool isUpdated, string imageFileExtension, int width, int height)
        {
            image.Source = source;
            this.ImageUpdated = isUpdated;
            this.ImageFileExtension = imageFileExtension;

            if (width > 0 && height > 0)
            {
                image.Stretch = Stretch.Fill;

                // Portrait.
                if (height > width)
                {
                    image.Height = this.Height;
                    image.Width = width * this.Height / height;
                    if (image.Width > this.Width)
                    {
                        image.Width = this.Width;
                        image.Height = height * this.Width / width;
                    }
                }
                // Landscape.
                else
                {
                    image.Width = this.Width;
                    image.Height = height * this.Width / width;
                    if (image.Height > this.Height)
                    {
                        image.Height = this.Height;
                        image.Width = width * this.Height / height;
                    }
                }
            }
            else
            {
                image.Stretch = Stretch.Uniform;
            }

            return image.Source != null;
        }
    }
}
