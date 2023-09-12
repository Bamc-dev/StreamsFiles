using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamsFiles.Entity
{
    internal class TrackInformation
    {
        public int id { get; set; }
        public string language { get; set; }
        public TrackInformation(int id, string language)
        {
            this.id = id;
            this.language = language;
        }
    }
}
