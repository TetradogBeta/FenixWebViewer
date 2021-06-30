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
using System.Threading.Tasks;
using System.Collections;

namespace CheckFenix
{
    /// <summary>
    /// Lógica de interacción para VisorCapitulos.xaml
    /// </summary>
    public partial class VisorCapitulos : UserControl,IVisor
    {
        public VisorCapitulos()
        {
            InitializeComponent();
            visorCapitulos.Reader = (elements)=> (elements as IEnumerable<Capitulo>).Select(c => new CapituloViewer(c));

        }
        public Visor Visor => visorCapitulos;
        public IEnumerable<Capitulo> Capitulos { get => visorCapitulos.Elements as IEnumerable<Capitulo>; set => visorCapitulos.Elements = value; }

       
    }
}
