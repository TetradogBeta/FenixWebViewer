using CheckFenix.Core;

using Gabriel.Cat.S.Extension;
using HtmlAgilityPack;

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

namespace CheckFenix
{

    /// <summary>
    /// Lógica de interacción para winSerie.xaml
    /// </summary>
    public partial class winSerie : Window
    {


        public winSerie()
        {
            InitializeComponent();


        }
        public winSerie(Serie serie) : this()
        {
            Serie = serie;
            Refresh();
        }
        public Serie Serie { get; set; }
        public async Task Refresh()
        {
            BitmapImage bmp = new BitmapImage();

            lstCapitulos.Items.Clear();
            for (int i = 1; i <= Serie.Total; i++)
                lstCapitulos.Items.Add($"Capitulo {i}");

            bmp.BeginInit();
            bmp.UriSource = Serie.Picture;
            bmp.EndInit();

            imgSerie.Source =bmp;

            Title = Serie.Name;
            tbDesc.Text = Serie.Description;
            if (Serie.Finalizada)
            {
                tbFechaNextOFinalizado.Text = nameof(Serie.Finalizada);
                gFechaOFinalizado.Background = Brushes.Tomato;
            }
            else
            {
                tbFechaNextOFinalizado.Text = Serie.NextCapterDate;
                gFechaOFinalizado.Background = Brushes.OrangeRed;
            }
            if (Serie.Total > 0)
                lstCapitulos.SelectedIndex=0;
        }



        private void lstCapitulos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCapitulos.SelectedIndex >= 0 && lstCapitulos.SelectedIndex < Serie.Total)
            {
                lstLinks.Items.Clear();
                lstLinks.Items.AddRange(Serie[lstCapitulos.SelectedIndex + 1].GetLinks().Where(l => l.StartsWith("http")).Select(l => new Url(l)));
            }
        }

        private void lstLinks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Equals(lstLinks.SelectedItem, default(Url)))
                (lstLinks.SelectedItem as Url).Uri.Abrir();
        }
    }
    public class Url
    {
        public Url(string url)
        {
            Uri = new Uri(url);
        }
        public Uri Uri { get; private set; }
        public override string ToString()
        {
            return Uri.Host;
        }
    }
}
