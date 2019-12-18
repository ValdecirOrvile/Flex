using System;

namespace ServicoNoticias.Models
{
    public class BufferSocketServer
    {
        public string Handle { get; set; }

        public byte[] Buffer { get; set; }

        public DateTime BufferTime { get; set; }

    }
}
