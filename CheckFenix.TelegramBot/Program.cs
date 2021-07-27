using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using CheckFenix.Core;
using System.Linq;
using Gabriel.Cat.S.Check;

namespace CheckFenix.TelegramBot
{
    public class CapituloFenix : IFile
    {
        public CapituloFenix(Capitulo capitulo) => Capitulo = capitulo;
        public Capitulo Capitulo { get; set; }
        public string Name => Capitulo.Name;

        public Uri Picture => Capitulo.Picture;

        public IEnumerable<Link> GetLinks()
        {
            string linkMega = Capitulo.GetLinkMega();
            return Equals(linkMega, default) ? default : new Link[] { linkMega };
        }

    }
    class Program
    {
        public const string VERSION = "1.4";
        public const int TIEMPOCHECK = 5 * 60 * 1000;


        static void Main(string[] args)
        {
            Init(args).Wait();
        }
        static async Task Init(string[] args)
        {
            Check checkFenix = new Check("Config");
            await checkFenix.Load(args);
            await checkFenix.Publicar(async (webUrl) => Capitulo.GetCapitulosHome(webUrl.AbsoluteUri).Reverse().Select(c => new CapituloFenix(c)), TIEMPOCHECK);

        }


    }
}
