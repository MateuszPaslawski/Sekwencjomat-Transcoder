using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;


namespace SekwencjomatTranscoder
{
    class Program
    {
        private static readonly string AssemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static string OutputDirectory = Path.Combine(AssemblyDirectory, "output");
        private static string INIPath = string.Empty;
        private static readonly string TemplatePath = Path.Combine(INIPath, "szablon.ini");

        private static void EmbeddedTemplateToDisk()
        {
            File.WriteAllBytes(TemplatePath, Encoding.UTF8.GetBytes(Properties.Resources.templateINI));
        }

        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Black;

            if(!CheckArgs(args))
            {
                Console.ReadKey();
                return;
            }

            CheckINIPath();


            INIModel iniModel = new INIModel(INIPath);

            string pathCheck = iniModel.CheckAllPaths();

            if (pathCheck != string.Empty)
            {
                Console.WriteLine($"Plik bądź ścieżka nie istnieją: {pathCheck}");
                Console.ReadKey();
                Environment.Exit(0);
            }

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;

            iniModel.ExecuteFFmpeg();

        }

        static bool CheckArgs(string[] args)
        {
            if (args.Count() > 1)
            {
                Console.WriteLine($"Podano niewłaściwą ilośc argumentów: [{args.Count()}]\nMaksymalna wartość to: 1");
                return false;
            }

            if (args.Count() == 1)
            {

                if (File.Exists(args.First()))
                {
                    INIPath = args.First();
                    return true;
                }
                else
                {
                    Console.WriteLine($"Podany plik nie istnieje: [{args.First()}]");
                    return false;
                }
            }

            if (args.Count() == 0)
            {
                foreach (FileInfo file in new DirectoryInfo(AssemblyDirectory).GetFiles())
                {
                    FileInfo fi = new FileInfo(file.FullName);
                    if (fi.Extension == ".ini" && fi.Name.ToLower() != "desktop.ini")
                    {
                        INIPath = file.FullName.ToString();
                        return true;
                    }
                }
            }

            return false;
        }

        static void CheckINIPath()
        {
            if (!File.Exists(INIPath))
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Nie znaleziono pliku inicjalizacyjnego, czy chcesz utworzyć i edytować szablon w lokalizacji programu?" +
                    $"\nSzablon zawiera opis oraz przykładowe wartości.");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"[T]ak\t[N]ie\n");

                string key;

                while (true)
                {
                    Console.Write(">");
                    key = Console.ReadKey(true).KeyChar.ToString().ToLower();

                    if (key == "t" || key == "n")
                        break;
                }
                Console.WriteLine();
                Console.WriteLine();
                if (key == "t")
                {
                    EmbeddedTemplateToDisk();
                    Process proc = new Process();
                    proc.StartInfo.Arguments = TemplatePath;
                    proc.StartInfo.FileName = "notepad.exe";
                    proc.Start();
                    return;
                }

                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine($"Plik inicjalizacyjny nie został podany bądź nie istnieje.");
                Console.WriteLine($"Podana ścieżka: [{INIPath}]");
                Console.WriteLine();
                Console.ReadKey();
                return;
            }
        }
    }
}
