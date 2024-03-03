using System;
using Windows.UI.Xaml.Media.Imaging;

namespace YouPipeDownloader
{
    public class AudioTrackProperties
    {
        public string Description { get; set; }
        public string Duration { get; set; }
        public string Id { get; set; }
        public BitmapImage Thumbnail { get; set; }
        public string Title { get; set; }
    }
}