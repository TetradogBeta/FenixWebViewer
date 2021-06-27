using CheckFenix.CargarBD;
using CheckFenix.Core;
using Gabriel.Cat.S.Extension;
using System;
using System.Collections.Generic;
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
            List<Update> updates = context.Updates.ToList();
            Update last =updates.LastOrDefault();
            
          
            if (Equals(last, default) || last.LastPage>1)
            {
                lastIndex = Equals(last, default) ? Serie.GetSeriesFinalizadasLastIndex(url):last.LastPage;
                for (int i = lastIndex; i >0 ; i--)
                {
                    
                    foreach (Serie serie in Serie.GetSeriesFinalizadas(url, i, false, 1))
                    {
                      if(Equals(context.Series.Find(serie.Pagina.AbsoluteUri),default))
                        context.Series.Add(new SerieBD(serie));
                    }
                    context.Updates.Add(new Update() { LastPage = i-1 });
                    context.SaveChanges();

                }
         

            }
            else
            {
                for (int i =1; !encontrado; i++)
                {
                    if (!Equals(context.Series.Find(Serie.GetSeriesFinalizadas(url, i).First().Pagina.AbsoluteUri), default))
                    {
                        encontrado = true;
                        lastIndex = i;
                    }
                }
                if (encontrado)
                {
                 
                        for (int i = last.LastPage == 0 ? 1 : last.LastPage; i <= lastIndex; i++)
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
                        context.Updates.Add(new Update() { LastPage = 1 });
                   
                    context.SaveChanges();
                }
            }

        }
    }
}
