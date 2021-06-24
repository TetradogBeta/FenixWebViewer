using Gabriel.Cat.S.Extension;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CheckFenix.Core
{
    public class Serie:IComparable,IComparable<Serie>,IEqualityComparer<Serie>
    {
        public static string CacheFolder = "CacheSeries";
        static SortedList<string, Bitmap> DicImagenes { get; set; } = new SortedList<string, Bitmap>();
        SortedList<int, Capitulo> capiulos;
        public Serie()
        {
            capiulos = new SortedList<int, Capitulo>();
        }
        public Serie(HtmlNode nodoSerie) : this()
        {
            //pagina
            Pagina=new Uri(nodoSerie.GetByTagName("a").First().Attributes["href"].Value);
            LoadNameAndDesc();
            Reload();
        }



        public Uri Pagina { get; set; }
        public Uri Picture { get; set; }
        public Bitmap Image
        {
            get
            {
                if (!DicImagenes.ContainsKey(Picture.AbsoluteUri))
                    DicImagenes.Add(Picture.AbsoluteUri,Picture.GetBitmap().Escala(0.5f));
                return DicImagenes[Picture.AbsoluteUri];
            }
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public string NextCapterDate { get; set; }
        public int Total { get; set; }
    
        public Capitulo this[int capitulo]
        {
            get
            {

                
                if (Total < capitulo)
                {
                    throw new CapituloNoEncontradoException(Finalizada);
                }
                if (!capiulos.ContainsKey(capitulo))
                {
                    //lo cargo
                    capiulos.Add(capitulo, Capitulo.FromUrl(new Uri($"{Pagina.AbsoluteUri.Replace(HtmlDic.URLANIMEFENIX, HtmlDic.URLANIMEFENIX + "ver/")}-{capitulo}")));
                }
                return capiulos[capitulo];
            }
        }
        public bool Finalizada => string.IsNullOrEmpty(NextCapterDate);

        public IEnumerable<Capitulo> GetCapitulos()
        {
            for (int i = 1; i <= Total; i++)
                yield return this[i];
        }
        public override bool Equals(object obj)
        {
            Serie serie = obj as Serie;
            return !Equals(serie, default(Serie)) && Pagina.Equals(serie.Pagina);
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
            HtmlDocument pagina =HtmlDic.GetHtml(Pagina);
            HtmlNode nodoNombre = pagina.GetByTagName("meta").Where(m =>!Equals(m.Attributes["name"],default(HtmlAttribute)) && m.Attributes["name"].Value.Equals("title")).FirstOrDefault();
            HtmlNode nodoDesc= pagina.GetByTagName("meta").Where(m => !Equals(m.Attributes["name"], default(HtmlAttribute)) && m.Attributes["name"].Value.Equals("description")).FirstOrDefault();
            HtmlNode nodoPicture = pagina.GetByClass("is-2by4").FirstOrDefault();
            Name = nodoNombre.Attributes["content"].Value;
            Description = nodoDesc.Attributes["content"].Value.Replace("&quot;","");
            Picture = new Uri(nodoPicture.ChildNodes[1].Attributes["src"].Value);

        }
        public void Reload()
        {
            //actualiza el total,finalizada y el next
            HtmlDocument pagina = HtmlDic.GetHtml(Pagina);
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
        int IComparable.CompareTo(object obj)
        {
            return ICompareTo(obj as Serie);
        }

        int IComparable<Serie>.CompareTo(Serie other)
        {
            return ICompareTo(other);
        }

        private int ICompareTo(Serie other)
        {
            return Equals(other, default(Serie)) ? -1 : Pagina.AbsoluteUri.CompareTo(other.Pagina.AbsoluteUri);
        }
        public bool Equals([AllowNull] Serie x, [AllowNull] Serie y)
        {
            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] Serie obj)
        {
            return obj.GetHashCode();
        }

        public static IEnumerable<Serie> GetAllSeries()
        {
            //

            const string BASEURL = HtmlDic.URLANIMEFENIX + "animes?page=";
            const string CLASE = "serie-card";

            HtmlNode[] nodosSeries;
            int paginaActual = 1;
            do
            {
                nodosSeries = HtmlDic.GetHtml(new Uri(BASEURL + paginaActual)).GetByClass(CLASE).ToArray();
                for (int i = 0; i < nodosSeries.Length; i++)
                {
                    yield return new Serie(nodosSeries[i]);
                }
                paginaActual++;


            } while (nodosSeries.Length > 0);

        }
        public static void SaveCache()
        {
            Uri urlAnimeFenix = new Uri(HtmlDic.URLANIMEFENIX);

            foreach(var item in DicImagenes)
            {
                item.Value.Save(System.IO.Path.Combine(CacheFolder, new Uri(item.Key).MakeRelativeUri(urlAnimeFenix).ToString() + ".jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }


    }
    public class CapituloNoEncontradoException : Exception
    {
        public CapituloNoEncontradoException(bool noSaldra = false) => NoSaldra = noSaldra;
        public bool NoSaldra { get; private set; }

    }
}
