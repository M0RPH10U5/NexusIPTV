# NexusIPTV



A desktop IPTV player for Windows built with **.NET 10, WPF, and VLC**.



NexusIPTV is designed to provide a straightforward desktop interface for IPTV playlists, channel browsing, electronic program guide information, and video playback without requiring a web browser.



## Features



### IPTV Playback



* VLC-powered video playback

* M3U playlist support

* Channel selection and switching

* Fullscreen playback

* Support for channels with and without logos

* Graceful fallback display for channels without a logo

* Playback-oriented channel selection



### Channel Browser



The channel browser provides:



* Channel names

* TVG IDs

* Channel logos

* Channel groups/categories

* Search filtering

* Group/category filtering

* Favorites filtering

* "No results" feedback when filters produce no matches



### Search



Channels can be searched using the search box.



Search matches channel information including:



* Channel name

* TVG ID



Search is case-insensitive.



### Group / Category Filtering



Channels can be filtered by their IPTV group/category.



The group selector includes:



* All Groups

* Favorites

* Individual IPTV groups discovered from the playlist



### Favorites



Channels can be marked as favorites.



Favorites:



* Are displayed with a star indicator

* Can be filtered using the **★ Favorites** category

* Persist between application launches

* Are stored locally in the user's application data



Favorite state is associated with the channel's TVG ID when available, with the stream URL used as a fallback.



### Channel Logos



Channel logos supplied by the IPTV playlist are displayed in the channel list.



When a channel does not provide a usable logo, NexusIPTV displays a simple `TV` fallback instead of leaving an empty space.



### Electronic Program Guide



NexusIPTV supports EPG information associated with the IPTV playlist.



EPG data is matched to channels using the available TVG/channel identifiers and is displayed alongside the channel information where available.



### Keyboard Navigation



The channel browser is designed to distinguish between **highlighting a channel** and **playing a channel**.


| Key       | Action                                             |
| --------- | -------------------------------------------------- |
| `↑`       | Highlight the previous channel                     |
| `↓`       | Highlight the next channel                         |
| `Enter`   | Play the highlighted channel                       |
| `Space`   | Toggle favorite status for the highlighted channel |



Arrow-key navigation does **not** immediately change playback.



This allows a user to move through the channel list and preview which channel is highlighted before pressing `Enter` to switch playback.



Mouse selection continues to play the selected channel normally.



## Project Structure



```text

NexusIPTV/
├── src/
│   ├── NexusIPTV.App/
│   │   ├── Views/
│   │   └── ...
│   │
│   ├── NexusIPTV.Core/
│   │   └── Models/
│   │
│   └── NexusIPTV.Vlc/
│
├── tests/
│   └── NexusIPTV.Tests/
│
├── NexusIPTV.sln
├── .gitignore
└── README.md

```



### NexusIPTV.Core



Contains the application's core models and IPTV-related logic.



This project is intentionally separated from the WPF user interface so that core functionality can be tested independently.



### NexusIPTV.Vlc



Contains the VLC integration used by the application for media playback.



### NexusIPTV.App



The WPF desktop application.



This contains the user interface, channel browser, search and filtering controls, favorites, fullscreen playback, keyboard navigation, and application-level playback interaction.



### NexusIPTV.Tests



Contains automated tests for the application's core functionality.



## Requirements



* Windows

* .NET 10 SDK

* VLC / compatible VLC runtime required by the VLC integration

* An IPTV M3U playlist

* XMLTV/EPG data when EPG functionality is desired



## Building



Clone the repository and open a terminal in the project directory:



```powershell

dotnet restore

dotnet build

```



To run the test suite:



```powershell

dotnet test

```



To run the WPF application:



```powershell

dotnet run --project .\\src\\NexusIPTV.App

```



## Configuration



NexusIPTV is currently intended to be configured around the IPTV playlist and EPG data supplied to the application.



Runtime user data such as favorites is stored locally and is not part of the Git repository.



## Current Status



NexusIPTV is an actively developed project.



The current implementation includes:



* IPTV channel loading

* VLC playback

* Fullscreen playback

* EPG support

* Channel search

* Group/category filtering

* Channel logos

* Logo fallback handling

* Persistent favorites

* Favorites filtering

* Keyboard channel navigation

* Enter-to-play behavior

* Space-to-favorite behavior



Playback-state robustness and additional playback/UI improvements are planned as subsequent development work.



## Development Philosophy



NexusIPTV is being developed incrementally, with functionality being tested and stabilized before moving on to larger features.



The project favors:



* Clear separation between core logic and UI

* Small, focused changes

* Testable application logic

* Native Windows desktop behavior

* Straightforward IPTV functionality

* Keyboard accessibility alongside mouse interaction



## License



Licensed under GNU GPL V3.

