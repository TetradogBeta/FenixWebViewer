﻿using Gabriel.Cat.S.Extension;
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

namespace CheckFenix.Core
{
    public class Serie:IComparable,IComparable<Serie>,IEqualityComparer<Serie>
    {
        public static string CacheFolder = "CacheSeries";
        public static string FavoriteFile = "Favoritos.txt";
        static LlistaOrdenada<string, Bitmap> DicImagenes { get; set; } 
        static LlistaOrdenada<string,bool> DicFavoritos { get; set; }
        SortedList<int, Capitulo> capiulos;

        static Serie()
        {
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
                Bitmap bmp;
                string url = Path.GetFileName(Picture.AbsoluteUri);
         
                if (!DicImagenes.ContainsKey(url))
                    DicImagenes.Add(url, Picture.GetBitmap().Escala(0.5f));
                bmp= DicImagenes[url];
      
                return bmp;
            }
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
                    capiulos.Add(capitulo, Capitulo.FromUrl(new Uri($"{Pagina.AbsoluteUri.Replace(HtmlAndLinksDic.URLANIMEFENIX, HtmlAndLinksDic.URLANIMEFENIX + "ver/")}-{capitulo}")));
                }

                return capiulos[capitulo];
            }
        }
        public DateTime? NextChapter => !Finalizada ? DateTime.Parse(NextCapterDate) : default(DateTime);
        public bool Finalizada => string.IsNullOrEmpty(NextCapterDate);
        public Capitulo UltimoOrDefault =>Total>0? this[Total - 1]:default(Capitulo);

        public IEnumerable<Capitulo> GetCapitulos()
        {
            for (int i = 1; i <= Total; i++)
                yield return this[i];
        }
        public IEnumerable<Comentario> GetComentarios(IReadComentario reader)
        {
         
            return Comentario.GetComentarios(reader,Pagina);
        }
        public override bool Equals(object obj)
        {
            Serie serie = obj as Serie;
            return !Equals(serie, default(Serie)) && Name.Equals(serie.Name);
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
            HtmlDocument pagina =new HtmlDocument().LoadString(HtmlAndLinksDic.GetHtmlServer(Pagina));
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
            HtmlDocument pagina =new HtmlDocument().LoadString( HtmlAndLinksDic.GetHtml(this));
            HtmlNode nodoFecha= pagina.GetByTagName("span").Where(m => m.InnerText.Contains("Próximo")).FirstOrDefault();
            HtmlNode nodoTotal = pagina.GetByTagName("span").Where(m => m.InnerText.Contains("Episodios:")).FirstOrDefault();

            if (!Equals(nodoFecha, default(HtmlNode)))
            {
                NextCapterDate = nodoFecha.NextSibling.OuterHtml;
            }
            else NextCapterDate = string.Empty;

            Total = int.Parse(nodoTotal.NextSibling.OuterHtml);
        }

        public bool UltimoEnParrilla()
        {
            Capitulo ultimo = UltimoOrDefault;
            bool estaEnParrilla = !Equals(ultimo, default);
            if (estaEnParrilla)
            {
               estaEnParrilla= HtmlAndLinksDic.GetHtmlServer(ultimo.Pagina).Contains(ultimo.Pagina.AbsoluteUri);
            }
            return estaEnParrilla;
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

            const string BASEURL = HtmlAndLinksDic.URLANIMEFENIX + "animes?page=";
            const string CLASE = "serie-card";

            HtmlNode[] nodosSeries;
            int paginaActual = 1;
            do
            {
                nodosSeries =new HtmlDocument().LoadString(HtmlAndLinksDic.GetHtml(new Uri(BASEURL + paginaActual)))
                                               .GetByClass(CLASE).ToArray();

                for (int i = 0; i < nodosSeries.Length; i++)
                {
                    yield return new Serie(nodosSeries[i]);
                }
                paginaActual++;


            } while (nodosSeries.Length > 0);

        }
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
            else if (DicImagenes.Count== 0 && Directory.Exists(CacheFolder))
                Directory.Delete(CacheFolder);

            foreach (var item in DicImagenes)
            {
                try
                {
                    path = Path.Combine(CacheFolder, item.Key);
                    if(!File.Exists(path))
                        item.Value.Save(path);
                }
                catch {
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
