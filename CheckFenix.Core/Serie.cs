using Gabriel.Cat.S.Extension;
using Gabriel.Cat.S.Utilitats;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CheckFenix.Core
{
    public class Serie
    {
        public static string CacheFolder = "CacheSeries";
        public static string FavoriteFile = "Favoritos.txt";
        private static LlistaOrdenada<string, bool> DicFavoritos { get; set; }
        private static LlistaOrdenada<string, Bitmap> DicImagenes { get; set; }
        public static LlistaOrdenada<string, Serie> DicSeriesCompleto { get; set; }
        public static LlistaOrdenada<string, Serie> DicSeriesBasico { get; set; }
        public static IEnumerable<Serie> GetFavoritos() => DicFavoritos.Keys.Select(linkSerie => new Serie(new Uri(linkSerie)));


        private Serie precuela;
        private Serie secuela;
        private Uri paginaPrecuela;
        private Uri paginaSecuela;
        private int total;
        private DateTime? nextCapter;
        private string description;
        private string name;
        private Uri picture;
        private Capitulo ultimo;

        static Serie()
        {
            DicSeriesBasico = new LlistaOrdenada<string, Serie>();
            DicSeriesCompleto = new LlistaOrdenada<string, Serie>();
            DicImagenes = new LlistaOrdenada<string, Bitmap>();
            DicFavoritos = new LlistaOrdenada<string, bool>();
            if (Directory.Exists(CacheFolder))
            {
                //cargo el cache!
                foreach (string item in Directory.GetFiles(CacheFolder))
                    DicImagenes.Add(Path.GetFileName(item), new Bitmap(item));

            }

            if (File.Exists(FavoriteFile))
            {
                foreach (string url in File.ReadAllLines(FavoriteFile))
                    DicFavoritos.Add(url, true);
            }

        }

        public Serie(Uri pagina)
        {
            total = -1;
            Pagina = pagina;
        }
        LlistaOrdenada<int, Capitulo> Capitulos { get; set; }
        public Capitulo this[int capitulo]
        {
            get
            {
                if (Equals(Capitulos, default))
                {
                    Capitulos = new LlistaOrdenada<int, Capitulo>();
                }

                if (capitulo > Total)
                {
                    throw new CapituloNoEncontradoException(Finalizada);
                }

                if (!Capitulos.ContainsKey(capitulo))
                {
                    //lo cargo
                    Capitulos.AddOrReplace(capitulo, new Capitulo(new Uri($"{Pagina.AbsoluteUri.Replace(Pagina.Host, $"{Pagina.Host}/ver")}-{capitulo}")));
                }

                return Capitulos.GetValue(capitulo);
            }
        }

        public string Name
        {
            get
            {
                CargarDatosBasicosOCompletosSiEsNecesario();
                return name;
            }
            set => name = value;
        }
        public string Description
        {
            get
            {
                CargarDatosBasicosOCompletosSiEsNecesario();
                return description;
            }
            set => description = value;
        }



        public DateTime? NextChapter
        {
            get
            {
                CargarDatosSiEsNecesario();
                return nextCapter;
            }
            set => nextCapter = value;
        }

        public int Total
        {
            get
            {
                CargarDatosSiEsNecesario();
                return total;
            }
            set => total = value;
        }
        public Uri Pagina { get; set; }
        public Uri Picture
        {
            get
            {
                if (Equals(picture, default))
                    CargarDatosSiEsNecesario();

                return picture;
            }
            set => picture = value;
        }


        public Uri PaginaPrecuela
        {
            get
            {
                CargarDatosSiEsNecesario();
                return paginaPrecuela;
            }
            set => paginaPrecuela = value;
        }
        public Uri PaginaSecuela
        {
            get
            {
                CargarDatosSiEsNecesario();
                return paginaSecuela;
            }
            set => paginaSecuela = value;
        }
        public bool HasPrecuela => !Equals(PaginaPrecuela, default(Serie));
        public bool HasSecuela => !Equals(PaginaSecuela, default(Serie));

        public bool Finalizada => !NextChapter.HasValue || NextChapter.Value < DateTime.Now;
        public Uri PaginaUltimo => new Uri($"{Pagina.AbsoluteUri}-{Total}");
        public Capitulo Ultimo
        {
            get
            {
                if (Equals(ultimo, default) || ultimo.Numero != Total)
                {
                    ultimo = new Capitulo(PaginaUltimo) { Name = Name, PaginaSerie = Pagina };
                }
                return ultimo;
            }
        }
        public Serie Precuela
        {
            get
            {
                if (HasPrecuela && Equals(precuela, default))
                {
                    precuela = new Serie(PaginaPrecuela);
                }
                return precuela;

            }
            set => precuela = value;
        }
        public Serie Secuela
        {
            get
            {
                if (HasSecuela && Equals(secuela, default))
                {
                    secuela = new Serie(PaginaSecuela);
                }
                return secuela;

            }
            set => secuela = value;
        }
        public bool IsFavorito
        {
            get => DicFavoritos.ContainsKey(Pagina.AbsoluteUri);
            set
            {
                if (value)
                {
                    if (!DicFavoritos.ContainsKey(Pagina.AbsoluteUri))
                        DicFavoritos.Add(Pagina.AbsoluteUri, true);
                }
                else
                {
                    if (DicFavoritos.ContainsKey(Pagina.AbsoluteUri))
                        DicFavoritos.Remove(Pagina.AbsoluteUri);
                }
            }
        }
        public async Task<Bitmap> GetImage()
        {

            Bitmap bmp;
            string url = Path.GetFileName(Picture.AbsoluteUri);

            if (!DicImagenes.ContainsKey(url))
                DicImagenes.Add(url, (await Picture.GetBitmapAsnyc()).Escala(0.5f));
            bmp = DicImagenes[url];

            return bmp;

        }
        public void Refresh()
        {
            //elimino la pagina
            total = -1;
            CargarDatosSiEsNecesario();
        }
        private void CargarDatosSiEsNecesario()
        {
            HtmlDocument pagina;
            HtmlNode nodoNombre;
            HtmlNode nodoDesc;
            HtmlNode nodoPicture;
            HtmlNode nodoPrecuela;
            HtmlNode nodoSecuela;
            HtmlNode nodoFecha;
            HtmlNode nodoTotal;
            Serie serie;

            if (total < 0)
            {
                if (!DicSeriesCompleto.ContainsKey(Pagina.AbsoluteUri))
                {
                    DicSeriesCompleto.Add(Pagina.AbsoluteUri, this);
                    DicSeriesBasico.AddOrReplace(Pagina.AbsoluteUri, this);

                    pagina = new HtmlDocument().LoadUrl(Pagina);
                    nodoNombre = pagina.GetByTagName("meta").Where(m => !Equals(m.Attributes["name"], default(HtmlAttribute)) && m.Attributes["name"].Value.Equals("title")).FirstOrDefault();
                    nodoDesc = pagina.GetByTagName("meta").Where(m => !Equals(m.Attributes["name"], default(HtmlAttribute)) && m.Attributes["name"].Value.Equals("description")).FirstOrDefault();
                    nodoPicture = pagina.GetByClass("is-2by4").FirstOrDefault();
                    nodoPrecuela = pagina.GetByTagName("li").Where(l => l.ChildNodes.Any(c => c.Name == "span" && c.InnerText.Contains("Precuela:"))).FirstOrDefault();
                    nodoSecuela = pagina.GetByTagName("li").Where(l => l.ChildNodes.Any(c => c.Name == "span" && c.InnerText.Contains("Secuela:"))).FirstOrDefault();
                    nodoFecha = pagina.GetByTagName("span").Where(m => m.InnerText.Contains("Próximo")).FirstOrDefault();
                    nodoTotal = pagina.GetByTagName("span").Where(m => m.InnerText.Contains("Episodios:")).FirstOrDefault();

                    if (!Equals(nodoFecha, default(HtmlNode)))
                    {
                        NextChapter = DateTime.Parse(nodoFecha.NextSibling.OuterHtml);
                    }


                    Total = int.Parse(nodoTotal.NextSibling.OuterHtml);

                    Name = nodoNombre.Attributes["content"].Value;
                    Description = nodoDesc.Attributes["content"].Value.Replace("&quot;", "");
                    Picture = new Uri(nodoPicture.ChildNodes[1].Attributes["src"].Value);
                    //Precuela:
                    if (!Equals(nodoPrecuela, default(HtmlNode)))
                    {
                        PaginaPrecuela = new Uri(nodoPrecuela.GetByTagName("a").First().Attributes["href"].Value);
                    }
                    //Secuela:
                    if (!Equals(nodoSecuela, default(HtmlNode)))
                    {
                        PaginaSecuela = new Uri(nodoSecuela.GetByTagName("a").First().Attributes["href"].Value);
                    }
                }
                else
                {
                    serie = DicSeriesCompleto[Pagina.AbsoluteUri];
                    Name = serie.Name;
                    Total = serie.Total;
                    PaginaPrecuela = serie.PaginaPrecuela;
                    PaginaSecuela = serie.PaginaSecuela;
                    Picture = serie.Picture;
                    Description = serie.Description;

                }

            }
        }
        private void CargarDatosBasicosOCompletosSiEsNecesario()
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description))
            {
                if (DicSeriesBasico.ContainsKey(Pagina.AbsoluteUri))
                {
                    name = DicSeriesBasico[Pagina.AbsoluteUri].Name;
                    description = DicSeriesBasico[Pagina.AbsoluteUri].Description;
                }
                else
                {
                    CargarDatosSiEsNecesario();
                }
            }
        }
        public bool UltimoEnParrilla()
        {
            return new Uri(Pagina.Host).DownloadString().Contains(Ultimo.Pagina.AbsolutePath);
        }
        public override string ToString()
        {
            return Name;
        }

        public static IEnumerable<Serie> GetSeriesCuarentena(string urlFenix, int startPage = 1, bool upToDown = true, int maxPages = -1)
        {
            return IGetSeries(urlFenix, "estado%5B%5D=4&order=default", startPage, upToDown, maxPages);
        }
        public static IEnumerable<Serie> GetSeriesProximanente(string urlFenix, int startPage = 1, bool upToDown = true, int maxPages = -1)
        {
            return IGetSeries(urlFenix, "estado%5B%5D=3&order=default", startPage, upToDown, maxPages);
        }
        public static IEnumerable<Serie> GetSeriesEmision(string urlFenix, int startPage = 1, bool upToDown = true, int maxPages = -1)
        {
            return IGetSeries(urlFenix, "estado%5B%5D=1&order=default", startPage, upToDown, maxPages);
        }
        public static IEnumerable<Serie> GetSeriesFinalizadas(string urlFenix, int startPage = 1, bool upToDown = true, int maxPages = -1)
        {
            return IGetSeries(urlFenix, "estado%5B%5D=2&order=default", startPage, upToDown, maxPages);//estas no van a cambiar, así que se deberian de guardar!
        }
        static IEnumerable<Serie> IGetSeries(string urlFenix, string filtro, int startPage = 1, bool upToDown = true, int maxPages = -1)
        {

            const string CLASE = "serie-card";

            HtmlNode[] nodosSeries;
            HtmlNode nodoLink;
            string uri, name;

            int paginaActual = startPage;
            int totalPaginas = 0;
            string urlBase = $"{urlFenix}animes?{filtro}&page=";
            do
            {
                //no se puede guardar porque la pagina 1 es la más nueva ergo los indices cambian
                nodosSeries = new HtmlDocument().LoadUrl(new Uri(urlBase + paginaActual))
                                                .GetByClass(CLASE).ToArray();
                if (upToDown)
                {
                    for (int i = 0; i < nodosSeries.Length; i++)
                    {
                        nodoLink = nodosSeries[i].GetByTagName("a").First();
                        uri= nodoLink.Attributes["href"].Value;
                        name = nodoLink.Attributes["title"].Value;
                        yield return DicSeriesCompleto.ContainsKey(uri)?DicSeriesCompleto[uri]: DicSeriesBasico.ContainsKey(uri) ? DicSeriesBasico[uri]:GetSerie(nodosSeries[i],uri,name);
                    }
                }
                else
                {
                    for (int i = nodosSeries.Length - 1; i >= 0; i--)
                    {
                        nodoLink = nodosSeries[i].GetByTagName("a").First();
                        uri = nodoLink.Attributes["href"].Value;
                        name = nodoLink.Attributes["title"].Value;
                        yield return DicSeriesCompleto.ContainsKey(uri) ? DicSeriesCompleto[uri] : DicSeriesBasico.ContainsKey(uri) ? DicSeriesBasico[uri] : GetSerie(nodosSeries[i], uri, name);
                    }
                }
                paginaActual++;
                totalPaginas++;

            } while (nodosSeries.Length > 0 && (maxPages < 0 || totalPaginas < maxPages));

        }

        private static Serie GetSerie(HtmlNode nodoSerie,string uri,string nombre)
        {

            Serie serie;
            
            serie = new Serie(new Uri(uri));
            serie.Name = nombre;
            serie.Description = nodoSerie.GetByClass("serie-card__information").First().GetByTagName("p").First().InnerText;
            serie.Picture = new Uri(nodoSerie.GetByTagName("img").First().Attributes["src"].Value);
            DicSeriesBasico.AddOrReplace(uri, serie);
            return serie;
        }
        #region LastIndex
        public static int GetSeriesCuarentenaLastIndex(string urlFenix)
        {
            return IGetLastIndex(urlFenix, "estado%5B%5D=4&order=default");
        }
        public static int GetSeriesProximanenteLastIndex(string urlFenix)
        {
            return IGetLastIndex(urlFenix, "estado%5B%5D=3&order=default");
        }
        public static int GetSeriesEmisionLastIndex(string urlFenix)
        {
            return IGetLastIndex(urlFenix, "estado%5B%5D=1&order=default");
        }
        public static int GetSeriesFinalizadasLastIndex(string urlFenix)
        {
            return IGetLastIndex(urlFenix, "estado%5B%5D=2&order=default");
        }
        static int IGetLastIndex(string urlFenix, string filtro)
        {
            int index = 42;
            while (!Equals(IGetSeries(urlFenix, filtro, index).FirstOrDefault(), default)) index++;
            if (index > 42)
                index--;
            while (Equals(IGetSeries(urlFenix, filtro, index).FirstOrDefault(), default)) index--;
            return index;

        }
        #endregion
        public static void SaveFavoritos()
        {
            string pathBack = FavoriteFile + ".bak";

            if (File.Exists(pathBack))
                File.Delete(pathBack);

            if (File.Exists(FavoriteFile))
            {
                File.Move(FavoriteFile, pathBack);
            }
            File.WriteAllLines(FavoriteFile, DicFavoritos.Where(p => p.Value).Select(p => p.Key));
        }

        public static void SaveCache()
        {
            string path;

            if (DicImagenes.Count > 0 && !Directory.Exists(CacheFolder))
                Directory.CreateDirectory(CacheFolder);
            else if (DicImagenes.Count == 0 && Directory.Exists(CacheFolder))
                Directory.Delete(CacheFolder);

            foreach (var item in DicImagenes)
            {
                try
                {
                    path = Path.Combine(CacheFolder, item.Key);
                    if (!File.Exists(path))
                        item.Value.Save(path);
                }
                catch
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
        }

    }
    public class CapituloNoEncontradoException : Exception
    {
        public CapituloNoEncontradoException(bool noSaldra = false) => NoSaldra = noSaldra;
        public bool NoSaldra { get; private set; }

    }
}
