using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.IO;
using System.Threading;
using Gabriel.Cat.S.Utilitats;
using System.Linq;

namespace CheckFenix.Core
{
    public static class HtmlAndLinksDic
    {
        public const string URLANIMEFENIX = "https://www.animefenix.com/";
        static LlistaOrdenada<string, string> DicDiaEmision { get; set; }//solo se guarda si hay link a mega
                                                                         //este no cae ese dia

        //diaDeLaSemana
        static LlistaOrdenada<DayOfWeek, LlistaOrdenada<string, string>> DicDiaNoEmision { get; set; } //tiene la fecha de cuando se emite el html no cambiará
        static LlistaOrdenada<string, KeyValuePair<DateTime, string>> DicPrimeraSemanaDeFinalizar { get; set; }//tiene la fecha fin para ponerlo en el definitivo
        static LlistaOrdenada<string, string> DicFinalizados { get; set; }//se guardan porque son inmutables
        static LlistaOrdenada<string, LlistaOrdenada<string>> DicCapitulos { get; set; }//los links pueden aparecer y desaparecer, tener opción de quitar para poder recargar
                                                                                        //si no va ninguno mirar de eliminarlos para poderlos recargar
        static LlistaOrdenada<string, LlistaOrdenada<string>> DicCapitulosCaidos { get; set; }//ya sea informado o automatizado, se guardan mientras salgan en la web del capitulo, así no hay problemas
        static HtmlAndLinksDic()
        {
            DicDiaEmision = new LlistaOrdenada<string, string>();
            DicDiaNoEmision = new LlistaOrdenada<DayOfWeek, LlistaOrdenada<string, string>>();
            DicPrimeraSemanaDeFinalizar = new LlistaOrdenada<string, KeyValuePair<DateTime, string>>();
            DicFinalizados = new LlistaOrdenada<string, string>();
            DicCapitulos = new LlistaOrdenada<string, LlistaOrdenada<string>>();
            DicCapitulosCaidos = new LlistaOrdenada<string, LlistaOrdenada<string>>();

            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
                DicDiaNoEmision.Add(dayOfWeek, new LlistaOrdenada<string, string>());

            //cargo lo guardado
           
        }
        public static string GetHtml(Uri urlPagina)
        {
            //miro si existe y si no pues lo pongo donde toque
            return urlPagina.DownloadString();
        }
        public static void AddHtml(Serie serie, string html = default(string))
        {
            const int TOTALDIASPARAFINALIZAR = 7 * 3;
            DayOfWeek dayOfWeek;
         
            if (Equals(html, default(string)))
            {
                html = GetHtml(serie.Pagina);
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
                    if (!Equals(serie.UltimoOrDefault,default(Capitulo)))
                    {

                        if (GetLinks(serie.UltimoOrDefault).Any(l => l.Contains("mega.nz")))
                        {

                            DicDiaEmision.AddOrReplace(serie.Pagina.AbsoluteUri, html);
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
                DicCapitulosCaidos.Add(capitulo.Pagina.AbsoluteUri, new LlistaOrdenada<string>());
            }
            links = capitulo.GetLinksFromHtml();
            DicCapitulos[capitulo.Pagina.AbsoluteUri].AddOrReplaceRange(links.Where(l=>!DicCapitulosCaidos[capitulo.Pagina.AbsoluteUri].ContainsKey(l)).ToList());

            return DicCapitulos[capitulo.Pagina.AbsoluteUri].Values.Where(l => !DicCapitulosCaidos[capitulo.Pagina.AbsoluteUri].ContainsKey(l));
        }
        public static void AddLinkCaido(Capitulo capitulo,string link)
        {
            if (!DicCapitulosCaidos.ContainsKey(capitulo.Pagina.AbsoluteUri))
            {
                DicCapitulos.Add(capitulo.Pagina.AbsoluteUri, new LlistaOrdenada<string>());
                DicCapitulosCaidos.Add(capitulo.Pagina.AbsoluteUri, new LlistaOrdenada<string>());
            }
            DicCapitulosCaidos[capitulo.Pagina.AbsoluteUri].AddOrReplace(link,link);
        }

    }
}
