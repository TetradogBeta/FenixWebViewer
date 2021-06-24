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
        public VisorSeries()
        {
            InitializeComponent();
            TotalPage = 12;
            Page = 0;
        }
        public VisorSeries(IEnumerable<Serie> series)
        {
            Series = series;
            Refresh();
        }
        public IEnumerable<Serie> Series { get; set; }
        public int TotalPage { get; set; }
        public int Page { get; set; }
        public async Task Refresh()
        {
            IEnumerable<Serie> series = Series.Skip(Page * TotalPage).Take(TotalPage);

            ugSeries.Children.Clear();
            foreach (Serie serie in series)
            {
                ugSeries.Children.Add(new SerieViewer(serie));
            }


        }



    }
}
