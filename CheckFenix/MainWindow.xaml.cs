using CargarBD;
using CheckFenix.CargarBD;
using CheckFenix.Core;
using Gabriel.Cat.S.Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
        const string TITULO = "AnimeFenix Desktop 1.5b";
        const string FINALIZADAS = "Finalizadas";

        IEnumerable<Capitulo> capitulosAnt;
        IEnumerable<Capitulo> capitulos;
        IEnumerable<Serie> enEmision;
        IEnumerable<Serie> proximasSeries;
        TimeSpan? NotificationTimeOut { get; set; }
        Timer TimerAutoRefresh { get; set; }
        bool ErrorMostrado { get; set; }
        public MainWindow()
        {
            NotificationTimeOut = TimeSpan.FromMinutes(3);
            Manager = new Notifications.Wpf.Core.NotificationManager(Dispatcher);
            TimerAutoRefresh = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
            Title = TITULO;
            InitializeComponent();
            TimerAutoRefresh.Elapsed += AutoRefresh;

            InitUpdate = new Task(new Action(() =>
            {
                Context context;
                try
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        tbFinalizadas.Header = CARGANDO;
                        tbFinalizadas.IsEnabled = false;
                    }));

                    Program.Main(URLANIMEFENIX);
                    context = new Context();
                    SeriesFinalizadas = context.Series.Select(serie => new Serie(new Uri(serie.Pagina)) { Picture = new Uri(serie.Picture), Name = serie.Name }).ToList();
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        visorSeriesFinalizadas.Series = SeriesFinalizadas;
                        visorSeriesFinalizadas.Visor.Refresh().ContinueWith(AcabaDeCargar()).Wait();
                        tbFinalizadas.Header = FINALIZADAS;
                        tbFinalizadas.IsEnabled = true;
                        TimerAutoRefresh.Start();
                    }));
                }
                catch
                {

                    MostrarErrorSinInternet();
                }

            }));
            InitUpdate.Start();
        }


        Notifications.Wpf.Core.NotificationManager Manager { get; set; }
        Task InitUpdate { get; set; }

        public IEnumerable<Capitulo> CapitulosActuales
        {
            get
            {
                if (Equals(capitulos, default))
                    capitulos = Capitulo.GetCapitulosHome(URLANIMEFENIX);
                return capitulos;
            }
        }

        public IEnumerable<Serie> SeriesEnEmision
        {
            get
            {

                if (Equals(enEmision, default))
                    enEmision = Serie.GetSeriesEmision(URLANIMEFENIX).Where(s => s.Total > 0);
                return enEmision;
            }
        }

        public IEnumerable<Serie> SeriesEnCuarentena => Serie.GetSeriesCuarentena(URLANIMEFENIX);
        public IEnumerable<Serie> ProximasSeries
        {
            get
            {
                if (Equals(proximasSeries, default))
                    proximasSeries = Serie.GetSeriesEmision(URLANIMEFENIX).Where(s => s.Total == 0);
                return proximasSeries;

            }
        }

        public IList<Serie> SeriesFinalizadas { get; set; }


        public IEnumerable<Serie> Favorites => Serie.GetFavoritos();

        private void tbMain_SelectionChanged(object sender = null, SelectionChangedEventArgs e = null)
        {
            IVisor visor;
            IEnumerable items = default;
            switch (tbMain.SelectedIndex)
            {
                case CAPITULOSACTUALESPAGE:
                    if (Equals(capitulos, default))
                    {
                        items = CapitulosActuales;
                    }
                    break;
                case SERIESACTUALESPAGE:
                    if (Equals(enEmision, default))
                    {
                        items = SeriesEnEmision;
                    }

                    break;
                case FAVORITOSPAGE:

                    items = Favorites;

                    break;
                case PROXIMANENTEPAGE:

                    if (Equals(proximasSeries, default))
                    {
                        items = ProximasSeries;

                    }
                    break;
            }
            if (!Equals(items, default))
            {
                visor = (tbMain.SelectedItem as TabItem).Content as IVisor;
                visor.Visor.Elements = items;
                Title = CARGANDO;
                visor.Visor.Refresh().ContinueWith(AcabaDeCargar()).Wait();
            }

        }

        private void MostrarErrorSinInternet()
        {
            if (!ErrorMostrado)
                Dispatcher.BeginInvoke(new Action(() =>
                            {
                                ErrorMostrado = true;
                                if (MessageBox.Show("La conexión a internet esta inaccesible y eso imposibilita usar la aplicación con normalidad,¿Desea cerrarla?", "Atención", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                                    this.Close();
                            }));
        }

        private void AutoRefresh(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {

                Refresh();
                new Task(new Action(async () =>
                {
                    Notifications.Wpf.Core.NotificationContent notification;

                    SortedList<string, Capitulo> dicCapitulos = new SortedList<string, Capitulo>();

                    foreach (Capitulo capitulo in capitulosAnt)
                        dicCapitulos.Add(capitulo.Pagina.AbsoluteUri, capitulo);
                    foreach (Capitulo capitulo in CapitulosActuales)
                    {
                        if (!dicCapitulos.ContainsKey(capitulo.Pagina.AbsoluteUri))
                        {
                            capitulo.Serie.Reset();//así no da problemas con la fecha del proximo capitulo y el total :)
                            if (capitulo.Serie.IsFavorito)
                            {
                                //notifico sobre el capitulo
                                notification = new Notifications.Wpf.Core.NotificationContent();
                                notification.Message = $"Ha Salido {capitulo.Name}";
                                notification.Type = Notifications.Wpf.Core.NotificationType.Information;
                                notification.Title = "Capitulo nuevo!";

                                await Manager.ShowAsync(notification, onClick: () => capitulo.AbrirLink(), expirationTime: NotificationTimeOut);
                            }
                        }

                    }
                })).Start();
            }));
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
        private void visorSeries_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MoverVisor((tbMain.SelectedItem as TabItem).Content, e.Delta > 0 ? Key.Up : Key.Down);
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                Refresh();
            }
            else
            {
                MoverVisor(sender, e.Key);
            }

        }

        public void Refresh()
        {
            enEmision = default;
            proximasSeries = default;
            if (!Equals(capitulos, default))
            {
                capitulosAnt = capitulos;
                capitulos = default;
            }

            tbMain_SelectionChanged();
        }

        void MoverVisor(object sender, Key e)
        {

            IVisor visor = sender as IVisor;
            if (!Equals(visor, default))
            {
                if (e.Equals(Key.Up))
                {
                    if (visor.Visor.Page > 0)
                    {
                        visor.Visor.Page--;
                        Title = CARGANDO;
                        visor.Visor.Refresh().ContinueWith(AcabaDeCargar()).Wait();

                    }
                }
                else if (e.Equals(Key.Down))
                {
                    visor.Visor.Page++;
                    Title = CARGANDO;
                    visor.Visor.Refresh().ContinueWith(AcabaDeCargar()).Wait();
                }
            }

        }



    }
}
