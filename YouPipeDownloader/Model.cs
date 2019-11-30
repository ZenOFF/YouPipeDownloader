using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace YouPipeDownloader
{
    internal class Model
    {
        private string TempFolder = Path.Combine(Environment.CurrentDirectory, @"TempFolder\");

        private int _countSongsInPlaylist = 10;

        public async void DownloadingSong(string IdSong)
        {
            AudioTrack audioTrack = new AudioTrack(IdSong);

            await audioTrack.GetSong();
        }

        public async Task<AudioTrackProperties> GetVideoInfo(string Id)
        {
            AudioTrack audioTrack = new AudioTrack(Id);
            AudioTrackProperties audioTrackProperties = new AudioTrackProperties();
            audioTrackProperties = await audioTrack.GetInfo();
            return audioTrackProperties;
        }

        public async Task<ObservableCollection<AudioTrackProperties>> GetPlaylist(string PlaylistId)
        {
            Playlist playlist = new Playlist(_countSongsInPlaylist, PlaylistId);

            return await playlist.GetIdVideosInPlayList();
        }
    }
}