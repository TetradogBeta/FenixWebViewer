using CheckFenix.Core;
using System.ComponentModel.DataAnnotations;

namespace CheckFenix.CargarBD
{
    public class SerieBD
    {
        public SerieBD()
        {

        }
        public SerieBD(Serie serie)
        {
            Pagina = serie.Pagina.AbsoluteUri;
            Picture = serie.Picture.AbsoluteUri;
            Name = serie.Name;

        }
        [Key]
        public string Pagina { get; set; }
        public string Picture { get; set; }
        public string Name { get; set; }
    }
}