using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouPipeDownloader.MappingResponse
{
    internal class RootItems
    {
        [JsonProperty("snippet")]
        public SnippetInfo Snippet { get; set; }

        [JsonProperty("contentDetails")]
        public ContentInfo Content { get; set; }
}
}