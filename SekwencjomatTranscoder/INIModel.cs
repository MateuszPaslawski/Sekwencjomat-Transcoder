using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SekwencjomatTranscoder
{
    class INIModel
    {
        public INIModel()
        {

        }

        enum LocalFiles
        {
            FFmpeg,
            InputFile,
            OutputDirectory,
        }

        enum TranscodingParameters
        {
            Codec,
            Container,
            Bitrate,
            Resolution,
        }
    }
}
