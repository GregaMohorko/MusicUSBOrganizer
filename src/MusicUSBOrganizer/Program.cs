using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MusicUSBOrganizer.AudioFileReading;
using MusicUSBOrganizer.FileNameDetermination;
using MusicUSBOrganizer.PlaylistReading;

namespace MusicUSBOrganizer;
internal class Program
{
	[STAThread]
	static void Main()
	{
		try {
			DoWork();
		}catch(Exception ex) {
			Console.WriteLine(ex);
			Console.WriteLine();
		}

		Console.Write("Press any key to continue . . . ");
		_ = Console.ReadKey(true);
		Console.WriteLine();
	}

	static void DoWork()
	{
		// select playlist files
		Console.WriteLine("Selecting playlist files ...");
		var playlistFiles = SelectPlaylistFiles();
		if(playlistFiles == null || playlistFiles.Count == 0) {
			Console.WriteLine("\tNo playlists selected. Closing.");
			return;
		}
		Console.WriteLine("\tPlaylist files selected.");

		// select destination
		Console.WriteLine("Selecting destination ...");
		string destinationFolder = SelectDestinationFolder();
		if(destinationFolder == null) {
			Console.WriteLine("\tNo destination selected. Closing.");
			return;
		}
		var existingFilesInDestinationFolder = Directory.EnumerateFiles(destinationFolder, "*", SearchOption.AllDirectories)
			.Take(100)
			.ToList();
		if(existingFilesInDestinationFolder.Any()) {
			if(existingFilesInDestinationFolder.Any(existingFilePath => Path.GetFileName(Path.GetDirectoryName(existingFilePath)) != "System Volume Information")) {
				Console.WriteLine("\tDestination folder needs to be empty.");
				return;
			}
		}
		Console.WriteLine("\tDestination selected.");

		// read playlist files
		Console.WriteLine("Reading playlist files ...");
		var playlists = PlaylistReader.ReadAll(playlistFiles);
		Console.WriteLine("\tPlaylist files read.");

		// read metadata about files
		Console.WriteLine("Reading audio files metadata ...");
		var audioFiles = AudioFileReader.ReadAllAudioMetadata(playlists);
		Console.WriteLine("\tAudio files metadata read.");

		// determine new file names
		Console.WriteLine("Determining best new file names ...");
		var newFileNames = FileNameDeterminator.DetermineFileNames(audioFiles);
		Console.WriteLine("\tBest new file names determined.");

		// copy files
		Console.WriteLine("Copying files ...");
		var copiedFiles = CopyFiles(destinationFolder, newFileNames);
		Console.WriteLine("\tFiles copied.");

		// change title
		Console.WriteLine("Setting track titles ...");
		SetTrackTitles(copiedFiles);
		Console.WriteLine("\tTrack titles set.");

		Console.WriteLine();
		Console.WriteLine("Done.");
	}

	static List<string> SelectPlaylistFiles()
	{
		var dialog = new OpenFileDialog
		{
			Title = "Select playlists to include ...",
			Multiselect = true,
			Filter = "Playlist files (*.m3u, *.m3u8)|*.m3u;*.m3u8",
			FilterIndex = 0,
			CheckFileExists = true,
			CheckPathExists = true
		};
		if(dialog.ShowDialog() != DialogResult.OK) {
			return null;
		}
		return dialog.FileNames.ToList();
	}

	static string SelectDestinationFolder()
	{
		var dialog = new FolderBrowserDialog
		{
			Description = "Select destination folder where the audio files will be copied to ...",
			ShowNewFolderButton = true
		};
		if(dialog.ShowDialog() != DialogResult.OK) {
			return null;
		}
		return dialog.SelectedPath;
	}

	static List<(AudioFile AudioFile, string NewFilePath, string NewTitle)> CopyFiles(
		string destinationFolder,
		List<(AudioFile AudioFile, string NewFileName, string NewTitle)> filesToMove
		)
	{
		var copiedFiles = new List<(AudioFile AudioFile, string NewFilePath, string NewTitle)>();

		foreach(var fileToMove in filesToMove) {
			string folderPath = Path.Combine(destinationFolder, GetSafeFileName(fileToMove.AudioFile.Playlist.Name));
			string newFilePath = Path.Combine(folderPath, GetSafeFileName(fileToMove.NewFileName));
			if(newFilePath != fileToMove.AudioFile.FilePathOriginal) {
				if(File.Exists(newFilePath) == false) {
					if(Directory.Exists(folderPath) == false) {
						Directory.CreateDirectory(folderPath);
					}
					File.Copy(fileToMove.AudioFile.FilePathOriginal, newFilePath);
				}
			}
			copiedFiles.Add((fileToMove.AudioFile, newFilePath, fileToMove.NewTitle));
		}

		return copiedFiles;
	}

	static string GetSafeFileName(string name)
	{
		var stringBuilder = new StringBuilder(name);
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		foreach(char oldChar in invalidFileNameChars) {
			stringBuilder = stringBuilder.Replace(oldChar, '_');
		}

		return stringBuilder.ToString();
	}

	static void SetTrackTitles(List<(AudioFile AudioFile, string NewFilePath, string NewTitle)> copiedFiles)
	{
		foreach(var copiedFile in copiedFiles) {
			using var audioFileTag = TagLib.File.Create(copiedFile.NewFilePath);
			audioFileTag.Tag.Title = copiedFile.NewTitle;
			audioFileTag.Save();
		}
	}
}
