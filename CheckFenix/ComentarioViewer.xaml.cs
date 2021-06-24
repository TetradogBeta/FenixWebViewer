using CheckFenix.Core;
using Gabriel.Cat.S.Extension;
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

namespace CheckFenix
{
    /// <summary>
    /// Lógica de interacción para ComentarioViewer.xaml
    /// </summary>
    public partial class ComentarioViewer : UserControl
    {
        public ComentarioViewer()
        {
            InitializeComponent();
        }
        public ComentarioViewer(Comentario comentario):this()
        {
            Comentario = comentario;
            Refresh();
        }
        public Comentario Comentario { get; set; }
        public void Refresh()
        {
            imgAvatar.SetImage(Comentario.Image);
            tbNombre.Text = Comentario.Name;
            tbComentario.Text = Comentario.Mensaje;
        }
    }
}
