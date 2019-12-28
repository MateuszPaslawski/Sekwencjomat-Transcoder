using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SekwencjomatTranscoder
{
    class ConsoleLogger
    {
        private static int beforeConsoleWidth = 0;
        private static int beforeConsoleHeight = 0;

        public const int margin = 20;
        public static int delay = 50;
        public static char character = '■';

        private static bool canOutput = true;

        public static void StartOutput()
        {
            canOutput = true;

            Console.CursorVisible = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Task.Run(() =>
            {
                while (canOutput)
                {
                    if (beforeConsoleWidth != Console.WindowWidth || beforeConsoleHeight != Console.WindowHeight)
                    {
                        beforeConsoleWidth = Console.WindowWidth;
                        beforeConsoleHeight = Console.WindowHeight;
                        Console.Clear();
                        Console.CursorVisible = false;
                    }

                    Console.ForegroundColor = ConsoleColor.White;

                    string precent = ((double)INIModel.CurrentFile / INIModel.FilesCount * 100).ToString("0.00");
                    int current = INIModel.CurrentFile;
                    int max = INIModel.FilesCount;

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.SetCursorPosition(0, 0);
                    Console.Write(".  ");
                    Thread.Sleep(delay);
                    Console.SetCursorPosition(0, 0);
                    Console.Write(".. ");
                    Thread.Sleep(delay);
                    Console.SetCursorPosition(0, 0);
                    Console.Write("...");
                    Thread.Sleep(delay);
                    Console.SetCursorPosition(0, 0);
                    Console.Write(":..");
                    Thread.Sleep(delay);
                    Console.SetCursorPosition(0, 0);
                    Console.Write("::.");
                    Thread.Sleep(delay);
                    Console.SetCursorPosition(0, 0);
                    Console.Write(".::");
                    Thread.Sleep(delay);
                    Console.SetCursorPosition(0, 0);
                    Console.Write("..:");
                    Thread.Sleep(delay);
                    Console.SetCursorPosition(0, 0);
                    Console.Write("...");
                    Thread.Sleep(delay);
                    Console.SetCursorPosition(0, 0);
                    Console.Write(" ..");
                    Thread.Sleep(delay);
                    Console.SetCursorPosition(0, 0);
                    Console.Write("  .");
                    Thread.Sleep(delay);
                    Console.SetCursorPosition(0, 0);
                    Console.Write("   ");

                    Thread.Sleep(delay);
                    Console.SetCursorPosition(5, 1);
                    Console.ForegroundColor = ConsoleColor.DarkRed;

                    int length = Console.WindowWidth - 25;
                    double ratio = (double)length / max;
                    double j = 1;

                    while (j <= current * ratio)
                    {
                        if (j > (length) / 3)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                        }

                        if (j > 2 * (length) / 3)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                        }

                        Console.Write(character.ToString());
                        ++j;
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($" [{precent} %]");

                    Console.SetCursorPosition(0, 2);
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"\tCzas od rozpoczęcia: {sw.Elapsed.ToString(@"hh\:mm\:ss")}\t\t");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\tPrzetwarzanie pliku: [ {current} / {max} ]");

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"\n{"Przedział czasu:",margin}");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"\t{INIModel.CurrentTimeSpan}");

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"{"Kodek wideo:",margin}");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"\t{INIModel.CurrentCodec}");

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"{"Kontener:",margin}");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"\t{INIModel.CurrentContainer}");

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"{"Bitrate:",margin}");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"\t{INIModel.CurrentBitrate}");

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"{"Rozdzielczość:",margin}");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"\t{INIModel.CurrentResolution}");

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"{"FPS:",margin}");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"\t{INIModel.CurrentFPS}");

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"{"Kodowanie barwy:",margin}");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"\t{INIModel.CurrentChromaSubsampling}");

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"\n\n{"Ścieżka pliku:",margin}");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"\t{INIModel.CurrentFilePath,margin}\n\n");
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                    Thread.Sleep(delay);
                }
            });

        }

        public void StopOutput()
        {
            canOutput = false;
        }
    }
}
