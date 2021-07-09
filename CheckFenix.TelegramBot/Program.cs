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
        public const string VERSION = "1.2";
        public const int TIEMPOCHECK = 1 * 60 * 1000;
        public static TelegramBotClient BotClient { get; set; }
        public static SortedList<string, string> DicCapitulosPublished { get; set; }
        public static string FileConfig => "config";
        public static string File { get; set; }
        public static string Web { get; set; }
        public static string Canal { get; set;}
        public static string ApiKeyBot {get; set;}
        public static Semaphore Semaphore { get; set; }

        static void Main(string[] args)
        {
            const int TOTAL = 4;
            string[] lines;
            Timer temporizadorCheck = new Timer(MirarCapitulosNuevos);
            DicCapitulosPublished = new SortedList<string, string>();
            Semaphore = new Semaphore(1, 1);
            if (System.IO.File.Exists(FileConfig))
            {
                lines = System.IO.File.ReadAllLines(FileConfig);
                if (lines.Length != TOTAL)
                    throw new Exception("el archivo no contiene todos los elementos, File,Web,Canal,ApiKeyBot");
                File = lines[0];
                Web = lines[1];
                Canal = $"@{lines[2]}";
                ApiKeyBot = lines[3];

            }
            else if (args.Length == 4)
            {
                File = args[0];
                Web = args[1];
                Canal = $"@{args[2]}";
                ApiKeyBot = args[3];
                System.IO.File.WriteAllLines(FileConfig, args);
            }else if (args.Length > 0)
            {
                throw new Exception("se tienen que pasar todos los elementos: File,Web,Canal,ApiKeyBot");
            }
            else
            {
                throw new Exception("No se puede iniciar la aplicación!");
            }

            if (System.IO.File.Exists(File))
            {
                foreach (string anime in System.IO.File.ReadAllLines(File))
                    DicCapitulosPublished.Add(anime, anime);
            }

            Console.WriteLine($"Iniciando Bot V{VERSION} en Telegram!");
            BotClient = new TelegramBotClient(ApiKeyBot);
            
            temporizadorCheck.Change(0, TIEMPOCHECK);
            Console.WriteLine("Esperando nuevos capitulos!");

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
            try
            {
                Semaphore.WaitOne();
                foreach (Capitulo capitulo in Capitulo.GetCapitulosHome(Web).Reverse())
                {
                    if (!DicCapitulosPublished.ContainsKey(capitulo.Name))
                    {
                        DicCapitulosPublished.Add(capitulo.Name, capitulo.Name);
                        linkMega = capitulo.GetLinkMega();
                        if (!Equals(linkMega, default))
                        {
                            BotClient.SendPhotoAsync(Canal, new Telegram.Bot.Types.InputFiles.InputOnlineFile(capitulo.Picture), $"{capitulo.Name} {linkMega}");
                            Console.WriteLine(capitulo.Name);
                        }
                        else DicCapitulosPublished.Remove(capitulo.Name);
                    }
                }
                Console.WriteLine("Enter para cerrarlo y guardar");
            }
            catch { }
            finally { Semaphore.Release(); }
        }
    }
}
