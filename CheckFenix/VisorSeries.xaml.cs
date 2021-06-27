using CheckFenix.Core;
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
    /// Lógica de interacción para VisorSeries.xaml
    /// </summary>
    public partial class VisorSeries : UserControl
    {
        private int page;

        public VisorSeries()
        {
            InitializeComponent();
            TotalPage = 12;
            Page = 0;
            TotalRows = 2;
            MostrarFavorito = true;
            UltimaPaginaSinAcabar = -1;
        }

        public IEnumerable<Serie> Series { get; set; }

        public int TotalRows { get; set; }
        public int TotalPage { get; set; }
        public int Page { get => page; set { page = UltimaPaginaSinAcabar < 0?value: value <= UltimaPaginaSinAcabar?value:UltimaPaginaSinAcabar; } }
        public bool MostrarFavorito { get; set; }
        int UltimaPaginaSinAcabar { get; set; }
        public async Task Refresh()
        {
            SerieViewer visorSerie;
            IEnumerable<Serie> series;

            if (UltimaPaginaSinAcabar < 0 || Page <= UltimaPaginaSinAcabar)
            {
                series = Series.Skip(Page * TotalPage).Take(TotalPage);
                ugSeries.Rows = TotalRows;
                ugSeries.Children.Clear();
                foreach (Serie serie in series)
                {
                    visorSerie = new SerieViewer(serie) { MostarFavorito = MostrarFavorito };
                    visorSerie.Refresh();
                    ugSeries.Children.Add(visorSerie);
                }
                if (ugSeries.Children.Count == 0)
                {
                    UltimaPaginaSinAcabar = Page - 1;
                    Page--;
                    await Refresh();
                }
                else if (ugSeries.Children.Count < TotalPage)
                    UltimaPaginaSinAcabar = Page;
            }

        }



    }
}
