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
using System.Threading.Tasks;


namespace SekwencjomatTranscoder
{
    class Program
    {
        private static string INIfilePath = string.Empty;
        private static string INISearchPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static string FFmpegPath = string.Empty;
        private static string InputPath = string.Empty;
        private static string OutputPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "output");
        private static string TemplatePath = Path.Combine(INIfilePath, "szablon.ini");
        private static List<string>  listCodecs;
        private static List<string>  listContainers;
        private static List<string>  listBitrates;
        private static List<string> listResolutions;

        private static void EmbeddedTemplateToDisk()
        {
            byte[] byte_array = Encoding.UTF8.GetBytes( Properties.Resources.templateINI);
            File.WriteAllBytes(TemplatePath, byte_array);
        }

        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Black;
            if (args.Count() > 1)
            {
                Console.WriteLine($"Podano niewłaściwą ilośc argumentów: [{args.Count()}]\nMaksymalna wartość to: 1");
                Console.ReadKey();
                return;
            }

            if (args.Count() == 1)
            {

                if (File.Exists(args.First()))
                {
                    INIfilePath = args.First();
                }
                else
                {
                    Console.WriteLine($"Podany plik nie istnieje: [{args.First()}]");
                    Console.ReadKey();
                    return;
                }
            }
            if (args.Count() == 0)
            {
                foreach (var item in new DirectoryInfo(INISearchPath).GetFiles())
                {
                    FileInfo fi = new FileInfo(item.FullName);
                    if (fi.Extension == ".ini" && fi.Name.ToLower() != "desktop.ini")
                        INIfilePath = item.FullName.ToString();
                }
            }
            if (!CheckINIPath())
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
                    var proc = new Process();
                    proc.StartInfo.Arguments = TemplatePath;
                    proc.StartInfo.FileName = "notepad.exe";
                    proc.Start();
                    return;
                }

                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine($"Plik inicjalizacyjny nie został podany bądź nie istnieje.");
                Console.WriteLine($"Podana ścieżka: [{INIfilePath}]");
                Console.WriteLine();
                Console.ReadKey();
                return;
            }
            FillGlobalVariables();

            string pathCheck = CheckAllPaths();
            if (pathCheck != string.Empty)
            {
                Console.WriteLine($"Plik bądź ścieżka nie istnieją: {pathCheck}");
                Console.ReadKey();
            }


            if (listCodecs.Count == 0)
            {
                Console.WriteLine("Liczba wybranych kodeków wideo nie może być równa 0. Wybierz przynajmniej jeden.");
                Console.ReadKey();
                return;
            }
            if (listContainers.Count == 0)
            {
                Console.WriteLine("Liczba wybranych kontenerów wideo nie może być równa 0. Wybierz przynajmniej jeden.");
                Console.ReadKey();
                return;
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;

            int filesCount = CountAllFilesToTranscode();
            int currentCounter = 0;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (var codec in listCodecs)
            {
                //codec/
                string codecPath = Path.Combine(OutputPath, codec);
                Directory.CreateDirectory(codecPath);
                foreach (var container in listContainers)
                {
                    //codec/container/
                    string containerPath = Path.Combine(codecPath, container);
                    Directory.CreateDirectory(containerPath);
                    if (listBitrates.Count == 0 && listResolutions.Count > 0)
                    {
                        foreach (var resolution in listResolutions)
                        {
                            Process proc = new Process();
                            proc.StartInfo.FileName = FFmpegPath;
                            string outputfile = Path.Combine(containerPath, $"{resolution}.{container}");
                            proc.StartInfo.Arguments = $@"-nostats -loglevel 0 -y -i ""{InputPath}"" -vcodec {codec.CodecToFFmpegSyntax()} ""{outputfile}""";
                            proc.StartInfo.UseShellExecute = false;

                            int precent = (int)((double)++currentCounter / filesCount * 100);

                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine($"Czas od rozpoczęcia: {sw.Elapsed.ToString(@"hh\:mm\:ss")}\t\t");
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"Przetwarzanie pliku: [ {currentCounter} / {filesCount} ] {precent}%");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.Write($"Kodek wideo:\t");
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"{codec,10}");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.Write($"Kontener:\t");
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"{container,10}");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.Write($"Rozdzielczość:\t");
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"{resolution,10}");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine($"\nŚcieżka pliku:");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write($"{outputfile,10}\n\n");
                            Console.ForegroundColor = ConsoleColor.DarkGray;

                            proc.Start();
                            proc.WaitForExit();
                        }

                    }
                    else if (listBitrates.Count > 0 && listResolutions.Count == 0)
                    {
                        foreach (var bitrate in listBitrates)
                        {
                            Process proc = new Process();
                            proc.StartInfo.FileName = FFmpegPath;
                            string outputfile = Path.Combine(containerPath, $"{bitrate}k.{container}");
                            proc.StartInfo.Arguments = $@"-nostats -loglevel 0 -y -i ""{InputPath}"" -vcodec {codec.CodecToFFmpegSyntax()} ""{outputfile}""";
                            proc.StartInfo.UseShellExecute = false;

                            int precent = (int)((double)++currentCounter / filesCount * 100);

                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine($"Czas od rozpoczęcia: {sw.Elapsed.ToString(@"hh\:mm\:ss")}\t\t");
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"Przetwarzanie pliku: [ {currentCounter} / {filesCount} ] {precent}%");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.Write($"Kodek wideo:\t");
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"{codec,10}");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.Write($"Kontener:\t");
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"{container,10}");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.Write($"Bitrate:\t");
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"{bitrate,10}");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine($"\nŚcieżka pliku:");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write($"{outputfile,10}\n\n");
                            Console.ForegroundColor = ConsoleColor.DarkGray;

                            proc.Start();
                            proc.WaitForExit();
                        }
                    }
                    else if (listBitrates.Count == 0 && listResolutions.Count == 0)
                    {
                        Process proc = new Process();
                        proc.StartInfo.FileName = FFmpegPath;
                        string outputfile = Path.Combine(containerPath, $"{codec}.{container}");
                        proc.StartInfo.Arguments = $@"-nostats -loglevel 0 -y -i ""{InputPath}"" -vcodec {codec.CodecToFFmpegSyntax()} ""{outputfile}""";
                        proc.StartInfo.UseShellExecute = false;

                        int precent = (int)((double)++currentCounter / filesCount * 100);
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine($"Czas od rozpoczęcia: {sw.Elapsed.ToString(@"hh\:mm\:ss")}\t\t");
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"Przetwarzanie pliku: [ {currentCounter} / {filesCount} ] {precent}%");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write($"Kodek wideo:\t");
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"{codec,10}");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write($"Kontener:\t");
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"{container,10}");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($"\nŚcieżka pliku:");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write($"{outputfile,10}\n\n");
                        Console.ForegroundColor = ConsoleColor.DarkGray;

                        proc.Start();
                        proc.WaitForExit();
                    }
                    else if (listBitrates.Count > 0 && listResolutions.Count > 0)
                    {
                        foreach (var resolution in listResolutions)
                        {
                            //codec/container/resolution
                            string resolutionPath = Path.Combine(containerPath, resolution);
                            Directory.CreateDirectory(resolutionPath);

                            foreach (var bitrate in listBitrates)
                            {
                                Process proc = new Process();
                                proc.StartInfo.FileName = FFmpegPath;
                                string outputfile = Path.Combine(resolutionPath, $"{resolution}-{bitrate}k.{container}");
                                proc.StartInfo.Arguments = $@"-nostats -loglevel 0 -y -i ""{InputPath}"" -vcodec {codec.CodecToFFmpegSyntax()} ""{outputfile}""";
                                proc.StartInfo.UseShellExecute = false;

                                int precent = (int)((double)++currentCounter / filesCount * 100);

                                Console.Clear();
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine($"Czas od rozpoczęcia: {sw.Elapsed.ToString(@"hh\:mm\:ss")}\t\t");
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine($"Przetwarzanie pliku: [ {currentCounter} / {filesCount} ] {precent}%");
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.Write($"Kodek wideo:\t");
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine($"{codec,10}");
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.Write($"Kontener:\t");
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine($"{container,10}");
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.Write($"Rozdzielczość:\t");
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine($"{resolution,10}");
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.Write($"Bitrate:\t");
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine($"{bitrate,10}");
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.WriteLine($"\nŚcieżka pliku:");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.Write($"{outputfile,10}\n\n");
                                Console.ForegroundColor = ConsoleColor.DarkGray;

                                proc.Start();
                                proc.WaitForExit();
                            }
                        }
                    }

                }

            }

            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine($"Transkodowanie plików wideo zakończone pomyślnie w czasie: {sw.Elapsed.ToString(@"hh\:mm\:ss")}.");
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("Opuszczanie za 3 sekundy...");
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Thread.Sleep(1000);
            Console.WriteLine("Opuszczanie za 2 sekundy....");
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Thread.Sleep(1000);
            Console.WriteLine("Opuszczanie za 1 sekundę.....");
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Thread.Sleep(1000);

            Process.Start($"explorer.exe", OutputPath);
        }

        static List<string> INIstringToList(string INIstring, string INIvalue)
        {
            try
            {
                string trimmedInput = Regex.Replace(INIstring, @"\s+", "").Trim();
                if (trimmedInput != string.Empty)
                    return new List<string>(trimmedInput.Split(',').ToList());
                else
                    return new List<string>();

            }
            catch
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine($"Brak wartości [{INIvalue}] sekcji [TranscodingParameters] w pliku inicjalizacyjnym.");
                Console.WriteLine($"Ścieżka pliku: {INIfilePath}");
                Console.ReadKey();
                return null;
            }
        }

        static void FillGlobalVariables()
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(INIfilePath);
            Console.WriteLine(INIfilePath);
            InputPath = Path.GetFullPath(data["LocalFiles"]["InputFile"].Replace("\"",""));
            FFmpegPath = Path.GetFullPath(data["LocalFiles"]["FFmpeg"].Replace("\"", ""));
            if (data["LocalFiles"]["OutputDirectory"].Trim() != string.Empty)
                OutputPath = Path.GetFullPath(data["LocalFiles"]["OutputDirectory"].Replace("\"", ""));

            listCodecs = INIstringToList(data["TranscodingParameters"]["Codec"], "Codec");
            listContainers = INIstringToList(data["TranscodingParameters"]["Container"], "Container");
            listBitrates = INIstringToList(data["TranscodingParameters"]["Bitrate"], "Bitrate");
            listResolutions = INIstringToList(data["TranscodingParameters"]["Resolution"], "Resolution");
        }

        static int CountAllFilesToTranscode()
        {
            int counter = 1;
            counter *= listCodecs.Count;
            counter *= listContainers.Count;

            if (listBitrates.Count > 0)
                counter *= listBitrates.Count;

            if (listResolutions.Count > 0)
                counter *= listResolutions.Count;

            return counter;
        }

        static string CheckAllPaths()
        {
            var filesList = new List<string> { FFmpegPath, InputPath };

            foreach (var item in filesList)
            {
                if (!File.Exists(item))
                    return item;
            }

            Directory.CreateDirectory(OutputPath);

            return string.Empty;
        }

        static bool CheckINIPath()
        {
            if (!File.Exists(INIfilePath))
                return false;
            else
                return true;
        }
    }
}
