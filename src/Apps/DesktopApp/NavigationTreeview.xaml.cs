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

namespace DesktopApp
{
    public delegate void NavigationTreeviewItemSelectedHandler(NavigationTreeviewModelItem item);

    /// <summary>
    /// Interaction logic for NavigationTreeview.xaml
    /// </summary>
    public partial class NavigationTreeview : UserControl
    {
        public event NavigationTreeviewItemSelectedHandler NavigationTreeviewItemSelected;

        public NavigationTreeview()
        {
            InitializeComponent();
        }

        public void Load(Catalog catalog)
        {
            treeview.ItemsSource = new NavigationTreeviewModel(catalog).Items;
        }

        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.NavigationTreeviewItemSelected?.Invoke(treeview.SelectedItem as NavigationTreeviewModelItem);
        }
    }
}
