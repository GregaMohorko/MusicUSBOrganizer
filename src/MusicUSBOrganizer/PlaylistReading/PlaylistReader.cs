using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MusicUSBOrganizer.PlaylistReading;
internal static class PlaylistReader
{
	public static List<Playlist> ReadAll(List<string> playlistFiles)
	{
		var playlists = new List<Playlist>();

		foreach(string playlistFile in playlistFiles) {
			var playlist = new Playlist
			{
				Name = Path.GetFileNameWithoutExtension(playlistFile),
				FilePaths = new List<string>()
			};

			using(var fileStream = File.OpenText(playlistFile)) {
				string readLine;
				while((readLine = fileStream.ReadLine()) != null) {
					string cleanedLine = RemoveComment(readLine).Trim();
					if(string.IsNullOrWhiteSpace(cleanedLine)) {
						continue;
					}
					playlist.FilePaths.Add(cleanedLine);
				}
			}

			playlists.Add(playlist);
		}

		return playlists;
	}

	private static string RemoveComment(string line)
	{
		int indexOfCommentStart = line.IndexOf('#');
		if(indexOfCommentStart == -1) {
			return line;
		}
		return line.Substring(0, indexOfCommentStart);
	}
}
