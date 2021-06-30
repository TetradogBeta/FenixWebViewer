using CheckFenix.Core;
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
    /// Lógica de interacción para VisorSeries.xaml
    /// </summary>
    public partial class VisorSeries : UserControl,IVisor
    {

        public VisorSeries()
        {
            InitializeComponent();
            visorSeries.Reader = (elements)=> (elements as IEnumerable<Serie>).Select(s => new SerieViewer(s) { MostarFavorito = MostrarFavorito }); 
            MostrarFavorito = true;
 
        }
        public Visor Visor => visorSeries;
        public IEnumerable<Serie> Series { get => visorSeries.Elements as IEnumerable<Serie>; set => visorSeries.Elements = value; }
  
        public bool MostrarFavorito { get; set; }

      
    }

}
