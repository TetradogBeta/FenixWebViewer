using CheckFenix.Core;
using Gabriel.Cat.S.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace CheckFenix
{
    /// <summary>
    /// Lógica de interacción para CapituloViewer.xaml
    /// </summary>
    public partial class CapituloViewer : UserControl
    {
        public CapituloViewer()
        {
            InitializeComponent();
        }
        public CapituloViewer(Capitulo capitulo):this()
        {
            Capitulo = capitulo;
            Refresh();
        }
        public Capitulo Capitulo { get; set; }
        public void Refresh()
        {
            imgCapitulo.Source = new BitmapImage(Capitulo.Picture);
            imgCapitulo.ToolTip = Capitulo.Name;
            
        }

        private void imgCapitulo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string urlMega = Capitulo.GetLinks().Where(l => l.Contains("mega.nz")).FirstOrDefault();
            if (!string.IsNullOrEmpty(urlMega))
            {
                new Uri(urlMega).Abrir();
            }
        }

        private void imgCapitulo_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Capitulo.Parent.Pagina.Abrir();
        }
    }
}
