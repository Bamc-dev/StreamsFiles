using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamsFiles.Entity
{
    public class FileChunk
    {
        public string FileId { get; set; }
        public int ChunkNumber { get; set; }
        public byte[] Data { get; set; }
    }
}
