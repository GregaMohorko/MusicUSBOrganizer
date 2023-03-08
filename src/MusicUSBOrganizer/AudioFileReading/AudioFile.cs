using MusicUSBOrganizer.PlaylistReading;

namespace MusicUSBOrganizer.AudioFileReading;
internal class AudioFile
{
	public Playlist Playlist { get; set; }
	public string FilePathOriginal { get; set; }
	public string Artist { get; set; }
	public string Title { get; set; }
	public uint Year { get; set; }
	public uint DiscNumber { get; set; }
	public string Album { get; set; }
	public uint TrackNumber { get; set; }
	public int Bitrate { get; set; }
}
