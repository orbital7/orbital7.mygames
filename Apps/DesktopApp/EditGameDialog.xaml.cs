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
using System.Windows.Shapes;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for EditGameDialog.xaml
    /// </summary>
    public partial class EditGameDialog : Window
    {
        private Game Game { get; set; }

        public EditGameDialog(Game game)
        {
            InitializeComponent();
            App.SetWindowFont(this);

            this.Game = game;
            editGameView.Load(game.GameList.Config.FolderPath, game);
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                this.Game.Save(editGameView.GameImage);
                this.DialogResult = true;
            }
            catch(Exception ex)
            {
                MessageBoxHelper.ShowError(this, ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            this.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
