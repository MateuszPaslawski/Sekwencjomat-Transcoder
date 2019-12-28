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

        public static string CurrentTimeSpan = "0:0";
        public static string CurrentCodec = string.Empty;
        public static string CurrentContainer = string.Empty;
        public static string CurrentBitrate = string.Empty;
        public static string CurrentResolution = string.Empty;
        public static string CurrentFPS = string.Empty;
        public static string CurrentChromaSubsampling = string.Empty;
        public static string CurrentFilePath = string.Empty;

        private static List<List<string>> ListOfParamsLists;

        public static int CurrentFile = 1;
        public static int FilesCount = 1;


        public INIModel(string iniPath)
        {
            INIPath = iniPath;

            ReadFromINI();

            FilesCount = CountAllFilesToTranscode();
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

            FilesCount = counter;
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
            ConsoleLogger.StartOutput();
            int currentCounter = 0;

            foreach (string timespan in ListOfTimeSpans)
            {
                string FFmpegArgs = $@"-nostats -loglevel 0 -y -i ""{InputPath}"" -cpu-used {Environment.ProcessorCount} ";
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

                    string output_codec = string.Empty;

                    if (codec != "empty")
                    {
                        //timespan/codec
                        output_codec = codec;
                        currentDir = Path.Combine(currentDir, codec);
                        FFmpegArgs += $"-vcodec {codec.CodecToFFmpegSyntax()} ";
                        codec_previousFFmpegArgs = FFmpegArgs;
                    }



                    foreach (string container in ListOfContainers)
                    {
                        FFmpegArgs = codec_previousFFmpegArgs;
                        string container_previousFFmpegArgs = FFmpegArgs;

                        string output_Container = Path.GetExtension(InputPath);
                        if (container != "empty")
                        {
                            //timespan/codec/container
                            output_Container = container;
                            currentDir = Path.Combine(currentDir, container);
                        }


                        foreach (string bitrate in ListOfBitrates)
                        {
                            FFmpegArgs = container_previousFFmpegArgs;
                            string bitrate_previousFFmpegArgs = FFmpegArgs;

                            string output_bitrate = string.Empty;

                            if (bitrate != "empty")
                            {
                                output_bitrate = bitrate + "k";
                                FFmpegArgs += $"-b:v {bitrate}k ";
                                bitrate_previousFFmpegArgs = FFmpegArgs;
                            }



                            foreach (string resolution in ListOfResolutions)
                            {
                                FFmpegArgs = bitrate_previousFFmpegArgs;
                                string resolution_previousFFmpegArgs = FFmpegArgs;

                                string output_resolution = string.Empty;

                                if (resolution != "empty")
                                {
                                    output_resolution = resolution;
                                    FFmpegArgs += $"-vf scale={resolution.Replace('x',':')} ";
                                    resolution_previousFFmpegArgs = FFmpegArgs;
                                }


                                foreach (string fps in ListOfFPS)
                                {
                                    FFmpegArgs = resolution_previousFFmpegArgs;
                                    string fps_previousFFmpegArgs = FFmpegArgs;

                                    string output_fps = string.Empty;

                                    if (fps != "empty")
                                    {
                                        output_fps = $"[{fps}fps]";
                                        FFmpegArgs += $"-filter:v fps=fps={fps} ";
                                        fps_previousFFmpegArgs = FFmpegArgs;
                                    }

                                    foreach (string chroma in ListOfChromaSubsampling)
                                    {
                                        FFmpegArgs = fps_previousFFmpegArgs;
                                        string chroma_previousFFmpegArgs = FFmpegArgs;

                                        string output_chroma = string.Empty;

                                        if (chroma != "empty")
                                        {
                                            output_chroma = chroma;
                                            FFmpegArgs += $"-pix_fmt {chroma} ";
                                            chroma_previousFFmpegArgs = FFmpegArgs;
                                        }

                                        string outputFileName = $"{output_resolution} {output_bitrate} {output_chroma} {output_fps}".Replace("  ", " ").Trim().Replace(" ", "_");

                                        if (outputFileName == string.Empty)
                                            outputFileName = Path.GetFileNameWithoutExtension(InputPath);

                                        string outputPath = $"{outputFileName}.{output_Container.RemoveString(".")}";
                                        Directory.CreateDirectory(currentDir);
                                        outputPath = Path.Combine(currentDir, outputPath);
                                        FFmpegArgs += $"\"{outputPath}\"";

                                        CurrentFilePath = outputPath;
                                        CurrentTimeSpan = timespan.TimeSpanConverter();
                                        CurrentCodec = output_codec;
                                        CurrentContainer = output_Container;
                                        CurrentBitrate = output_bitrate;
                                        CurrentResolution = output_resolution;
                                        CurrentFPS = output_fps;
                                        CurrentChromaSubsampling = output_chroma;
                                        CurrentFile = ++currentCounter;
                                        RunFFmpegProcess(FFmpegArgs);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void RunFFmpegProcess(string arg)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = FFmpegPath;
            proc.StartInfo.Arguments = arg;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            proc.WaitForExit();
        }

       
    }
}
