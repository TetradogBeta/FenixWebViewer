using CheckFenix.Core;
using Gabriel.Cat.S.Extension;
using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Lógica de interacción para CapituloViewer.xaml
    /// </summary>
    public partial class CapituloViewer : UserControl,IRefresh
    {
        public CapituloViewer()
        {
            InitializeComponent();
        }
        public CapituloViewer(Capitulo capitulo):this()
        {
            Capitulo = capitulo;
        }
        public Capitulo Capitulo { get; set; }
        public void Refresh()
        {
            imgCapitulo.SetImage( Capitulo.GetImage());
            imgCapitulo.ToolTip = Capitulo.Name;
            
        }

        private void imgCapitulo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(!Capitulo.AbrirLink())
            {
                MessageBox.Show("No hay un servidor para verlo...");
            }
        }

        private void imgCapitulo_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            new winSerie(Capitulo.Serie).Show();
        }
    }
}
