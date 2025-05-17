[![Release]][Link]
<!----------------------------------------------------------------------------->
[Link]: https://github.com/Antoniofo/AudioPlayer/releases
<!---------------------------------[ Buttons ]--------------------------------->
[Release]: https://img.shields.io/badge/Release-EFFDE?style=for-the-badge&logoColor=white&logo=DocuSign


# Installation:

You will need [AudioPlayerApi](https://github.com/Killers0992/AudioPlayerApi?tab=readme-ov-file#installation) along with this plugin to work.

You can install this plugin, download the [.dll](https://github.com/Antoniofo/AudioPlayer/releases) file and placing it in ``%AppData%\Roaming\EXILED\Plugins`` (Windows) or ``~/.config/EXILED/Plugins`` (Linux)

Optional: You will need some audio files in the `audio` folder inside the Exiled `Config` folder named `mtf.ogg` and `chaos.ogg` for each respawn, if you fancy some custom respawn audios.


# How to use ?

Open your Remote Admin Console and write:

To play a sound (NO EXTENSION): 

``audio play file``

To list audio files available:

``audio list``

To stop global audio only:

``audio stop``

To stop all audio:

``audio stop true``

To play audio from URL(WITH EXTENSION):

``audio play https://myawesomewebserver.net/files/audio.ogg``

To play audio at certain coordinate:

``audio atplace <X> <Y> <Z> <duration> file``

``audio atplace 19 200 20 3 file``

Usage: audio|audioplayer play/list/stop/atplace [[x] [y] [z] [distance]] [[filename/URL]|[true/false]]

With the `play` and `atplace` you can use either a file from the server filesystem or an URL.

Note: the audio files need to meet the requirement for the AudioPlayerApi:
- .ogg format
- mono
- SampleRate of 48000

# Permission

The permission you can give in the permission file:

`- AudioPlayer`
