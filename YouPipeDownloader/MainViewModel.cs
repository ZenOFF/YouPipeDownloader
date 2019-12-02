using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace YouPipeDownloader
{
    public class MainViewModel : BindableBase
    {
        private Model model = new Model();

        //ЮРЛ адрес вида https://www.youtube.com/watch?v=K61-tK7Xlzg&list=PLJ49GBcP7B3Yewnj-zKuYGABUU3sQpyMt&index=2&t=0s
        private string _inputUrl;

        public string InputUrl
        {
            get
            {
                return _inputUrl;
            }
            set
            {
                IdSong = GetVideoIdFromUrl(value);
                RaisePropertyChanged(nameof(IdSong));

                IdPlaylist = GetPlaylistIdFromUrl(value);
                RaisePropertyChanged(nameof(IdPlaylist));

                SetProperty(ref _inputUrl, value);
            }
        }

        //идентификатор видео "K61-tK7Xlzg"
        private string _idSong;

        public string IdSong
        {
            get
            {
                return _idSong;
            }
            set
            {
                _idSong = value;
                RaisePropertyChanged(nameof(IdSong));
            }
        }

        //идентификатор плэйлиста "PLJ49GBcP7B3Yewnj-zKuYGABUU3sQpyMt"
        private string _idPlaylist;

        public string IdPlaylist
        {
            get
            {
                return _idPlaylist;
            }

            set
            {
                _idPlaylist = value;
                RaisePropertyChanged(nameof(IdPlaylist));
            }
        }

        //Обложка видео
        public BitmapImage _thumbnail;

        public BitmapImage Thumbnail
        {
            get
            {
                return _thumbnail;
            }

            set
            {
                SetProperty(ref _thumbnail, value);
            }
        }

        //список Playlist
        private ObservableCollection<AudioTrackProperties> _playlist;

        public ObservableCollection<AudioTrackProperties> Playlist
        {
            get
            {
                return _playlist;
            }

            set
            {
                SetProperty(ref _playlist, value);
            }
        }

        private AudioTrackProperties _itemPlaylistSelect;

        //выбранный элемент ListView
        public AudioTrackProperties ItemPlaylistSelect
        {
            get
            {
                return _itemPlaylistSelect;
            }
            set
            {
                SetProperty(ref _itemPlaylistSelect, value);
                RaisePropertyChanged(nameof(Title));
            }
        }

        private string _title;

        //Заголовок
        public string Title
        {
            get
            {
                if (!String.IsNullOrEmpty(_title))
                {
                    VisibilityDownloadButton = Visibility.Visible;
                }
                else
                {
                    VisibilityDownloadButton = Visibility.Collapsed;
                }
                return _title;
            }

            set
            {
                SetProperty(ref _title, value);
            }
        }

        //Описание
        private string _description;

        public string Description
        {
            get
            {
                return _description;
            }

            set
            {
                SetProperty(ref _description, value);
            }
        }

        //Продолжительность
        private string _duration;

        public string Duration
        {
            get
            {
                return _duration;
            }

            set
            {
                SetProperty(ref _duration, value);
            }
        }

        private Visibility _visibilityDownloadButton = Visibility.Visible;

        public Visibility VisibilityDownloadButton
        {
            get
            {
                return _visibilityDownloadButton;
            }
            set
            {
                SetProperty(ref _visibilityDownloadButton, value);
            }
        }

        public MainViewModel()
        {
        }

        //кнопка получения информации по Id
        public async void DownloadButton_Click()
        {
            if (!String.IsNullOrEmpty(IdPlaylist))
            {
                Playlist = await model.GetPlaylist(IdPlaylist);
            }
        }

        //нажатие по выбраному элементу ListView и заполнение свойст Title,IdSong,Description,Duration,Thumbnail
        public async void ItemPlaylistClick(object sender, ItemClickEventArgs e)
        {
            AudioTrackProperties audioTrackProperties = e.ClickedItem as AudioTrackProperties;

            Title = audioTrackProperties.Title;
            IdSong = audioTrackProperties.Id;

            audioTrackProperties = await model.GetVideoInfo(IdSong);
            Description = audioTrackProperties.Description;
            Duration = audioTrackProperties.Duration;
            Thumbnail = audioTrackProperties.Thumbnail;
            RaisePropertyChanged(nameof(Thumbnail));
        }

        //RegEx для форматирования юрл адреса и получения IdVideo и IdPlaylist
        private string GetVideoIdFromUrl(string Url)
        {
            Regex IndexSearch = new Regex($@"&index"); //поиск значений

            if (!IndexSearch.IsMatch(Url))
            {
                Url = Url + "&list";
            }

            Regex rgx = new Regex($@"watch.v=(.+)&list"); //поиск значений
            string Id = "";

            foreach (Match match in rgx.Matches(Url))
            {
                Id = match.Groups[1].Value.ToString();
            }
            return Id;
        }

        private string GetPlaylistIdFromUrl(string Url)
        {
            Regex IndexSearch = new Regex($@"&index"); //поиск значений

            if (!IndexSearch.IsMatch(Url))
            {
                Url = Url + "&index";
            }

            Regex rgx = new Regex($@"list=(.+)&index"); //поиск значений
            string Id = "";

            foreach (Match match in rgx.Matches(Url))
            {
                Id = match.Groups[1].Value.ToString();
            }
            return Id;
        }
    }
}