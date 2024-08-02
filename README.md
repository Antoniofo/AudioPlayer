[![Release]][Link]
<!----------------------------------------------------------------------------->
[Link]: https://github.com/Antoniofo/AudioPlayer/releases
<!---------------------------------[ Buttons ]--------------------------------->
[Release]: https://img.shields.io/badge/Release-EFFDE?style=for-the-badge&logoColor=white&logo=DocuSign
![GitHub all releases](https://img.shields.io/github/downloads/Antoniofo/AudioPlayer/total)


# Installation:

You will need the [SCPSLAudioAPI.dll](https://github.com/CedModV2/SCPSLAudioApi/releases) along with this plugin to work. Place it in the same folder as the AudioPlayer.dll

This plugin has some dependecies you will find a zip archive named dependencies.zip inside the [release](https://github.com/Antoniofo/AudioPlayer/releases) you will have to put those two files in the `dependencies` folder indide the `Plugins` folder

You can install this plugin, download the [.dll](https://github.com/Antoniofo/AudioPlayer/releases) file and placing it in ``%AppData%\Roaming\EXILED\Plugins`` (Windows) or ``~/.config/EXILED/Plugins`` (Linux)

Optional: You will need some audio files in the `audio` folder inside the Exiled `Config` folder named mtf.ogg and chaos.ogg for each respawn.


# How to use ?

Open your Remote Admin Console and write:

``audio`` 

Usage: audio|audioplayer play/playurl/list/stop [filename]/[URL] [displayName]

Note: the audio files need to meet the requirement for the SCPSLAudioApi:
- .ogg format
- mono
- SampleRate of 48000

# Permission

The permission you can give in the permission file:

`- AudioPlayer`

# Stats
![Alt](https://repobeats.axiom.co/api/embed/b7e2b9be38f9202b150d87f243dd9aaff51407e9.svg "Repobeats analytics image")
