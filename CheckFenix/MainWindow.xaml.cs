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
        const string TITULO = "AnimeFenix Desktop 1.5";
        const string FINALIZADAS = "Finalizadas";

        IEnumerable<Capitulo> capitulosAnt;
        IEnumerable<Capitulo> capitulos;
        IEnumerable<Serie> enEmision;
        IEnumerable<Serie> proximasSeries;
        TimeSpan? NotificationTimeOut { get; set; }
        Timer TimerAutoRefresh { get; set; }
        public MainWindow()
        {
            NotificationTimeOut = TimeSpan.FromMinutes(3);
            TimerAutoRefresh = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
            Title = TITULO;
            InitializeComponent();
            TimerAutoRefresh.Elapsed += AutoRefresh;
            TimerAutoRefresh.Start();
            InitUpdate = new Task(new Action(() =>
            {
                Context context;

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
                    visorSeriesFinalizadas.Refresh().ContinueWith(AcabaDeCargar()).Wait();
                    tbFinalizadas.Header = FINALIZADAS;
                    tbFinalizadas.IsEnabled = true;
          
                }));

            }));
            InitUpdate.Start();
        }



        Task InitUpdate { get; set; }

        public IEnumerable<Capitulo> CapitulosActuales
        {
            get
            {
                if(Equals(capitulos,default))
                     capitulos= Capitulo.GetCapitulosHome(URLANIMEFENIX);
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

            switch (tbMain.SelectedIndex)
            {
                case CAPITULOSACTUALESPAGE:
                    if (Equals(capitulos, default))
                    {
                        visorCapitulosActuales.Capitulos = CapitulosActuales;
                        Title = CARGANDO;
                        visorCapitulosActuales.Refresh().ContinueWith(AcabaDeCargar()).Wait();
                    }
                    break;
                case SERIESACTUALESPAGE:
                    if (Equals(enEmision, default))
                    {
                        visorSeriesEnEmision.Series = SeriesEnEmision;
                        Title = CARGANDO;
                        visorSeriesEnEmision.Refresh().ContinueWith(AcabaDeCargar()).Wait();
                    }

                    break;
                case FAVORITOSPAGE:

                    visorSeriesFavoritas.Series = Favorites;
                    Title = CARGANDO;
                    visorSeriesFavoritas.Refresh().ContinueWith(AcabaDeCargar()).Wait();
                    break;
                case PROXIMANENTEPAGE:

                    if (Equals(proximasSeries, default))
                    {
                        visorSeriesParaSalir.Series = ProximasSeries;
                        Title = CARGANDO;
                        visorSeriesParaSalir.Refresh().ContinueWith(AcabaDeCargar()).Wait();
                    }
                    break;
            }

        }
        private void AutoRefresh(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(()=> {
                Notifications.Wpf.Core.NotificationContent notification;
                Notifications.Wpf.Core.NotificationManager manager = new Notifications.Wpf.Core.NotificationManager();
                SortedList<string, Capitulo> dicCapitulos = new SortedList<string, Capitulo>();
                Refresh();
                foreach (Capitulo capitulo in capitulosAnt)
                    dicCapitulos.Add(capitulo.Pagina.AbsoluteUri, capitulo);
                foreach (Capitulo capitulo in CapitulosActuales)
                    if(!dicCapitulos.ContainsKey(capitulo.Pagina.AbsoluteUri) && capitulo.Serie.IsFavorito)
                    {
                        //notifico sobre el capitulo
                        notification = new Notifications.Wpf.Core.NotificationContent();
                        notification.Message = $"Ha Salido {capitulo.Name}";
                        notification.Type = Notifications.Wpf.Core.NotificationType.Information;
                        notification.Title = "Capitulo nuevo!";
                      
                        manager.ShowAsync(notification,onClick:()=>capitulo.AbrirLink(),expirationTime:NotificationTimeOut);
                    }
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
        private void visorSeriesEnEmision_MouseWheel(object sender, MouseWheelEventArgs e)
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
