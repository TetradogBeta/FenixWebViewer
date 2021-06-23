using CheckFenix.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;

namespace CheckFenix
{
    /// <summary>
    /// Lógica de interacción para VisorCapitulos.xaml
    /// </summary>
    public partial class VisorCapitulos : UserControl
    {
        public VisorCapitulos()
        {
            InitializeComponent();
            TotalPage = 9;
            Page = 0;
        }
        public IEnumerable<Capitulo> Capitulos { get; set; }
        public int TotalPage { get; set; }
        public int Page { get; set; }
        public void Refresh()
        {
            ugCapitulos.Children.Clear();
            foreach(Capitulo capitulo in Capitulos.Skip(Page*TotalPage).Take(TotalPage))
            {
                ugCapitulos.Children.Add(new CapituloViewer(capitulo));
            }
        }
    }
}
