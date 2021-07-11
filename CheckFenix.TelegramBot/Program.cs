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
    public class CapituloFenix : IFileMega
    {
        public CapituloFenix(Capitulo capitulo) => Capitulo = capitulo;
        public Capitulo Capitulo { get; set; }
        public string Name => Capitulo.Name;

        public Uri Picture => Capitulo.Picture;

        public string[] GetLinksMega()
        {
            string linkMega = Capitulo.GetLinkMega();
            return Equals(linkMega, default) ? new string[0] : new string[] { linkMega };
        }

    }
    class Program
    {
        public const string VERSION = "1.3";
        public const int TIEMPOCHECK = 5 * 60 * 1000;


        static void Main(string[] args)
        {
            Check checkFenix = new Check("Config");
            checkFenix.Load(args);
            checkFenix.Publicar(() => Capitulo.GetCapitulosHome(checkFenix.Web.AbsoluteUri).Reverse().Select(c => new CapituloFenix(c)),TIEMPOCHECK);

        }

    }
}
