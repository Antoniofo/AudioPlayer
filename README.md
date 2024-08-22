[![Release]][Link]
<!----------------------------------------------------------------------------->
[Link]: https://github.com/Antoniofo/AudioPlayer/releases
<!---------------------------------[ Buttons ]--------------------------------->
[Release]: https://img.shields.io/badge/Release-EFFDE?style=for-the-badge&logoColor=white&logo=DocuSign


# Installation:

You will need the [SCPSLAudioAPI.dll](https://github.com/CedModV2/SCPSLAudioApi/releases) along with this plugin to work. Place it in the same folder as the AudioPlayer.dll

This plugin has some dependecies you will find a zip archive named dependencies.zip inside the [release](https://github.com/Antoniofo/AudioPlayer/releases) you will have to put those two files in the `dependencies` folder indide the `Plugins` folder

You can install this plugin, download the [.dll](https://github.com/Antoniofo/AudioPlayer/releases) file and placing it in ``%AppData%\Roaming\EXILED\Plugins`` (Windows) or ``~/.config/EXILED/Plugins`` (Linux)

Optional: You will need some audio files in the `audio` folder inside the Exiled `Config` folder named mtf.ogg and chaos.ogg for each respawn.


# How to use ?

Open your Remote Admin Console and write:

To play a sound: 

``audio play file.ogg DisplayName``

To list audio files available:

``audio list``

To stop audio:

``audio stop 1``

To play audio from URL:

``audio play https://myawesomewebserver.net/files/audio.ogg DisplayName``

To play audio at certain coordinate:

``audio atplace <X> <Y> <Z> <duration> file.ogg DisplayName``

``audio atplace 19 200 20 3 file.ogg DisplayName``

Usage: audio|audioplayer play/list/stop/atplace [[x] [y] [z] [distance]] [[filename/URL]|[true/false]] [displayName]

With the `play` and `atplace` you can use either a file from the server filesystem or an URL.

Note: the audio files need to meet the requirement for the SCPSLAudioApi:
- .ogg format
- mono
- SampleRate of 48000

# Permission

The permission you can give in the permission file:

`- AudioPlayer`
