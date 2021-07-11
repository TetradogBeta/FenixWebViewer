using CheckFenix.Core;
using Gabriel.Cat.S.Extension;
using Gabriel.Cat.S.Utilitats;
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
    public partial class SerieViewer : UserControl,IRefresh
    {
        static ImageSource ImgOn = Resource.corazonOn.ToImageSource();
        static ImageSource ImgOff = Resource.corazonOff.ToImageSource();
        static LlistaOrdenada<string, ImageSource> DicImgs = new LlistaOrdenada<string, ImageSource>();
        public SerieViewer()
        {
            InitializeComponent();
            AbrirSerieAlHacerClick = true;
            MostarFavorito = true;
            CargarImagenFull = false;
            btnFavorito.ImgOn = ImgOn;
            btnFavorito.ImgOff = ImgOff;
        }
        public SerieViewer(Serie serie):this()
        {
            Serie = serie;
        }
        public Serie Serie { get; set; }
        public bool AbrirSerieAlHacerClick { get; set; }
        public bool MostarFavorito { get; set; }
        public bool CargarImagenFull { get; set; }
        public void Refresh()
        {
            if (CargarImagenFull)
            {
                DicImgs.AddOrReplace(Serie.Picture.AbsoluteUri,  Serie.Picture.GetBitmap().ToImageSource());
            }
            else if (!DicImgs.ContainsKey(Serie.Picture.AbsoluteUri))
            {
                DicImgs.Add(Serie.Picture.AbsoluteUri, Serie.GetImage().ToImageSource());
            }
            imgSerie.Source = DicImgs[Serie.Picture.AbsoluteUri];
            imgSerie.ToolTip = Serie.Name;
            btnFavorito.EstadoOn = Serie.IsFavorito;
            if (MostarFavorito)
                btnFavorito.Visibility = Visibility.Visible;
            else btnFavorito.Visibility = Visibility.Hidden;

        }

        private void imgSerie_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(AbrirSerieAlHacerClick)
               new winSerie(Serie).Show();
        }

        private void btnFavorito_SwitchChanged(object sender, bool e)
        {
            //guardo el favorito
            Serie.IsFavorito = e;
            Serie.SaveFavoritos();
        }
    }
}
