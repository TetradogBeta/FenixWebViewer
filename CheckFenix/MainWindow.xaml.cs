using CargarBD;
using CheckFenix.CargarBD;
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
        const string URLANIMEFENIX = "https://www.animefenix.com/";
        const int CAPITULOSACTUALESPAGE = 0;
        const int SERIESACTUALESPAGE = 1;
        const int ALLSERIESPAGE = 2;
        const int FAVORITOSPAGE = 3;
        const int PROXIMANENTEPAGE = 4;

        const string CARGANDO = "Cargando";
        const string TITULO = "AnimeFenix Desktop 1.4 Beta 1";
        const string FINALIZADAS = "Finalizadas";

        public MainWindow()
        {
            Title = TITULO;
            InitializeComponent();

            InitUpdate = new Task(new Action(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    tbFinalizadas.Header = CARGANDO;
                    tbFinalizadas.IsEnabled = false;
                }));

                Program.Main(URLANIMEFENIX);
                Context = new Context();

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    tbFinalizadas.Header = FINALIZADAS;
                    tbFinalizadas.IsEnabled = true;
                }));

            }));
            InitUpdate.Start();
        }

        Task InitUpdate { get; set; }
        public Context Context { get; set; }

        public IEnumerable<Capitulo> CapitulosActuales => Capitulo.GetCapitulosHome(URLANIMEFENIX);
        public IEnumerable<Serie> SeriesEnEmision => Serie.GetSeriesEmision(URLANIMEFENIX).Where(s => s.Total > 0);
        public IEnumerable<Serie> SeriesEnCuarentena => Serie.GetSeriesCuarentena(URLANIMEFENIX);
        public IEnumerable<Serie> ProximasSeries => Serie.GetSeriesEmision(URLANIMEFENIX).Where(s => s.Total == 0);
        public IEnumerable<Serie> SeriesFinalizadas
        {
            get
            {
                InitUpdate.Wait();
                return Context.Series.Select(serie => new Serie(new Uri(serie.Pagina)) { Picture = new Uri(serie.Picture),Name=serie.Name });
            }
        }

        public IEnumerable<Serie> Favorites => Serie.GetFavoritos();

        private void tbMain_SelectionChanged(object sender = null, SelectionChangedEventArgs e = null)
        {

            switch (tbMain.SelectedIndex)
            {
                case CAPITULOSACTUALESPAGE:
                    visorCapitulosActuales.Capitulos = CapitulosActuales;
                    Title = CARGANDO;
                    visorCapitulosActuales.Refresh().ContinueWith(AcabaDeCargar()).Wait();

                    break;
                case SERIESACTUALESPAGE:
                    visorSeriesEnEmision.Series = SeriesEnEmision;
                    Title = CARGANDO;
                    visorSeriesEnEmision.Refresh().ContinueWith(AcabaDeCargar()).Wait();


                    break;
                case ALLSERIESPAGE:

                    visorSeriesFinalizadas.Series = SeriesFinalizadas;
                    Title = CARGANDO;
                    visorSeriesFinalizadas.Refresh().ContinueWith(AcabaDeCargar()).Wait();
                    break;
                case FAVORITOSPAGE:

                    visorSeriesFavoritas.Series = Favorites;
                    Title = CARGANDO;
                    visorSeriesFavoritas.Refresh().ContinueWith(AcabaDeCargar()).Wait();
                    break;
                case PROXIMANENTEPAGE:

                    visorSeriesParaSalir.Series = ProximasSeries;
                    Title = CARGANDO;
                    visorSeriesParaSalir.Refresh().ContinueWith(AcabaDeCargar()).Wait();
                    break;
            }

        }

        private Action<Task> AcabaDeCargar()
        {
            Action<Task> act = async (t) => { await AcabarTask(); };
            return act;
        }
        async Task AcabarTask()
        {
            Action act = () => Title = TITULO;
            await Dispatcher.BeginInvoke(act);
        }
        private void Window_KeyDown_Gen(object sender, KeyEventArgs e)
        {
            Window_KeyDown((tbMain.SelectedItem as TabItem).Content, e);
        }
        private void visorSeriesEnEmision_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MoverVisor((tbMain.SelectedItem as TabItem).Content, e.Delta > 0 ? Key.Up : Key.Down);
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {

                tbMain_SelectionChanged();
            }
            else
            {
                MoverVisor(sender, e.Key);
            }

        }
        void MoverVisor(object sender, Key e)
        {
            VisorSeries visor = sender as VisorSeries;
            if (!Equals(visor, default))
            {
                if (e.Equals(Key.Up))
                {
                    if (visor.Page > 0)
                    {
                        visor.Page--;
                        Title = CARGANDO;
                        visor.Refresh().ContinueWith(AcabaDeCargar());

                    }
                }
                else if (e.Equals(Key.Down))
                {
                    visor.Page++;
                    Title = CARGANDO;
                    visor.Refresh().ContinueWith(AcabaDeCargar());
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Title = "Guardando Favoritos!";
            Serie.SaveFavoritos();
            Title = "Guardando Cache!";
            Capitulo.SaveCache();
            Serie.SaveCache();
        


        }


    }
}
