using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MusicUSBOrganizer.AudioFileReading;

namespace MusicUSBOrganizer.FileNameDetermination;
internal static class FileNameDeterminator
{
	public static List<(AudioFile AudioFile, string NewFileName)> DetermineFileNames(List<AudioFile> audioFiles)
	{
		var newFileNames = new List<(AudioFile, string)>();

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

							string newFileName = GetFileName(audioFile, artistShort, albumShort, titleShort, allDiscNumbersExceptThisOne, allAlbumsExceptThisOne, allTrackNumbers, allTitlesExceptThisOne);
							newFileNames.Add((audioFile, newFileName));
						}
					}
				}
			}
		}

		return newFileNames;
	}

	private static string GetFileName(
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
		// determine disc+album identifier
		string discAlbumIdentifier = null;
		if(allDiscNumbersExceptThisOne.Count > 0) {
			discAlbumIdentifier = audioFile.DiscNumber.ToString();
		}
		if(albumShort != null
			&& allAlbumsExceptThisOne.Count > 0
			) {
			if(discAlbumIdentifier != null) {
				discAlbumIdentifier += "-";
			} else {
				discAlbumIdentifier = string.Empty;
			}
			discAlbumIdentifier += albumShort;
		}
		
		// determine track number identifier
		string trackNumberIdentifier = allTrackNumbers.Count < 2 ? null : audioFile.TrackNumber.ToString();

		string titleIdentifier = (allTitlesExceptThisOne.Count == 0 || allTrackNumbers.Count == allTitlesExceptThisOne.Count + 1)
			? null
			: titleShort;

		// return the name
		return $"{artistShort}-{audioFile.Year}{(discAlbumIdentifier == null ? null : $"-{discAlbumIdentifier}")}{(trackNumberIdentifier == null ? null : $"-{trackNumberIdentifier}")}{(titleIdentifier == null ? null : $"-{titleIdentifier}")}-{audioFile.Bitrate}kbps-{audioFile.Title}{Path.GetExtension(audioFile.FilePathOriginal)}";
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
