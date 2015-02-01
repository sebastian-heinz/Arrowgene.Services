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
using System.Windows.Shapes;

namespace GuiServer.ViewImplementation.Windows
{
    /// <summary>
    /// Interaktionslogik für ScreenShotWindow.xaml
    /// </summary>
    public partial class ScreenShotWindow : Window
    {
        public ScreenShotWindow(BitmapImage image, string title)
        {
            InitializeComponent();
            this.imageView.Source = image;
            this.Title = title;
        }




    }
}
