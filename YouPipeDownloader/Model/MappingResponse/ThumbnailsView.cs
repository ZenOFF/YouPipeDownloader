﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouPipeDownloader.MappingResponse
{
    internal class ThumbnailsView
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
