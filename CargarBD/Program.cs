using CheckFenix.CargarBD;
using CheckFenix.Core;
using Gabriel.Cat.S.Extension;
using System;

using System.Linq;

namespace CargarBD
{
    public class Program
    {
        const string URL = "https://www.animefenix.com/";
        public static void Main(params string[] args)
        {
            int lastIndex=-1;
            string url = args.Length > 0 ? args[0] : URL;
            Context context = new Context();
            bool encontrado = false;
            Update last = context.Updates.ToList().LastOrDefault();
            
          
            if (Equals(last, default))
            {
                lastIndex = Serie.GetSeriesFinalizadasLastIndex(url);
                for (int i = lastIndex; i >= 1 && !encontrado; i--)
                {
                    if (Equals(context.Series.Find(Serie.GetSeriesFinalizadas(url, i, false).First().Pagina.AbsoluteUri), default))
                    {
                        encontrado = true;
                        lastIndex = i;
                    }
                }
           

                for (int i = lastIndex; i >= 0; i--)
                {
                    foreach (Serie serie in Serie.GetSeriesFinalizadas(url, i, false, 1))
                    {
                        context.Series.Add(new SerieBD(serie));
                    }
                    context.Updates.Add(new Update() { LastPage = i });
                    context.SaveChanges();

                }

            }
            else
            {
                for (int i = last.LastPage == 0 ? 1 : last.LastPage; !encontrado; i++)
                {
                    if (!Equals(context.Series.Find(Serie.GetSeriesFinalizadas(url, i).First().Pagina.AbsoluteUri), default))
                    {
                        encontrado = true;
                        lastIndex = i;
                    }
                }
                if (encontrado)
                {
                    for (int i = last.LastPage==0?1:last.LastPage; i <= lastIndex; i++)
                    {
                        Serie.GetSeriesFinalizadas(url, i, true, 1).WhileEach((serie) =>
                    {
                        encontrado = !Equals(context.Series.Find(serie.Pagina.AbsoluteUri), default);
                        if (!encontrado)
                        {
                            context.Series.Add(new SerieBD(serie));
                        }
                        return !encontrado;
                    });
                    }
                    context.Updates.Add(new Update() { LastPage = 1});
                    context.SaveChanges();
                }
            }

        }
    }
}
