using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace YouPipeDownloader
{
    internal class Playlist
    {
        private int _listSize { get; set; }
        private string _idList { get; set; }

        public Playlist(int ListSize, string IdList)
        {
            _listSize = ListSize;
            _idList = IdList;
        }

        private async Task<dynamic> GetPlayListAsync()
        {
            var parameters = new Dictionary<string, string>
            {
                ["key"] = "AIzaSyCIz-FuHD1jBmF7jcygpWQbruoquUpJOP8",
                ["playlistId"] = _idList,
                ["part"] = "snippet",
                ["fields"] = "pageInfo, items/snippet(title, description),items/snippet/resourceId(videoId)",
                ["maxResults"] = _listSize.ToString()
            };

            string baseUrl = "https://www.googleapis.com/youtube/v3/playlistItems?";
            string fullUrl = MakeUrlWithQuery(baseUrl, parameters);

            var result = await new HttpClient().GetStringAsync(fullUrl);

            if (result != null)
            {
                return JsonConvert.DeserializeObject(result);
            }

            return default(dynamic);
        }

        private string MakeUrlWithQuery(string baseUrl, IEnumerable<KeyValuePair<string, string>> parameters) //формирует полный юрл с параметрами
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentNullException(nameof(baseUrl));

            if (parameters == null || parameters.Count() == 0)
                return baseUrl;

            return parameters.Aggregate(baseUrl,
                (accumulated, kvp) => string.Format($"{accumulated}{kvp.Key}={kvp.Value}&"));
        }

        public async Task<ObservableCollection<AudioTrackProperties>> GetIdVideosInPlayList()
        {
            byte NumberInPlaylist = 1;
            ObservableCollection<AudioTrackProperties> VideoListWithId = new ObservableCollection<AudioTrackProperties>();

            var result = await GetPlayListAsync();
            if (result.items.Count > 0)

                foreach (var item in result.items)
                {
                    AudioTrackProperties ListElement = new AudioTrackProperties();
                    ListElement.Title = NumberInPlaylist.ToString() + ": " + item.snippet.title.ToString();
                    ListElement.Id = item.snippet.resourceId.videoId.ToString();
                    VideoListWithId.Add(ListElement);
                    NumberInPlaylist++;
                }
            return VideoListWithId;
        }
    }
}