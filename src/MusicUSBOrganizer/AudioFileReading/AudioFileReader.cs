using System.Collections.Generic;
using System.Linq;
using MusicUSBOrganizer.PlaylistReading;

namespace MusicUSBOrganizer.AudioFileReading;
internal static class AudioFileReader
{
	public static List<AudioFile> ReadAllAudioMetadata(List<Playlist> playlists)
	{
		return playlists
			.SelectMany(ReadAllAudioMetadata)
			.ToList();
	}

	public static List<AudioFile> ReadAllAudioMetadata(Playlist playlist)
	{
		var audioFiles = new List<AudioFile>();

		foreach(string filePath in playlist.FilePaths) {
			using var musicFile = TagLib.File.Create(filePath);
			audioFiles.Add(new AudioFile
			{
				Playlist = playlist,
				FilePathOriginal = filePath,
				Artist = musicFile.Tag.FirstPerformer,
				Year = musicFile.Tag.Year,
				DiscNumber = musicFile.Tag.Disc,
				Album = musicFile.Tag.Album,
				TrackNumber = musicFile.Tag.Track,
				Title = musicFile.Tag.Title,
				Bitrate = musicFile.Properties.AudioBitrate
			});
		}

		return audioFiles;
	}
}
