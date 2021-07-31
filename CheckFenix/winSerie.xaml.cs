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

            lstCapitulos.Items.Clear();
            for (int i = 1; i <= Serie.Total; i++)
                lstCapitulos.Items.Add($"Capitulo {i}");


            imgSerie.Serie=Serie;
            imgSerie.Refresh();

            if (Serie.Total == 0)
            {
                gCapitulos.Visibility =  Visibility.Hidden;
                rdCapitulos.Height = new GridLength(0);
                Title = $"Próximamente {Serie.Name}";
            }
            else
            {
                gCapitulos.Visibility = Visibility.Visible;
                Title =Serie.Name;
            }
      
           
            btnPrecuela.IsEnabled = Serie.HasPrecuela;
            btnSecuela.IsEnabled = Serie.HasSecuela;

            tbDesc.Text = Serie.Description;
            if (Serie.Finalizada)
            {
                tbFechaNextOFinalizado.Text = nameof(Serie.Finalizada);
                gFechaOFinalizado.Background = Brushes.Tomato;
            }
            else
            {
                tbFechaNextOFinalizado.Text = Serie.NextChapter.Value.ToLongDateString();
                gFechaOFinalizado.Background = Brushes.OrangeRed;
            }
            if (Serie.Total > 0)
                lstCapitulos.SelectedIndex=0;
        }



        private void lstCapitulos_SelectionChanged(object sender=default, SelectionChangedEventArgs e=default)
        {
            if (lstCapitulos.SelectedIndex >= 0 && lstCapitulos.SelectedIndex < Serie.Total)
            {
                lstLinks.Items.Clear();
                lstLinks.Items.AddRange(Serie[lstCapitulos.SelectedIndex + 1].Links.Where(l=>l.StartsWith("http")).Select(l => new Url(l)));
            }
        }

        private void lstLinks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Uri uri;
            if (!Equals(lstLinks.SelectedItem, default(Url)))
            {
                uri= (lstLinks.SelectedItem as Url).Uri;
                uri.Abrir();
            }
        }


        private void btnPrecuela_Click(object sender, RoutedEventArgs e)
        {
            new winSerie(Serie.Precuela).Show();
        }

        private void btnSecuela_Click(object sender, RoutedEventArgs e)
        {
            new winSerie(Serie.Secuela).Show();
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
