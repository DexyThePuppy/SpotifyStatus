# 🎵 ResoniteSpotifyStatus

🎶 Display Spotify status inside Resonite using WebSocket. Control playback (Spotify Premium only), either from just the owner or anyone.<br>
🖱️ Album, Title, and Artists are clickable with hyperlinks to open Spotify pages. Queue songs by pasting or dropping URIs onto the panel.<br>
🔊 Drop your Audio Stream for localized volume and broadcast / spatialize controls.<br>

## 🖥️ Server Setup

Make sure you have installed the [.NET 7.0 Runtime](https://dotnet.microsoft.com/download)!

1. 📦 Get the latest release [here](https://github.com/Banane9/SpotifyStatus/releases) and extract it.<br>
2. 🔑 Create a Spotify application on the [Spotify Developer Dashboard](https://developer.spotify.com/dashboard/applications).<br>
3. 🔗 Add `http://localhost:5000/callback` as a Redirect URI in your Spotify app settings.<br>
4. ⚙️ Put your Client ID and Client Secret into the `config.ini` file.<br>
5. 🚀 Run the program.<br>
6. 🔐 Sign in with your Spotify account and grant access.<br>

## 🛠️ Building and Running

1. 📂 Open a terminal or command prompt in the `SpotifyStatus.Standalone` directory.

2. 🏗️ Build the project:
   ```
   dotnet build
   ```

3. 🚀 Run the application:
   ```
   dotnet run
   ```

## 🌐 Resonite Setup

To use this in Resonite:

1. 📋 Copy this link: `resrec:///U-Banane9/R-88a9ae63-4861-42e5-a378-bed7468e0e50`<br>
2. 📥 Paste it into Resonite (Ctrl+V or use the Dash menu).<br>
3. 💾 Save the spawned item. (You might need to return to your inventory root)<br>
4. 📂 Open your inventory and find the saved item.<br>
5. 🔮 Spawn the item in your world.<br>
6. 🔌 Click "Connect to WebSocket" while the server is running.<br>
7. ⏳ Wait a moment for the connection.<br>
8. 🎚️ Optionally, drop your Audio Stream panel for volume and broadcast controls.<br>

## 📸 Previews 📸

| Unfocused | Focused | Theme Picker | Queue | Lyrics |
|---|---|---|---|---|
| <img src="https://raw.githubusercontent.com/DexyThePuppy/SpotifyStatus/refs/heads/master/Previews/2024-10-27%2001.42.29.webp" width="200"> | <img src="https://raw.githubusercontent.com/DexyThePuppy/SpotifyStatus/refs/heads/master/Previews/2024-10-27%2001.43.18.webp" width="200"> | <img src="https://raw.githubusercontent.com/DexyThePuppy/SpotifyStatus/refs/heads/master/Previews/2024-10-27%2002.09.22.webp" width="375"> |


## 🙏 Attributions

🎨 Icons from flaticon.com by Freepik, Pixel Perfect and Kirashastry.<br>
✏️ Modified for use as white on transparent masks and animations.<br>
