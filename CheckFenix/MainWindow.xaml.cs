using CheckFenix.Core;
using Gabriel.Cat.S.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace CheckFenix
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string URL = "https://www.animefenix.com/";
        public MainWindow()
        {

    
            InitializeComponent();
            LoadCapitulos();
        }
        public void LoadCapitulos()
        {
            Image img;
            foreach (Capitulo capitulo in Capitulo.GetCapitulos(URL))
            {
                img = new Image() { Source =new BitmapImage(capitulo.Picture) };
                img.Tag = capitulo;
                img.MouseLeftButtonDown += (s, e) =>
                {
                    string urlMega = ((s as Image).Tag as Capitulo).GetLinks().Where(l => l.Contains("mega.nz")).FirstOrDefault();
                    if (!string.IsNullOrEmpty(urlMega))
                    {
                        new Uri(urlMega).Abrir();
                    }
                };
                ugMain.Children.Add(img);
            }
        }
    }
}
