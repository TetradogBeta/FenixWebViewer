using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using CheckFenix.Core;
using System.Linq;

namespace CheckFenix.TelegramBot
{
    class Program
    {
        public const int TIEMPOCHECK = 5 * 60 * 1000;
        public static TelegramBotClient BotClient { get; set; }
        public static SortedList<string, string> DicCapitulosPublished { get; set; }
        public static string File => Resource.File;
        public static string Web => Resource.Web;
        public static string Canal => Resource.Canal;
        public static string ApiKeyBoy => Resource.ApiKeyBot;

        static void Main(string[] args)
        {
            Timer temporizadorCheck = new Timer(MirarCapitulosNuevos);
            DicCapitulosPublished = new SortedList<string, string>();
            if (System.IO.File.Exists(File))
            {
                foreach (string anime in System.IO.File.ReadAllLines(File))
                    DicCapitulosPublished.Add(anime, anime);
            }

            Console.WriteLine("Iniciando Bot Telegram!");
            BotClient = new TelegramBotClient(ApiKeyBoy);
            temporizadorCheck.Change(0, TIEMPOCHECK);
            Console.WriteLine("Bot Esperando nuevos capitulos!");
            Console.WriteLine("Enter para cerrarlo");

            Console.ReadLine();
            if (System.IO.File.Exists(File))
            {
                System.IO.File.Delete(File);
            }
            System.IO.File.WriteAllLines(File, DicCapitulosPublished.Values);

        }

        private static void MirarCapitulosNuevos(object state)
        {
            string linkMega;
            foreach (Capitulo capitulo in Capitulo.GetCapitulosHome(Web).Reverse())
            {
                if (!DicCapitulosPublished.ContainsKey(capitulo.Name))
                {
                    linkMega = capitulo.Links.Where(l => l.Contains("mega")).FirstOrDefault();
                    if (!Equals(linkMega, default))
                    {
                        BotClient.SendPhotoAsync(Canal, new Telegram.Bot.Types.InputFiles.InputOnlineFile(capitulo.Picture), $"{capitulo.Name} {linkMega}");
                        DicCapitulosPublished.Add(capitulo.Name, capitulo.Name);
                        Console.WriteLine(capitulo.Name);
                    }
                }
            }
        }
    }
}
