using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VideoLibrary;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace YouPipeDownloader
{
    internal class AudioTrack
    {
        private string _idSong;
        private IEnumerable<YouTubeVideo> _testVideoInfo = new ObservableCollection<YouTubeVideo>();
        private string _title;

        public AudioTrack(string IdSong)
        {
            _idSong = IdSong;
        }

        public AudioTrack(string IdSong, string Title) : this(IdSong)
        {
            _title = Title;
        }

        public delegate void updDel();

        public async Task<AudioTrackProperties> GetInfo(string UrlYouTube = "https://www.youtube.com/watch?v=")
        {
            //создаём экземляр класса СвойстАудиоДорожки
            AudioTrackProperties audioTrackProperties = new AudioTrackProperties();
            //выполняем запрос к API и получаем длительность аудио дорожки
            var videoInfoString = await RequestAudioDurationAsync();
            audioTrackProperties.Duration = GetVideoDuration(videoInfoString);
            //выполняем запрос к API и получаем название и описание аудио дорожки
            videoInfoString = await RequestAudioInfoAsync();
            audioTrackProperties.Title = GetVideoTitle(videoInfoString);
            audioTrackProperties.Description = GetVideoDescription(videoInfoString);
            //выполняем запрос к API и получаем адрес к Thumbnail
            audioTrackProperties.Thumbnail = new BitmapImage(new Uri(GetVideoThumbnails(videoInfoString)));
            return audioTrackProperties;
        }

        public string GetVideoDescription(dynamic NonFormatingString)
        {
            string Description = "";
            if (NonFormatingString.items.Count > 0)

                foreach (var item in NonFormatingString.items)
                {
                    Description = item.snippet.description.ToString();
                }
            return Description;
        }

        public string GetVideoDuration(dynamic NonFormatingString)
        {
            string Duration = "";
            if (NonFormatingString.items.Count > 0)

                foreach (var item in NonFormatingString.items)
                {
                    Duration = item.contentDetails.duration.ToString();
                }
            return Duration;
        }

        public string GetVideoThumbnails(dynamic NonFormatingString)
        {
            string ThumbnailUrl = "";
            if (NonFormatingString.items.Count > 0)

                foreach (var item in NonFormatingString.items)
                {
                    ThumbnailUrl = item.snippet.thumbnails.@default.url.ToString();
                }
            return ThumbnailUrl;
        }

        public string GetVideoTitle(dynamic NonFormatingString)
        {
            string Title = "";
            if (NonFormatingString.items.Count > 0)

                foreach (var item in NonFormatingString.items)
                {
                    Title = item.snippet.title.ToString();
                }
            return Title;
        }

        public async Task SaveAudioTrack(string UrlYouTube = "https://www.youtube.com/watch?v=")
        {
            var youtube = YouTube.Default;
            //var resolutions = videoInfos.Where(j => j.AdaptiveKind == AdaptiveKind.Video).Select(j => j.Resolution);
            //var bitRates = videoInfos.Where(j => j.AdaptiveKind == AdaptiveKind.Audio).Select(j => j.AudioBitrate);
            //var unknownFormats = videoInfos.Where(j => j.AdaptiveKind == AdaptiveKind.None).Select(j => j.Resolution);
            try
            {
                var videos = youtube.GetAllVideosAsync(String.Concat(UrlYouTube, _idSong)).GetAwaiter().GetResult();

                var minResolution = videos.First(i => i.Resolution == videos.Min(j => j.Resolution));

                //HttpWebRequest httpWebRequest = new HttpWebRequest()

                System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

                HttpResponseMessage response = await client.GetAsync(minResolution.Uri.ToString(), HttpCompletionOption.ResponseHeadersRead);

                Debug.WriteLine(response.Content.Headers.ContentLength + "ДЛИНА ОТВЕТА");
                ObservableCollection<ContentPart> responseContentParts = GetContentParts(5, response.Content.Headers.ContentLength);
                ObservableCollection<Task<byte[]>> tasksQueue = new ObservableCollection<Task<byte[]>>();
                foreach (ContentPart part in responseContentParts)
                {
                    tasksQueue.Add(SendRequestDownloadPart(client, minResolution.Uri, part));
                }
                //wait end of downloading
                byte[][] continuation = await Task.WhenAll(tasksQueue);

                Array.Reverse(continuation);
                //get result byte array size
                byte[] resultCombineArrays = Combine(continuation);

                //создание временного файла
                StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
                StorageFile videoFile = await tempFolder.CreateFileAsync(_idSong + ".m4a", CreationCollisionOption.ReplaceExisting);
                //сохранение
                await FileIO.WriteBytesAsync(videoFile, resultCombineArrays);
                //конвертирование
                await StartConvertAudioFile(videoFile);
            }
            catch (Exception e)
            {
                MessageDialog messageDialog = new MessageDialog(e.Message);
                await messageDialog.ShowAsync();
                return;
            }
        }

        private byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        private async Task<byte[]> SendRequestDownloadPart(HttpClient client, string uri, ContentPart contentPart)
        {
            var req = new HttpRequestMessage { RequestUri = new Uri(uri) };
            req.Headers.Range = new RangeHeaderValue(contentPart.StartIndex, contentPart.EndIndex);
            HttpResponseMessage responsePart = await client.SendAsync(req);
            byte[] bytesResponse = await responsePart.Content.ReadAsByteArrayAsync();
            return bytesResponse;
        }

        private ObservableCollection<ContentPart> GetContentParts(int partCount, long? contentLenght)
        {
            ObservableCollection<ContentPart> result = new ObservableCollection<ContentPart>();
            long? rangeContentSize = contentLenght / partCount;
            while (contentLenght >= rangeContentSize)
            {
                ContentPart contentPart = new ContentPart();
                contentPart.StartIndex = contentLenght - rangeContentSize;
                contentPart.EndIndex = contentLenght;
                contentLenght = contentLenght - (rangeContentSize + 1);
                result.Add(contentPart);
            }
            if (contentLenght < rangeContentSize && contentLenght > 0)
            {
                ContentPart contentPart = new ContentPart();
                contentPart.StartIndex = 0;
                contentPart.EndIndex = contentLenght;
                result.Add(contentPart);
            }
            return result;
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

        private async Task<dynamic> RequestAudioDurationAsync()
        {
            var parameters = new Dictionary<string, string>
            {
                ["key"] = "AIzaSyCIz-FuHD1jBmF7jcygpWQbruoquUpJOP8",
                ["id"] = _idSong,
                ["part"] = "contentDetails",
                ["fields"] = "items/contentDetails(duration)",
            };

            string baseUrl = "https://www.googleapis.com/youtube/v3/videos?";
            string fullUrl = MakeUrlWithQuery(baseUrl, parameters);

            var result = await new System.Net.Http.HttpClient().GetStringAsync(new Uri(fullUrl));

            if (result != null)
            {
                return JsonConvert.DeserializeObject(result);
            }

            return default(dynamic);
        }

        private async Task<dynamic> RequestAudioInfoAsync()
        {
            var parameters = new Dictionary<string, string>
            {
                ["key"] = "AIzaSyCIz-FuHD1jBmF7jcygpWQbruoquUpJOP8",
                ["id"] = _idSong,
                ["part"] = "snippet",
                ["fields"] = "items/snippet(title,description),items/snippet/thumbnails/default(url)",
            };

            string baseUrl = "https://www.googleapis.com/youtube/v3/videos?";
            string fullUrl = MakeUrlWithQuery(baseUrl, parameters);

            var result = await new System.Net.Http.HttpClient().GetStringAsync(new Uri(fullUrl));

            if (result != null)
            {
                return JsonConvert.DeserializeObject(result);
            }

            return default(dynamic);
        }

        private async Task<StorageFile> SelectTargetFile()
        {
            try
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();

                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
                savePicker.DefaultFileExtension = ".mp3";
                savePicker.SuggestedFileName = _title;
                savePicker.FileTypeChoices.Add(".MP3", new List<string>() { ".mp3" });
                return await savePicker.PickSaveFileAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"An error had been throwed, {ex.Message}");
                return null;
            }
        }

        private async Task StartConvertAudioFile(StorageFile VideoOnDisk)
        {
            StorageFile DestinationFile = await SelectTargetFile();
            if (DestinationFile == null)
            {
                return;
            }
            try
            {
                //determine MediaEncodingProfiel
                MediaEncodingProfile profile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High);
                //init convert workder object
                MediaTranscoder transcoder = new MediaTranscoder();
                PrepareTranscodeResult prepareOp = await transcoder.PrepareFileTranscodeAsync(VideoOnDisk, DestinationFile, profile);
                if (prepareOp.CanTranscode)
                {
                    //start convert
                    await prepareOp.TranscodeAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"An error had been throwed, {ex.Message}");
            }
            //удаление TempVideo
            await VideoOnDisk.DeleteAsync();
            MessageDialog messageDialog = new MessageDialog("Converting is done");
            await messageDialog.ShowAsync();
        }
    }
}