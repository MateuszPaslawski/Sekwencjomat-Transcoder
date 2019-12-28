using System.Text;

namespace SekwencjomatTranscoder
{
    public static class StringExt
    {
        public static string CodecToFFmpegSyntax(this string input)
        {
            StringBuilder sb = new StringBuilder(input);

            sb.Replace("h264", "libx264");
            sb.Replace("h265", "libx265");
            sb.Replace("vp9", "libvpx-vp9");
            sb.Replace("av1", "libaom-av1 -strict -2");

            return sb.ToString();
        }

        public static string RemoveString(this string input, string stringToRemove)
        {
            return input.Replace(stringToRemove, string.Empty);
        }

        public static string TimeSpanConverter(this string input)
        {
            if (input == "empty")
            {
                return string.Empty;
            }
            else
            {
                return input.Replace(":", "-") + "s";
            }
        }
    }
}
