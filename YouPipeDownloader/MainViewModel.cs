﻿using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Controls;

namespace YouPipeDownloader
{
    public class MainViewModel : BindableBase
    {
        private Model model = new Model();

        private string _inputUrl;

        public string InputUrl
        {
            get
            {
                return _inputUrl;
            }
            set
            {
                IdSong = GetIdFromUrl(TypeId.Video, value);
                RaisePropertyChanged(nameof(IdSong));

                IdPlaylist = GetIdFromUrl(TypeId.Playlist, value);
                RaisePropertyChanged(nameof(IdPlaylist));

                SetProperty(ref _inputUrl, value);
            }
        }

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

        public string _thumbnail;

        public string Thumbnail
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

        private ObservableCollection<AudioTrackProperties> _playlist;

        public ObservableCollection<AudioTrackProperties> Playlist
        {
            get
            {
                return _playlist;
            }

            set
            {
                _playlist = value;
            }
        }

        private AudioTrackProperties _itemPlaylistSelect;

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

        public string Title
        {
            get
            {
                return _title;
            }

            set
            {
                SetProperty(ref _title, value);
            }
        }

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

        public MainViewModel()
        {
        }

        public async void DownloadButton_Click()
        {
            if (String.IsNullOrEmpty(IdPlaylist))
            {
                return;
            }
            Playlist = await model.GetPlaylist(IdPlaylist);
            RaisePropertyChanged(nameof(Playlist));
        }

        public async void ItemPlaylistClick(object sender, ItemClickEventArgs e)
        {
           
            AudioTrackProperties audioTrackProperties = e.ClickedItem as AudioTrackProperties;

            Title = audioTrackProperties.Title;
            IdSong = audioTrackProperties.Id;

            audioTrackProperties = await model.GetVideoInfo(IdSong);
            Description = audioTrackProperties.Description;
            Duration = audioTrackProperties.Duration;
            Thumbnail = audioTrackProperties.Thumbnail;
        }

        //RegEx для форматирования юрл адреса и получения IdVideo и IdPlaylist
        private string GetIdFromUrl(TypeId typeId, string Url)
        {
            string ComleteUrl = Url + "&index";
            //https://www.youtube.com/watch?v=K61-tK7Xlzg&list=PLJ49GBcP7B3Yewnj-zKuYGABUU3sQpyMt&index=2&t=0s
            Regex rgx = new Regex($@"watch.v=(.+)&list=(.+)&index"); //поиск значений
            string Video = "";
            string Playlist = "";
            foreach (Match match in rgx.Matches(ComleteUrl))
            {
                Video = match.Groups[1].Value.ToString();//High
                Playlist = match.Groups[2].Value.ToString(); //Low
            }
            switch (typeId)
            {
                case TypeId.Video: return Video;
                case TypeId.Playlist: return Playlist;
                default: return null;
            }
        }
    }
}