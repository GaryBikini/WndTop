using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace WndTop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        public MainWindow()
        {
            InitializeComponent();
        }

     
        private void btnTop_Checked(object sender, RoutedEventArgs e)
        {
            btnRelease.IsChecked = false;
            Utils.fun(this);
        }

        private void btnRelease_Checked(object sender, RoutedEventArgs e)
        {
            btnTop.IsChecked = false;
            Utils.fun(this);
        }

    }
}
