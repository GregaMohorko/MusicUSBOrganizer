using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MusicUSBOrganizer.AudioFileReading;

namespace MusicUSBOrganizer.FileNameDetermination;
internal static class FileNameDeterminator
{
	public static List<(AudioFile AudioFile, string NewFileName, string NewTitle)> DetermineFileNames(List<AudioFile> audioFiles)
	{
		var newFileNames = new List<(AudioFile, string, string)>();

		// 1st, by Artist
		var audioFilesByArtist = audioFiles
			.GroupBy(audioFile => audioFile.Artist.ToLowerInvariant())
			.ToList();
		var allArtists = audioFiles
			.Select(x => x.Artist)
			.Distinct()
			.ToList();
		foreach(var artistFilesGroup in audioFilesByArtist) {
			string artist = artistFilesGroup.First().Artist;
			var allArtistsExceptThisOne = allArtists
				.Where(x => x.Equals(artist, StringComparison.OrdinalIgnoreCase) == false)
				.ToList();
			string artistShort = GetShortestPossibleDistinctValue(artist, allArtistsExceptThisOne);

			// 2nd, by Year
			var filesByYear = artistFilesGroup
				.GroupBy(audioFile => audioFile.Year)
				.ToList();
			foreach(var yearFilesGroup in filesByYear) {
				uint year = yearFilesGroup.Key;

				// 3rd, by Disc number
				var filesByDiscNumber = yearFilesGroup
					.GroupBy(audioFile => audioFile.DiscNumber)
					.ToList();
				var allDiscNumbers = filesByDiscNumber
					.Select(g => g.Key)
					.ToList();
				foreach(var discNumberFilesGroup in filesByDiscNumber) {
					uint discNumber = discNumberFilesGroup.Key;
					var allDiscNumbersExceptThisOne = allDiscNumbers
						.Where(x => x != discNumberFilesGroup.Key)
						.ToList();

					// 4th, by Album
					var filesByAlbum = discNumberFilesGroup
						.GroupBy(audioFile => audioFile.Album?.ToLowerInvariant())
						.ToList();
					var allAlbums = discNumberFilesGroup
						.Select(x => x.Album)
						.Distinct()
						.ToList();
					foreach(var albumFilesGroup in filesByAlbum) {
						string album = albumFilesGroup.First().Album;
						var allAlbumsExceptThisOne = allAlbums
							.Where(x => x?.Equals(album, StringComparison.OrdinalIgnoreCase) != true)
							.ToList();
						string albumShort = album == null
							? null
							: GetShortestPossibleDistinctValue(album, allAlbumsExceptThisOne);

						// now determine file names
						var allTrackNumbers = albumFilesGroup
							.Select(x => x.TrackNumber)
							.Distinct()
							.ToList();
						foreach(var audioFile in albumFilesGroup) {
							string title = audioFile.Title;
							var allTitlesExceptThisOne = albumFilesGroup
								.Select(x => x.Title)
								.Where(x => x.Equals(title, StringComparison.OrdinalIgnoreCase) == false)
								.ToList();
							string titleShort = GetShortestPossibleDistinctValue(title, allTitlesExceptThisOne);

							(string newFileName, string newTitle) = GetFileName(audioFile, artistShort, albumShort, titleShort, allDiscNumbersExceptThisOne, allAlbumsExceptThisOne, allTrackNumbers, allTitlesExceptThisOne);
							newFileNames.Add((audioFile, newFileName, newTitle));
						}
					}
				}
			}
		}

		return newFileNames;
	}

	private static (string NewFileName, string NewTitle) GetFileName(
		AudioFile audioFile,
		string artistShort,
		string albumShort,
		string titleShort,
		List<uint> allDiscNumbersExceptThisOne,
		List<string> allAlbumsExceptThisOne,
		List<uint> allTrackNumbers,
		List<string> allTitlesExceptThisOne
		)
	{
		const string SEPARATOR = ";";

		// determine disc+album identifier
		string discAlbumIdentifier = null;
		if(allDiscNumbersExceptThisOne.Count > 0) {
			discAlbumIdentifier = audioFile.DiscNumber.ToString();
		}
		if(albumShort != null
			&& allAlbumsExceptThisOne.Count > 0
			) {
			if(discAlbumIdentifier != null) {
				discAlbumIdentifier += SEPARATOR;
			} else {
				discAlbumIdentifier = string.Empty;
			}
			discAlbumIdentifier += albumShort;
		}
		
		// determine track number identifier
		string trackNumberIdentifier = allTrackNumbers.Count < 2
			? null
			// set 02 instead of 2 if the max number is more than 9 etc. (so that the ordering is correct)
			: audioFile.TrackNumber.ToString(new string('0', (int)allTrackNumbers.Max() / 10));

		string titleIdentifier = (allTitlesExceptThisOne.Count == 0 || allTrackNumbers.Count == allTitlesExceptThisOne.Count + 1)
			? null
			: titleShort;

		// return the name
		string newFileName = $"{artistShort}{SEPARATOR}{audioFile.Year}{(discAlbumIdentifier == null ? null : $"{SEPARATOR}{discAlbumIdentifier}")}{(trackNumberIdentifier == null ? null : $"{SEPARATOR}{trackNumberIdentifier}")}{(titleIdentifier == null ? null : $"{SEPARATOR}{titleIdentifier}")}{SEPARATOR}{audioFile.Bitrate}kbps{SEPARATOR}{audioFile.Title}{Path.GetExtension(audioFile.FilePathOriginal)}";
		string newTitle = $"{artistShort}{SEPARATOR}{audioFile.Year}{(discAlbumIdentifier == null ? null : $"{SEPARATOR}{discAlbumIdentifier}")}{(trackNumberIdentifier == null ? null : $"{SEPARATOR}{trackNumberIdentifier}")}{(titleIdentifier == null ? null : $"{SEPARATOR}{titleIdentifier}")}{SEPARATOR}{audioFile.Title}";
		return (newFileName, newTitle);
	}

	/// <summary>
	/// For example, for "abc123" where siblings are "ab12" and "abc23", the result is "abc1".
	/// </summary>
	private static string GetShortestPossibleDistinctValue(string me, List<string> siblings)
	{
		string shortName = string.Empty;
		int i = -1;
		while(true) {
			++i;
			if(i != me.Length) {
				char character = me[i];
				shortName += character;
				if(char.IsLetter(character) == false) {
					continue;
				}
				if(siblings.Any(sibling => sibling.StartsWith(shortName, StringComparison.OrdinalIgnoreCase))) {
					continue;
				}
			}
			break;
		}
		return shortName;
	}
}
