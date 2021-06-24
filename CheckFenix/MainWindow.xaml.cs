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
        const int CAPITULOSACTUALESPAGE = 0;
        const int SERIESACTUALESPAGE = 1;
        const int ALLSERIESPAGE = 2;
        const string CARGANDO = "Cargando";
        const string TITULO = "AnimeFenix Desktop 1.1";
        public MainWindow()
        {

            InitializeComponent();
            Title = TITULO;

        }



        public IEnumerable<Capitulo> CapitulosActuales => Capitulo.GetCapitulosActuales(URL);
        public IEnumerable<Serie> SeriesActuales => CapitulosActuales.Select(s => s.Parent).Distinct();

        public IEnumerable<Serie> AllSeries => Serie.GetAllSeries();



        private void tbMain_SelectionChanged(object sender=null, SelectionChangedEventArgs e=null)
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
                    visorSeriesActuales.Refresh().ContinueWith(AcabaDeCargar());
                    break;
                case ALLSERIESPAGE:
                    visorTodasLasSeries.Series = AllSeries;
                    Title = CARGANDO;
                    visorTodasLasSeries.Refresh().ContinueWith(AcabaDeCargar());
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
            Action act=()=> Title = TITULO;
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
        }
    }
}
