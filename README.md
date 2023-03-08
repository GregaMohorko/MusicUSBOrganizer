# Music USB Organizer
A simple program that picks audio files from .m3u8 playlist files and copies them to the USB file in the respected folder structure.

It also renames the files in a single playlsit to a sortable manner:
- artist => year => disc number => album name => track number
- example file name for Metallica - Enter Sandman:
  - assuming there are no other artists that start with letter M:
    - "M-1991-1-958kbps-Enter Sandman"
  - assuming there is also Megadeth artist in the same playlist:
    - "Met-1991-1-958kbps-Enter Sandman"
	- Megadeth files will start with "Meg"

## Requirements
.NET Framework 4.8

## Author and License
Gregor Mohorko ([www.mohorko.info](https://www.mohorko.info))

Copyright (c) 2023 Gregor Mohorko
