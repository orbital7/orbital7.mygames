using Orbital7.Extensions.NETFramework.WPF;
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
        private CatalogEditor CatalogEditor { get; set; }

        public Game Game { get; private set; }

        private GamesListview GamesListview { get; set; }

        private bool AllowSelection { get; set; }

        private System.Windows.Media.Brush BackgroundColor { get; set; }

        public bool IsSelected
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
                    border.Background = this.BackgroundColor;
                    border.BorderBrush = this.BackgroundColor;
                }
            }
        }

        public GamesListviewItem(GamesListview parent, CatalogEditor catalogEditor, 
            Game game, bool allowSelection, bool allowEditing)
        {
            InitializeComponent();

            this.BackgroundColor = this.Background;
            this.GamesListview = parent;
            this.CatalogEditor = catalogEditor;
            this.Game = game;
            this.AllowSelection = allowSelection;
            WPFHelper.SetVisible(editPanel, allowEditing);
            UpdateView();
        }

        private void UpdateView()
        {
            textName.Text = String.IsNullOrEmpty(this.Game.Name) ? "[UNKNOWN]" : this.Game.Name;
            textFilename.Text = this.Game.FileSummary;
            textGenre.Text = this.Game.Genre;
            textPublisher.Text = this.Game.Publisher;
            textDeveloper.Text = this.Game.Developer;
            textRating.Text = this.Game.Rating > 0 ? this.Game.Rating.ToString("0.0") : "n/a";
            textReleaseDate.Text = this.Game.ReleaseDate.ToShortDateString("n/a");
            textPlatform.Text = this.Game.Platform.ToDisplayString();
            textDescription.Text = this.Game.Description;
            image.Load(this.Game, true);
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
            ShowGameDialog(new EditGameDialog(this.CatalogEditor, this.Game));            
        }

        private void linkMatch_Click(object sender, RoutedEventArgs e)
        {
            ShowGameDialog(new MatchGameDialog(this.CatalogEditor, this.Game));
        }

        private void linkDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBoxHelper.AskQuestion(this, "Are you sure you want to delete the " +
                    this.Game.Platform.ToDisplayString() + " game " + this.Game.ToString() + "?"))
                {
                    this.CatalogEditor.DeleteGameAsync(this.Game);
                    this.GamesListview.Update();
                }
            }
            catch(Exception ex)
            {
                MessageBoxHelper.ShowError(this, ex);
            }
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.GamesListview.Select(this);
        }
    }
}
