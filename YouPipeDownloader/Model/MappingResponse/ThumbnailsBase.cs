using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouPipeDownloader.MappingResponse
{
    internal class ThumbnailsBase
    {
        [JsonProperty("default")]
        public ThumbnailsView Default { get; set; }
    }
}
