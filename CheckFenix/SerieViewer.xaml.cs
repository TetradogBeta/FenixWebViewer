using CheckFenix.Core;
using Gabriel.Cat.S.Extension;
using System;
using System.Collections.Generic;
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
    /// Lógica de interacción para SerieViewer.xaml
    /// </summary>
    public partial class SerieViewer : UserControl
    {
        static ImageSource ImgOn = Resource.corazonOn.ToImageSource();
        static ImageSource ImgOff = Resource.corazonOff.ToImageSource();
        public SerieViewer()
        {
            InitializeComponent();
            
            btnFavorito.ImgOn = ImgOn;
            btnFavorito.ImgOff = ImgOff;
        }
        public SerieViewer(Serie serie):this()
        {
            Serie = serie;
            Refresh();
        }
        public Serie Serie { get; set; }
        public async Task Refresh()
        {
            imgSerie.SetImage(Serie.Image);
            imgSerie.ToolTip = Serie.Name;
            btnFavorito.EstadoOn = Serie.IsFavorito;
        }

        private void imgSerie_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            new winSerie(Serie).Show();
        }

        private void btnFavorito_SwitchChanged(object sender, bool e)
        {
            //guardo el favorito
            Serie.IsFavorito = e;
        }
    }
}
