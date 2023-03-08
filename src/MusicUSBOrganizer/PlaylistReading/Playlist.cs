using System.Collections.Generic;

namespace MusicUSBOrganizer.PlaylistReading;
internal class Playlist
{
	public string Name { get; set; }
	public List<string> FilePaths { get; set; }
}
