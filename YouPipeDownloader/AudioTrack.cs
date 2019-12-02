using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoLibrary;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace YouPipeDownloader
{
    internal class AudioTrack
    {
        private string _idSong;
        private string _title;

        public delegate void updDel();

        public AudioTrack(string IdSong)
        {
            _idSong = IdSong;
        }

        public AudioTrack(string IdSong, string Title) : this(IdSong)
        {
            _title = Title;
        }

        public async Task SaveAudioTrack(string UrlYouTube = "https://www.youtube.com/watch?v=")
        {
            var youtube = YouTube.Default;
            //загрука видео в кэш
            var vid = await youtube.GetVideoAsync(String.Concat(UrlYouTube, _idSong));
            byte[] videoByte = await vid.GetBytesAsync();
            //создание временного файла
            StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
            StorageFile videoFile = await tempFolder.CreateFileAsync(_idSong, CreationCollisionOption.ReplaceExisting);
            //сохранение
            await FileIO.WriteBytesAsync(videoFile, videoByte);

            await StartConvertAudioFile(videoFile);

            MessageDialog messageDialog = new MessageDialog("Converting is done");
            await messageDialog.ShowAsync();
        }

        public async Task<AudioTrackProperties> GetInfo(string UrlYouTube = "https://www.youtube.com/watch?v=")
        {
            //создаём экземляр класса СвойстАудиоДорожки
            AudioTrackProperties audioTrackProperties = new AudioTrackProperties();
            //выполняем запрос к API и получаем длительность аудио дорожки
            var videoInfoString = await RequestAudioDurationAsync();
            audioTrackProperties.Duration = GetVideoDuration(videoInfoString);
            //выполняем запрос к API и получаем название и описание аудио дорожки
            videoInfoString = await RequestAudioInfoAsync();
            audioTrackProperties.Description = GetVideoDescription(videoInfoString);

            audioTrackProperties.Thumbnail = new BitmapImage(new Uri(GetVideoThumbnails(videoInfoString)));
            return audioTrackProperties;
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

            var result = await new HttpClient().GetStringAsync(new Uri(fullUrl));

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
                ["fields"] = "items/snippet(description),items/snippet/thumbnails/default(url)",
            };

            string baseUrl = "https://www.googleapis.com/youtube/v3/videos?";
            string fullUrl = MakeUrlWithQuery(baseUrl, parameters);

            var result = await new HttpClient().GetStringAsync(new Uri(fullUrl));

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
        }
    }
}