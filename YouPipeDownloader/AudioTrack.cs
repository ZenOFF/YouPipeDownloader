using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoLibrary;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace YouPipeDownloader
{
    internal class AudioTrack
    {
        private string _idSong;

        public delegate void updDel();

        public AudioTrack(string IdSong)
        {
            _idSong = IdSong;
        }

        public async Task<dynamic> GetSong(string UrlYouTube = "https://www.youtube.com/watch?v=")
        {
            var youtube = YouTube.Default;

            //загрука видео в кэш
            var vid = await youtube.GetVideoAsync(String.Concat(UrlYouTube, _idSong));
            // onUpdateEnd();

            //сохранение видео в _tempFolder
            //using (FileStream SourceStream = File.Open(_tempFolder + vid.FullName, FileMode.OpenOrCreate))
            //{
            //    SourceStream.Seek(0, SeekOrigin.End);
            //    byte[] bt = await vid.GetBytesAsync();

            //    await SourceStream.WriteAsync(bt, 0, bt.Length);
            //}
            //// onUpdateEnd();

            //var inputFile = new MediaFile { Filename = _tempFolder + vid.FullName };
            //var outputFile = new MediaFile { Filename = _saveFolder + $"{vid.FullName}.mp3" };

            //using (var engine = new Engine())
            //{
            //    await Task.Run(() =>
            //    {
            //        engine.GetMetadata(inputFile);
            //        engine.Convert(inputFile, outputFile);
            //        Console.WriteLine("Конвертирование завершено");
            //    });
            //}

            //if (File.Exists(outputFile.Filename))
            //{
            //    await Task.Run(() =>
            //    {
            //        File.Delete(inputFile.Filename);
            //        Console.WriteLine("Временный файл удалён");
            //    });
            //}
            //// onUpdateEnd();
            return default(dynamic);
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

        //private async void SelectSourceBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

        //    openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
        //    foreach (var audioTypeItem in ViewModel.AudioTypeList)
        //    {
        //        openPicker.FileTypeFilter.Add($".{audioTypeItem}");
        //    }

        //    ViewModel.SourceFile = await openPicker.PickSingleFileAsync();
        //}

        //private async Task SelectTargetFile()
        //{
        //    try
        //    {
        //        var savePicker = new Windows.Storage.Pickers.FileSavePicker();

        //        savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
        //        savePicker.DefaultFileExtension = $".{ViewModel.SelectedAudioType}".ToLower();
        //        savePicker.SuggestedFileName = $"{ViewModel.SourceFile.DisplayName}";
        //        savePicker.FileTypeChoices.Add(ViewModel.SelectedAudioType.ToString(), new string[] { $".{ViewModel.SelectedAudioType.ToString().ToLower()}" });

        //        ViewModel.TargetFile = await savePicker.PickSaveFileAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        Alert($"An error had been throwed, {ex.Message}");
        //    }
        //}

        //private MediaEncodingProfile DetermineMediaEncodingProfile()
        //{
        //    MediaEncodingProfile profile = null;

        //    switch (ViewModel.SelectedAudioType)
        //    {
        //        case AudioFormat.MP4:
        //        case AudioFormat.AAC:
        //        case AudioFormat.M4A:
        //            profile = MediaEncodingProfile.CreateM4a(ViewModel.SelectedQuality);
        //            break;
        //        case AudioFormat.MP3:
        //            profile = MediaEncodingProfile.CreateMp3(ViewModel.SelectedQuality);
        //            break;
        //        case AudioFormat.WMA:
        //            profile = MediaEncodingProfile.CreateWma(ViewModel.SelectedQuality);
        //            break;
        //    }

        //    return profile;
        //}

        //private async Task StartConvertAudioFile()
        //{
        //    ViewModel.ProcessBarVisable = Visibility.Visible;
        //    ViewModel.ConvertLog = $"Start Convert{Environment.NewLine}";

        //    try
        //    {
        //        //determine MediaEncodingProfiel
        //        MediaEncodingProfile profile = DetermineMediaEncodingProfile();

        //        //init convert workder object
        //        MediaTranscoder transcoder = new MediaTranscoder();
        //        PrepareTranscodeResult prepareOp = await transcoder.PrepareFileTranscodeAsync(ViewModel.SourceFile, ViewModel.TargetFile, profile);
        //        if (prepareOp.CanTranscode)
        //        {
        //            //start convert
        //            var transcodeOp = prepareOp.TranscodeAsync();

        //            //registers progress event handler
        //            transcodeOp.Progress += async (IAsyncActionWithProgress<double> asyncInfo, double percent) =>
        //            {
        //                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        //                {
        //                    ViewModel.RateOfProgress = percent;
        //                    ViewModel.ConvertLog += $"Progressed {(int)percent}%{Environment.NewLine}";
        //                });
        //            };

        //            //registers completed event handler
        //            transcodeOp.Completed += async (IAsyncActionWithProgress<double> asyncInfo, AsyncStatus status) =>
        //            {
        //                asyncInfo.GetResults();
        //                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        //                {
        //                    ViewModel.ProcessBarVisable = Visibility.Collapsed;
        //                    ViewModel.ConvertLog += $"convert has been {status}";
        //                    if (status == AsyncStatus.Error)
        //                    {
        //                        Alert($"Convert failed, error code is {asyncInfo.ErrorCode}");
        //                    }
        //                });
        //            };
        //        }
        //        else
        //        {
        //            Alert($"Can not convert this file from {ViewModel.SourceFile.FileType} to {ViewModel.TargetFile.FileType}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Alert($"An error had been throwed, {ex.Message}");
        //    }
        //}
    }
}