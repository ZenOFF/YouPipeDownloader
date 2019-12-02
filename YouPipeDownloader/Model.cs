using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace YouPipeDownloader
{
    internal class Model
    {
        private int _countSongsInPlaylist = 50;

        public async Task DownloadingSong(string IdSong, string Title)
        {
            AudioTrack audioTrack = new AudioTrack(IdSong, Title);
            await audioTrack.SaveAudioTrack();
        }

        //получение информации(описание, продолжительность, обложка) о видео по ID
        public async Task<AudioTrackProperties> GetVideoInfo(string Id)
        {
            AudioTrack audioTrack = new AudioTrack(Id);
            AudioTrackProperties audioTrackProperties = new AudioTrackProperties();
            audioTrackProperties = await audioTrack.GetInfo();
            return audioTrackProperties;
        }

        //получение списка Playlist
        public async Task<ObservableCollection<AudioTrackProperties>> GetPlaylist(string PlaylistId)
        {
            Playlist playlist = new Playlist(_countSongsInPlaylist, PlaylistId);

            return await playlist.GetIdVideosInPlayList();
        }
    }
}