using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouPipeDownloader.MappingResponse
{
    internal class ContentInfo
    {
        [JsonProperty("duration")]
        public string Duration { get; set; }    
    }
}
