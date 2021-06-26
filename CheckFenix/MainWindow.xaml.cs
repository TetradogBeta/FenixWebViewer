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
        const int CAPITULOSACTUALESPAGE = 0;
        const int SERIESACTUALESPAGE = 1;
        const int ALLSERIESPAGE = 2;
        const int FAVORITOSPAGE = 3;
        const string CARGANDO = "Cargando";
        const string TITULO = "AnimeFenix Desktop 1.4beta";

        Task initSeries,initAllSeries;
        public MainWindow()
        {
            InitializeComponent();
            Title = TITULO;
            initSeries = new Task(() => { SeriesActuales.ToArray(); });
            initSeries.Start();
            initAllSeries = new Task(() => { AllSeries.Take(visorTodasLasSeries.TotalPage * 3).ToArray(); });
            initAllSeries.Start();
        }



        public IEnumerable<Capitulo> CapitulosActuales => Capitulo.GetCapitulosActuales(HtmlAndLinksDic.URLANIMEFENIX);
        public IEnumerable<Serie> SeriesActuales
        {
            get
            {
                SortedList<string, string> dic = new SortedList<string, string>();
                return CapitulosActuales.Select(s => s.Parent).Where(c =>
                {
                    bool exist = dic.ContainsKey(c.Name);
                    if (!exist)
                        dic.Add(c.Name, c.Name);
                    return !exist;
                });
            }
        }
        public IEnumerable<Serie> AllSeries => Serie.GetAllSeries();

        public IEnumerable<Serie> Favorites => Serie.GetFavoritos();

        private void tbMain_SelectionChanged(object sender = null, SelectionChangedEventArgs e = null)
        {

            switch (tbMain.SelectedIndex)
            {
                case CAPITULOSACTUALESPAGE:
                    visorCapitulosActuales.Capitulos = CapitulosActuales;
                    Title = CARGANDO;
                    visorCapitulosActuales.Refresh().ContinueWith(AcabaDeCargar());

                    break;
                case SERIESACTUALESPAGE:
                    visorSeriesActuales.Series = SeriesActuales;
                    Title = CARGANDO;
                    initSeries.Wait();
                    visorSeriesActuales.Refresh().ContinueWith(AcabaDeCargar());


                    break;
                case ALLSERIESPAGE:

                    visorTodasLasSeries.Series = AllSeries;
                    Title = CARGANDO;
                    initAllSeries.Wait();
                    visorTodasLasSeries.Refresh().ContinueWith(AcabaDeCargar());
                    break;
                case FAVORITOSPAGE:

                    visorSeriesFavoritas.Series = Favorites;
                    Title = CARGANDO;
                    visorSeriesFavoritas.Refresh().ContinueWith(AcabaDeCargar());
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
          
            if (e.Key == Key.F5)
            {
    
                tbMain_SelectionChanged();
            }
            else if (tbMain.SelectedIndex == ALLSERIESPAGE)
            {
                if (e.Key.Equals(Key.Up))
                {
                    if (visorTodasLasSeries.Page > 0)
                    {
                        visorTodasLasSeries.Page--;
                        Title = CARGANDO;
                        visorTodasLasSeries.Refresh().ContinueWith(AcabaDeCargar());

                    }
                }
                else if (e.Key.Equals(Key.Down))
                {
                    visorTodasLasSeries.Page++;
                    Title = CARGANDO;
                    visorTodasLasSeries.Refresh().ContinueWith(AcabaDeCargar());
                }
            }
            else if (tbMain.SelectedIndex == FAVORITOSPAGE)
            {
                if (e.Key.Equals(Key.Up))
                {
                    if (visorSeriesFavoritas.Page > 0)
                    {
                        visorSeriesFavoritas.Page--;
                        Title = CARGANDO;
                        visorSeriesFavoritas.Refresh().ContinueWith(AcabaDeCargar());

                    }
                }
                else if (e.Key.Equals(Key.Down))
                {
                    visorSeriesFavoritas.Page++;
                    Title = CARGANDO;
                    visorSeriesFavoritas.Refresh().ContinueWith(AcabaDeCargar());
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
            HtmlAndLinksDic.SaveCache();

           
        }
    }
}
