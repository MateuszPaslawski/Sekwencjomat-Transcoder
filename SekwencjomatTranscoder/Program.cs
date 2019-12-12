using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace SekwencjomatTranscoder
{
    class Program
    {
        private static string FFmpegPath = string.Empty;
        private static string INIfilePath = string.Empty;
        
        static void Main(string[] args)
        {
            if (args.Count() > 1)
            {
                Console.WriteLine($"Podano niewłaściwą ilośc argumentów: [{args.Count()}]\nMaksymalna wartość to: [1]");
                return;
            }

            if (args.Count() == 1)
            {

                if (!File.Exists(args.First()))
                {
                    Console.WriteLine($"Podany plik nie istanieje: [{args.First()}]");
                    return;
                }
                INIfilePath = args.First();
            }

            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(@"..\..\test.ini");

            //Console.WriteLine(data["LocalFiles"]["FFmpeg"]); 
            //Console.WriteLine(data["LocalFiles"]["ReferenceVideoFile"]);
            var codecString = Regex.Replace(data["TranscodingParameters"]["Codec"], @"\s+", "");
            var codecList = new List<string>(codecString.Split(',').ToList());

            foreach (var item in codecList)
                Console.WriteLine($"List Item: {item}");

            //Console.WriteLine(data["TranscodingParameters"]["Codec"]); 
            //Console.WriteLine(data["TranscodingParameters"]["Containes"]); 
            //Console.WriteLine(data["TranscodingParameters"]["Bitrate"]); 

            Console.ReadKey();
        }
    }
}
