using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouPipeDownloader.MappingResponse
{
    internal class AudioTrackInfoApiResponse
    {
        [JsonProperty("items")]
        public ObservableCollection<RootItems> Items { get; set; }
    }
}