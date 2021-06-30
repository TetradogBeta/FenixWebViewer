using System;
using System.Collections;
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
    /// Lógica de interacción para Visor.xaml
    /// </summary>
    public partial class Visor : UserControl
    {
        private int page;

        public Visor()
        {
            InitializeComponent();
            TotalPage = 12;
            Page = 0; UltimaPaginaSinAcabar = -1;
            TotalRows = 2;
            TotalColumns = 3;
        }
        public  GetElements Reader { get; set; }
        public IEnumerable Elements { get; set; }
        public int TotalRows { get; set; }
        public int TotalColumns { get; set; }
        public int TotalPage { get; set; }
        public int Page { get => page; set { page = UltimaPaginaSinAcabar < 0 ? value : value <= UltimaPaginaSinAcabar ? value : UltimaPaginaSinAcabar; } }
        int UltimaPaginaSinAcabar { get; set; }

        public async Task Refresh()
        {
            IEnumerable<UIElement> series;
            List<Task> cargaSeries;
            if (UltimaPaginaSinAcabar < 0 || Page <= UltimaPaginaSinAcabar)
            {
                cargaSeries = new List<Task>();
                series = Reader(Elements).Skip(Page * TotalPage).Take(TotalPage);
                ugVisor.Rows = TotalRows;
                ugVisor.Columns = TotalColumns;
                ugVisor.Children.Clear();
                foreach (UIElement serie in series)
                {

                    cargaSeries.Add((serie as IRefresh).Refresh());
                    ugVisor.Children.Add(serie);
                }
                if (ugVisor.Children.Count == 0)
                {
                    UltimaPaginaSinAcabar = Page - 1;
                    Page--;
                    await Refresh();
                }
                else if (ugVisor.Children.Count < TotalPage)
                    UltimaPaginaSinAcabar = Page;
                if (ugVisor.Children.Count > 0)
                {
                    await Task.WhenAll(cargaSeries);
                }
            }
        }



    }
    public interface IVisor
    {
        Visor Visor { get; }
    }
    public delegate IEnumerable<UIElement> GetElements(IEnumerable elements);

    public interface IRefresh
    {
        Task Refresh();
    }
}
