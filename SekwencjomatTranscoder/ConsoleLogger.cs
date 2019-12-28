using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SekwencjomatTranscoder
{
    class ConsoleLogger
    {
        private static int initialConsoleWidth = 0;
        private static int initialConsoleHeight = 0;
        public const int margin = 20;
        public static int delay = 50;
        public static char character = '■';

        private static bool canAnimate = true;

        public static async Task StartOutput()
        {
           

            Console.CursorVisible = false;
            canAnimate = true;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            await Task.Run(() =>
            {
                while (canAnimate)
                {
                    if (initialConsoleWidth != Console.WindowWidth || initialConsoleHeight != Console.WindowHeight)
                    {
                        initialConsoleWidth = Console.WindowWidth;
                        initialConsoleHeight = Console.WindowHeight;
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
                            Console.ForegroundColor = ConsoleColor.DarkYellow;

                        if (j > 2 * (length) / 3)
                            Console.ForegroundColor = ConsoleColor.DarkGreen;

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
                    Console.Write($"{"Kodek wideo:", margin}");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"\t{INIModel.CurrentCodec}");

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"{"Kontener:", margin}");
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
                    Console.WriteLine($"\n\nŚcieżka pliku:");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"\t{INIModel.CurrentFilePath,margin}\n\n");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    
                    Thread.Sleep(delay);
                }
            });

        }

        public void StopOutput()
        {
            canAnimate = false;
        }

        public static void WriteOnBottomLine(string text)
        {
            Console.CursorTop = Console.WindowTop + Console.WindowHeight - 1;
            Console.Write(text);
        }
    }
}
