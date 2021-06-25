using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.IO;
using System.Threading;
using Gabriel.Cat.S.Utilitats;
using System.Linq;
using System.Text;

namespace CheckFenix.Core
{
    public static class HtmlAndLinksDic
    {
        public const string URLANIMEFENIX = "https://www.animefenix.com/";

        public static string CacheFolder = "HtmlAndLinksCache";
        static LlistaOrdenada<string, string> DicDiaEmision { get; set; }//solo se guarda si hay link a mega
                                                                         //este no cae ese dia

        //diaDeLaSemana
        static LlistaOrdenada<DayOfWeek, LlistaOrdenada<string, string>> DicDiaNoEmision { get; set; } //tiene la fecha de cuando se emite el html no cambiará
        static LlistaOrdenada<string, KeyValuePair<DateTime, string>> DicPrimeraSemanaDeFinalizar { get; set; }//tiene la fecha fin para ponerlo en el definitivo
        static LlistaOrdenada<string, string> DicFinalizados { get; set; }//se guardan porque son inmutables
        static LlistaOrdenada<string, LlistaOrdenada<string>> DicCapitulos { get; set; }//los links pueden aparecer y desaparecer, tener opción de quitar para poder recargar
                                                                                        //si no va ninguno mirar de eliminarlos para poderlos recargar
        static LlistaOrdenada<string> DicCapitulosCaidos { get; set; }//ya sea informado o automatizado, se guardan mientras salgan en la web del capitulo, así no hay problemas

        static LlistaOrdenada<string, string> DicUrlsCargadas { get; set; }
        static HtmlAndLinksDic()
        {
            string path;
            string[] partes;
            TimeSpan unaSemana = TimeSpan.FromDays(7);
            DicDiaEmision = new LlistaOrdenada<string, string>();
            DicDiaNoEmision = new LlistaOrdenada<DayOfWeek, LlistaOrdenada<string, string>>();
            DicPrimeraSemanaDeFinalizar = new LlistaOrdenada<string, KeyValuePair<DateTime, string>>();
            DicFinalizados = new LlistaOrdenada<string, string>();
            DicCapitulos = new LlistaOrdenada<string, LlistaOrdenada<string>>();
            DicCapitulosCaidos = new LlistaOrdenada<string>();
            DicUrlsCargadas = new LlistaOrdenada<string, string>();

            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
                DicDiaNoEmision.Add(dayOfWeek, new LlistaOrdenada<string, string>());

            //cargo lo guardado


            path = Path.Combine(CacheFolder, nameof(DicDiaEmision));
            if (Directory.Exists(path))
            {
                //nombre del anime.html
                foreach(string file in Directory.GetFiles(path))
                {
                    if (File.GetLastWriteTime(file).Day == DateTime.Now.Day)
                    {
                        DicDiaEmision.Add(Path.Combine(URLANIMEFENIX, Path.GetFileNameWithoutExtension(file)), File.ReadAllText(file));
                    }
                    else
                    {
                        File.Delete(file);
                    }
                }
            }



            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
            {
                path = Path.Combine(CacheFolder, nameof(DicDiaNoEmision), dayOfWeek.ToString());
                if (Directory.Exists(path))
                {
                    if (DateTime.Now.DayOfWeek == dayOfWeek)
                    {
                        Directory.Delete(path);//borro los de ese dia
                    }
                    else
                    {
                        //diaSemana/nombre.html
                        foreach (string file in Directory.GetFiles(path))
                        {
                            if (File.GetLastWriteTime(file) - DateTime.Now < unaSemana)
                                DicDiaNoEmision[dayOfWeek].Add(Path.GetFileNameWithoutExtension(file), File.ReadAllText(file));
                            else File.Delete(file);//borro los caducados
                        }
                    }
                }


            }

            path = Path.Combine(CacheFolder, nameof(DicPrimeraSemanaDeFinalizar));

            if (Directory.Exists(path))
            {

                //ticksPara considerarlo finalizado->comprobar que tiene el mismo html o poner en finalizado el ultimo pero aparte para no molestar
                //ticks nombre.html
                foreach(string pathFile in Directory.GetFiles(path))
                {
                    partes =Path.GetFileNameWithoutExtension(pathFile).Split(" ");
                    if (DateTime.Now.Ticks < long.Parse(partes[0]))
                    {
                        //aun no se ha validado al 100%;
                        DicPrimeraSemanaDeFinalizar.Add(Path.Combine(URLANIMEFENIX, partes[1]), 
                                                        new KeyValuePair<DateTime, string>
                                                       (new DateTime(long.Parse(partes[0])), File.ReadAllText(pathFile)));
                    }
                    else
                    {
                        //poner en lista para comparar luego
                        //de momento los pongo así
                        DicFinalizados.Add(Path.Combine(URLANIMEFENIX, partes[1]), File.ReadAllText(pathFile));
                    }
                }

            }

            path = Path.Combine(CacheFolder, nameof(DicFinalizados));

            if (Directory.Exists(path))
            {
                //nombre.html
                foreach (string pathFile in Directory.GetFiles(path))
                {
                    DicFinalizados.Add(Path.Combine(URLANIMEFENIX, Path.GetFileNameWithoutExtension(pathFile)), File.ReadAllText(pathFile));
                }
            }

            path = Path.Combine(CacheFolder, nameof(DicCapitulosCaidos), ".txt");

            if (File.Exists(path))
            {
                DicCapitulosCaidos.AddOrReplaceRange(File.ReadAllLines(path));
            }


        }
        public static string GetHtmlServer(Uri urlPagina)
        {
            //html directo del servidor
            return urlPagina.DownloadString();
        }

        public static string GetHtml(Uri urlPagina)
        {
            //miro si existe y si no pues lo pongo donde toque
            string html;
            string url = urlPagina.AbsoluteUri;
            bool exist = DicDiaEmision.ContainsKey(url);
            if (!exist)
            {

                exist = DicPrimeraSemanaDeFinalizar.ContainsKey(url);
                if (!exist)
                {
                    exist = DicFinalizados.ContainsKey(url);
                    if (!exist)
                    {
                        if (!DicUrlsCargadas.ContainsKey(url))
                        {
                            html = urlPagina.DownloadString();
                            DicUrlsCargadas.Add(url, html);
                        }
                        else
                        {
                            html = DicUrlsCargadas[url];
                        }


                    }
                    else
                    {
                        html = DicFinalizados[url];
                    }

                }
                else
                {
                    html = DicPrimeraSemanaDeFinalizar[url].Value;
                }

            }
            else
            {
                html = DicDiaEmision[url];
            }
            return html;

        }
        public static string GetHtml(Serie serie)
        {//si no es necesario internet lo cojo del cache si esta
            string html;
            string url = serie.Pagina.AbsoluteUri;
            bool exist = DicDiaEmision.ContainsKey(url);
            if (!exist)
            {
                exist = DicDiaNoEmision[serie.NextChapter.Value.DayOfWeek].ContainsKey(url);
                if (!exist)
                {
                    exist = DicPrimeraSemanaDeFinalizar.ContainsKey(url);
                    if (!exist)
                    {
                        exist = DicFinalizados.ContainsKey(url);
                        if (!exist)
                        {
                            html = GetHtmlServer(serie.Pagina);
                            AddHtml(serie, html);

                        }
                        else
                        {
                            html = DicFinalizados[url];
                        }

                    }
                    else
                    {
                        html = DicPrimeraSemanaDeFinalizar[url].Value;
                    }
                }
                else
                {
                    html = DicDiaNoEmision[serie.NextChapter.Value.DayOfWeek][url];
                }
            }
            else
            {
                html = DicDiaEmision[url];
            }
            return html;

        }
        public static void AddHtml(Serie serie, string html = default(string))
        {
            const int TOTALDIASPARAFINALIZAR = 7 * 3;

            DayOfWeek dayOfWeek;

            if (Equals(html, default(string)))
            {
                if (!DicUrlsCargadas.ContainsKey(serie.Pagina.AbsoluteUri))
                {
                    html = GetHtmlServer(serie.Pagina);
                    DicUrlsCargadas.Add(serie.Pagina.AbsoluteUri, html);
                }
                else
                {
                    html = DicUrlsCargadas[serie.Pagina.AbsoluteUri];
                }
            }
            if (serie.Finalizada)
            {
                //miro si el ultimo esta en la parrilla
                if (serie.UltimoEnParrilla())
                {
                    if (!DicPrimeraSemanaDeFinalizar.ContainsKey(serie.Pagina.AbsoluteUri))
                        DicPrimeraSemanaDeFinalizar.Add(serie.Pagina.AbsoluteUri, new KeyValuePair<DateTime, string>(DateTime.Now + TimeSpan.FromDays(TOTALDIASPARAFINALIZAR), html));
                }
                else if (!DicPrimeraSemanaDeFinalizar.ContainsKey(serie.Pagina.AbsoluteUri) || DicPrimeraSemanaDeFinalizar[serie.Pagina.AbsoluteUri].Key < DateTime.Now)
                {
                    if (DicPrimeraSemanaDeFinalizar.ContainsKey(serie.Pagina.AbsoluteUri))
                        DicPrimeraSemanaDeFinalizar.Remove(serie.Pagina.AbsoluteUri);
                    DicFinalizados.AddOrReplace(serie.Pagina.AbsoluteUri, html);
                }
            }
            else
            {
                dayOfWeek = serie.NextChapter.Value.ToUniversalTime().DayOfWeek;

                if (DateTime.UtcNow.DayOfWeek == dayOfWeek)
                {
                    //es el dia de la emision! solo guardar si tiene link a mega sino quiere decir que no está listo
                    if (!Equals(serie.UltimoOrDefault, default(Capitulo)))
                    {

                        if (GetLinks(serie.UltimoOrDefault).Any(l => l.Contains("mega.nz")))
                        {

                            DicDiaEmision.AddOrReplace(serie.Pagina.AbsoluteUri, html);
                            if (DicDiaNoEmision[dayOfWeek].ContainsKey(serie.Pagina.AbsoluteUri))
                                DicDiaNoEmision[dayOfWeek].Remove(serie.Pagina.AbsoluteUri);
                        }
                    }
                }
                else if (!DicDiaNoEmision[dayOfWeek].ContainsKey(serie.Pagina.AbsoluteUri))
                {
                    DicDiaNoEmision[dayOfWeek].Add(serie.Pagina.AbsoluteUri, html);
                }
            }
        }

        public static IEnumerable<string> GetLinks(Capitulo capitulo)
        {
            IEnumerable<string> links;

            if (!DicCapitulos.ContainsKey(capitulo.Pagina.AbsoluteUri))
            {
                DicCapitulos.Add(capitulo.Pagina.AbsoluteUri, new LlistaOrdenada<string>());

            }
            links = capitulo.GetLinksFromHtml();
            DicCapitulosCaidos.AddOrReplaceRange(links.Where(l =>
            {
                bool? isOk;
                bool resp = !DicCapitulosCaidos.ContainsKey(l);
                if (resp)
                {
                    try
                    {
                        isOk = new Uri(l).IsOk();
                        resp = isOk.HasValue && !isOk.Value;//los que pueda añadir automaticamente lo hago aqui :)
                    }
                    catch
                    {
                        resp = true;
                    }
                }
                return resp;
            }).ToList());
            DicCapitulos[capitulo.Pagina.AbsoluteUri].AddOrReplaceRange(links.Where(l => !DicCapitulosCaidos.ContainsKey(l)).ToList());

            return DicCapitulos[capitulo.Pagina.AbsoluteUri].Values.Where(l => !DicCapitulosCaidos.ContainsKey(l));
        }
        public static void AddLinkCaido(string link)
        {
            DicCapitulosCaidos.AddOrReplace(link, link);
        }
        public static void SaveCache()
        {
            KeyValuePair<string, string> pair;
            KeyValuePair<string, KeyValuePair<DateTime, string>> pairPrimera;
            string pathFinalizado;
            string path = Path.Combine(CacheFolder, nameof(DicDiaEmision));

            if (DicDiaEmision.Count > 0)
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                for (int i = 0; i < DicDiaEmision.Count; i++)
                {
                    pair = DicDiaEmision[i];
                    File.WriteAllText(Path.Combine(path, $"{pair.Key.Remove(pair.Key.LastIndexOf('/'))}.html"), pair.Value);
                }
            }
            else
            {
                if (Directory.Exists(path))
                    Directory.Delete(path);
            }

            if (DicDiaNoEmision.Count > 0)
            {
                foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
                {
                    path = Path.Combine(CacheFolder, nameof(DicDiaNoEmision), dayOfWeek.ToString());
                    if (!Directory.Exists(path) && DicDiaNoEmision[dayOfWeek].Count > 0)
                        Directory.CreateDirectory(path);
                    for (int i = 0; i < DicDiaNoEmision[dayOfWeek].Count; i++)
                    {
                        pair = DicDiaNoEmision[dayOfWeek][i];
                        File.WriteAllText(Path.Combine(path, $"{pair.Key.Remove(pair.Key.LastIndexOf('/'))}.html"), pair.Value);
                    }
                }
            }
            else
            {
                path = Path.Combine(CacheFolder, nameof(DicDiaNoEmision));
                if (Directory.Exists(path))
                    Directory.Delete(path);
            }
            path = Path.Combine(CacheFolder, nameof(DicPrimeraSemanaDeFinalizar));

            if (DicPrimeraSemanaDeFinalizar.Count > 0)
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                for (int i = 0; i < DicPrimeraSemanaDeFinalizar.Count; i++)
                {
                    pairPrimera = DicPrimeraSemanaDeFinalizar[i];
                    File.WriteAllText(Path.Combine(path, $"{pairPrimera.Value.Key.Ticks} {pairPrimera.Key.Remove(pairPrimera.Key.LastIndexOf('/'))}.html"), pairPrimera.Value.Value);
                }
            }
            else
            {
                if (Directory.Exists(path))
                    Directory.Delete(path);
            }
            path = Path.Combine(CacheFolder, nameof(DicFinalizados));

            if (DicFinalizados.Count > 0)
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                for (int i = 0; i < DicFinalizados.Count; i++)
                {
                    pair = DicFinalizados[i];
                    pathFinalizado = Path.Combine(path, $"{pair.Key.Substring(pair.Key.LastIndexOf('/') + 1)}.html");
                    if(!File.Exists(pathFinalizado))
                      File.WriteAllText(pathFinalizado, pair.Value);
                }
            }
            else
            {
                if (Directory.Exists(path))
                    Directory.Delete(path);
            }
            path = Path.Combine(CacheFolder, nameof(DicCapitulosCaidos), ".txt");

            if (DicCapitulosCaidos.Count > 0)
            {
                File.WriteAllLines(path, DicCapitulosCaidos.Values);
            }
            else
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

    }
}
