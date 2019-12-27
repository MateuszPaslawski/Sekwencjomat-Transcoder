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
using System.Threading.Tasks;

namespace SekwencjomatTranscoder
{
    class INIModel
    {
        private static readonly string AssemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static string OutputDirectory = Path.Combine(AssemblyDirectory, "output");
        private static string INIPath;
        private static string InputPath;
        private static string FFmpegPath;

        public static List<string> ListOfTimeSpans;
        public static List<string> ListOfCodecs;
        public static List<string> ListOfContainers;
        public static List<string> ListOfBitrates;
        public static List<string> ListOfResolutions;
        public static List<string> ListOfFPS;
        public static List<string> ListOfChromaSubsampling;

        private static List<List<string>> ListOfParamsLists;


        public INIModel(string iniPath)
        {
            INIPath = iniPath;
            ReadFromINI();
        }





        static void ReadFromINI()
        {
            FileIniDataParser parser = new FileIniDataParser();
            IniData iniString = parser.ReadFile(INIPath);

            InputPath = Path.GetFullPath(iniString["LocalFiles"]["InputFile"].RemoveString("\""));
            FFmpegPath = Path.GetFullPath(iniString["LocalFiles"]["FFmpeg"].RemoveString("\""));

            if (iniString["LocalFiles"]["OutputDirectory"].Trim() != string.Empty)
            {
                OutputDirectory = Path.GetFullPath(iniString["LocalFiles"]["OutputDirectory"].RemoveString("\""));
            }

            ListOfTimeSpans = INIstringToList(iniString["TranscodingParameters"]["TimeSpan"], "TimeSpan");
            ListOfCodecs = INIstringToList(iniString["TranscodingParameters"]["Codec"], "Codec");
            ListOfContainers = INIstringToList(iniString["TranscodingParameters"]["Container"], "Container");
            ListOfBitrates = INIstringToList(iniString["TranscodingParameters"]["Bitrate"], "Bitrate");
            ListOfResolutions = INIstringToList(iniString["TranscodingParameters"]["Resolution"], "Resolution");
            ListOfFPS = INIstringToList(iniString["TranscodingParameters"]["FPS"], "FPS");
            ListOfChromaSubsampling = INIstringToList(iniString["TranscodingParameters"]["ChromaSubsampling"], "ChromaSubsampling");

            ListOfParamsLists = new List<List<string>> { ListOfCodecs, ListOfContainers, ListOfBitrates, ListOfResolutions, ListOfTimeSpans, ListOfFPS, ListOfChromaSubsampling };
        }


        static List<string> INIstringToList(string INIstring, string INIvalue)
        {
            try
            {
                string trimmedInput = Regex.Replace(INIstring, @"\s+", "").Trim();

                if (trimmedInput != string.Empty)
                    return new List<string>(trimmedInput.Split(',').ToList());
                else
                    return new List<string> { "empty" };
            }
            catch
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine($"Brak wartości [{INIvalue}] w sekcji [TranscodingParameters] w pliku inicjalizacyjnym.");
                Console.WriteLine($"Ścieżka pliku: {INIPath}");
                Console.ReadKey();
                return null;
            }
        }

        private static int CountAllFilesToTranscode()
        {
            int counter = 1;

            foreach (List<string> list in ListOfParamsLists)
                if (list.Count > 0)
                    counter *= list.Count;

            return counter;
        }

        public string CheckAllPaths()
        {
            List<string> filesList = new List<string> { FFmpegPath, InputPath };

            foreach (string item in filesList)
                if (!File.Exists(item))
                    return item;

            Directory.CreateDirectory(OutputDirectory);

            return string.Empty;
        }

        public void ExecuteFFmpeg()
        {
            int filesCount = CountAllFilesToTranscode();
            int currentCounter = 0;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            //string FFmpegEndingArgs = $@" ""{outputfile}""";

            foreach (string timespan in ListOfTimeSpans)
            {
                string FFmpegArgs = $@"-nostats -loglevel 0 -y -i ""{InputPath}"" ";
                string timespan_previousFFmpegArgs = FFmpegArgs;
                string currentDir = OutputDirectory;

                if (timespan != "empty")
                {
                    //timespan/
                    currentDir = Path.Combine(currentDir, timespan.TimeSpanConverter());
                    int from = int.Parse(timespan.Split(':')[0]);
                    int to = int.Parse(timespan.Split(':')[1]) - from;
                    FFmpegArgs += $"-ss {from} -t {to} ";
                    timespan_previousFFmpegArgs = FFmpegArgs;
                }



                foreach (string codec in ListOfCodecs)
                {
                    FFmpegArgs = timespan_previousFFmpegArgs;
                    string codec_previousFFmpegArgs = FFmpegArgs;

                    if (codec != "empty")
                    {
                        //timespan/codec
                        currentDir = Path.Combine(currentDir, codec);
                        FFmpegArgs += $"-vcodec {codec.CodecToFFmpegSyntax()} ";
                        codec_previousFFmpegArgs = FFmpegArgs;
                    }



                    foreach (string container in ListOfContainers)
                    {
                        FFmpegArgs = codec_previousFFmpegArgs;
                        string container_previousFFmpegArgs = FFmpegArgs;

                        string outputContainer = container;
                        if (container != "empty")
                        {
                            //timespan/codec/container
                            currentDir = Path.Combine(currentDir, container);
                        }
                        else if (container == "empty")
                        {
                            outputContainer = Path.GetExtension(InputPath);
                        }



                        foreach (string bitrate in ListOfBitrates)
                        {
                            FFmpegArgs = container_previousFFmpegArgs;
                            string bitrate_previousFFmpegArgs = FFmpegArgs;

                            if (bitrate != "empty")
                            {
                                FFmpegArgs += $"-b:v {bitrate}k ";
                                bitrate_previousFFmpegArgs = FFmpegArgs;
                            }



                            foreach (string resolution in ListOfResolutions)
                            {
                                FFmpegArgs = bitrate_previousFFmpegArgs;
                                string resolution_previousFFmpegArgs = FFmpegArgs;

                                if (resolution != "empty")
                                {
                                    FFmpegArgs += $"-vf scale={resolution.Replace('x',':')} ";
                                    resolution_previousFFmpegArgs = FFmpegArgs;
                                }


                                foreach (string fps in ListOfFPS)
                                {
                                    FFmpegArgs = resolution_previousFFmpegArgs;
                                    string fps_previousFFmpegArgs = FFmpegArgs;

                                    if (fps != "empty")
                                    {
                                        FFmpegArgs += $"-filter:v fps=fps={fps} ";
                                        fps_previousFFmpegArgs = FFmpegArgs;
                                    }

                                    foreach (string chroma in ListOfChromaSubsampling)
                                    {
                                        FFmpegArgs = fps_previousFFmpegArgs;
                                        string chroma_previousFFmpegArgs = FFmpegArgs;

                                        if (chroma != "empty")
                                        {
                                            FFmpegArgs += $"-pix_fmt {chroma} ";
                                            chroma_previousFFmpegArgs = FFmpegArgs;
                                        }
                                        Console.WriteLine(FFmpegArgs);
                                    }
                                }
                            }
                        }
                    }
                }
            }
                
            
            Console.ReadKey();
            return;

            foreach (string codec in ListOfCodecs)
            {
                //codec/
                string codecPath = Path.Combine(OutputDirectory, codec);
                Directory.CreateDirectory(codecPath);
                foreach (string container in ListOfContainers)
                {
                    //codec/container/
                    string containerPath = Path.Combine(codecPath, container);
                    Directory.CreateDirectory(containerPath);
                    if (ListOfBitrates.Count == 0 && ListOfResolutions.Count > 0)
                    {
                        foreach (string resolution in ListOfResolutions)
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
                    else if (ListOfBitrates.Count > 0 && ListOfResolutions.Count == 0)
                    {
                        foreach (string bitrate in ListOfBitrates)
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
                    else if (ListOfBitrates.Count == 0 && ListOfResolutions.Count == 0)
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
                    else if (ListOfBitrates.Count > 0 && ListOfResolutions.Count > 0)
                    {
                        foreach (string resolution in ListOfResolutions)
                        {
                            //codec/container/resolution
                            string resolutionPath = Path.Combine(containerPath, resolution);
                            Directory.CreateDirectory(resolutionPath);

                            foreach (string bitrate in ListOfBitrates)
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
        }
    }
}
