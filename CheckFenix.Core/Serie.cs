using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckFenix.Core
{
    public class Serie
    {
        SortedList<int, Capitulo> capiulos;
        public Serie()
        {
            capiulos = new SortedList<int, Capitulo>();
        }
        public Serie(HtmlNode nodoSerie) : this()
        {
            //pagina
            LoadNameAndDesc();
            Reload();
        }



        public Uri Pagina { get; set; }
        public Uri Picture { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string NextCapterDate { get; set; }
        public int Total { get; set; }
    
        public Capitulo this[int capitulo]
        {
            get
            {
                const string URLBASE = "https://www.animefenix.com/";
                
                if (Total < capitulo)
                {
                    throw new CapituloNoEncontradoException(Finalizada);
                }
                if (!capiulos.ContainsKey(capitulo))
                {
                    //lo cargo
                    capiulos.Add(capitulo, Capitulo.FromUrl(new Uri($"{Pagina.AbsoluteUri.Replace(URLBASE,URLBASE+"ver/")}-{capitulo}")));
                }
                return capiulos[capitulo];
            }
        }
        public bool Finalizada => string.IsNullOrEmpty(NextCapterDate);

        public IEnumerable<Capitulo> GetCapitulos()
        {
            for (int i = 0; i < Total; i++)
                yield return this[i];
        }
        private void LoadNameAndDesc()
        {
            //name
            //description
            /*
             <meta name="title" content="Nanatsu no Taizai: Fundo no Shinpan Online HD" />
             <meta name="description" content='ME CAGO EN LA CONCHA PELUDA DE LA MADRE DEL JAPONÉS QUE SE LE OCURRIÓ EMITIR ESTA MADRE A LAS 4AM EN LATINO AMÉRICA DOS AÑOS SEGUIDOS. 
                PD: Última temporada de NNT.' />
             */
            HtmlDocument pagina = new HtmlDocument().LoadUrl(Pagina);
            HtmlNode nodoNombre = pagina.GetByTagName("meta").Where(m =>!Equals(m.Attributes["name"],default(HtmlAttribute)) && m.Attributes["name"].Value.Equals("title")).FirstOrDefault();
            HtmlNode nodoDesc= pagina.GetByTagName("meta").Where(m => !Equals(m.Attributes["name"], default(HtmlAttribute)) && m.Attributes["name"].Value.Equals("description")).FirstOrDefault();
            HtmlNode nodoPicture = pagina.GetByClass("2by4").FirstOrDefault();
            Name = nodoNombre.Attributes["content"].Value;
            Description = nodoDesc.Attributes["content"].Value;
            Picture = new Uri(nodoPicture.FirstChild.Attributes["src"].Value);

        }
        public void Reload()
        {
            //actualiza el total,finalizada y el next
            HtmlDocument pagina = new HtmlDocument().LoadUrl(Pagina);
            HtmlNode nodoFecha= pagina.GetByTagName("span").Where(m => m.InnerText.Contains("Próximo")).FirstOrDefault();
            HtmlNode nodoTotal = pagina.GetByTagName("span").Where(m => m.InnerText.Contains("Episodios:")).FirstOrDefault();

            if (!Equals(nodoFecha, default(HtmlNode)))
            {
                NextCapterDate = nodoFecha.NextSibling.OuterHtml;
            }
            else NextCapterDate = string.Empty;

            Total = int.Parse(nodoTotal.NextSibling.OuterHtml);
        }
        public static Serie FromUrl(Uri urlSerie)
        {
            Serie serie = new Serie();
            serie.Pagina = urlSerie;
            serie.LoadNameAndDesc();
            serie.Reload();
            return serie;
        }

        public static IEnumerable<Serie> GetAllSeries()
        {
            //
            const string BASEURL = "https://www.animefenix.com/animes?page=";
            const string CLASE = "serie-card";

            HtmlNode[] nodosSeries;
            int paginaActual = 1;
            do
            {
                nodosSeries = new HtmlDocument().LoadUrl(BASEURL + paginaActual).GetByClass(CLASE).ToArray();
                for (int i = 0; i < nodosSeries.Length; i++)
                {
                    yield return new Serie(nodosSeries[i]);
                }
                paginaActual++;


            } while (nodosSeries.Length > 0);

        }
    }
    public class CapituloNoEncontradoException : Exception
    {
        public CapituloNoEncontradoException(bool noSaldra = false) => NoSaldra = noSaldra;
        public bool NoSaldra { get; private set; }

    }
}
