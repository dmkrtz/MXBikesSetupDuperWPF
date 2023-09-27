using ModernWpf.Controls;
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

namespace MXBikesSetupDuperWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /*private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(listBox.ItemContainerGenerator.ContainerFromItem(listBox.SelectedItem) is ListBoxItem lbItem))
                return;

            string itemName = lbItem.Content?.ToString();

            if (string.IsNullOrEmpty(itemName))
                return;

            main.Children.OfType<ModernWpf.Controls.SimpleStackPanel>().ToList().ForEach(b => b.Visibility = Visibility.Collapsed);

            if (main.FindName("Menu_" + itemName) is ModernWpf.Controls.SimpleStackPanel selectedPanel)
            {
                selectedPanel.Visibility = Visibility.Visible;
                MessageBox.Show(itemName);
            }
        }*/

    }
}
