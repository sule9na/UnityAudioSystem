# Unity Audio Management System

This system is designed to be a drop in AudioManager that will take care of all Sound Effect and Music needs in a single package.
All the scripts, components and prefabs you need are included in this demo project.

## Quick Start

If you just want to start using the system immediately then do the following:

### Set Up Your Scene

1. Add the AudioManager prefab to your main scene.
2. Check that the parameters are set up on the AudioManager correctly
    1. The sizes of the sound pool, max emitters and max sounds are set as you wish.
    2. The SoundEmitter prefab reference is valid.
	3. You have two Mixers, one called Music and one called Sounds.
	4. The mixer master groups have their volume exposed as a variable called MusicVolume and SoundVolume respectively.
    4. You have created a sound and music list (Right click in Project panel > AudioSystem > Music List or Sound List) and populated it with sounds you'd like to use.

### Populate Your Sound and Music Lists

To make your Scriptable Object data, go yo the folder where you'd like it stored.
Right click in Project panel > AudioSystem > Music List or Sound List

In each type of list you have an array that you can add more and more items to as you please.
Each array contains a specific set of data that is used for either a sound or a music track.
In the demo project you can see three sounds set up and two music tracks. You can replicate this format and change the parameters for each sound or music track to add as many as you want.

### Set Up Your Scene Scripts

In your scene you can now set up scripts to play sounds and music as you wish.
For example, set up buttons with listeners and call methods from the AudioManager instance to do various things.

You can access the sounds you've set up by pulling them from the Audio Manager as you need them.
It holds the references to each of the Music and Sound lists and you can access any specific sound or music track by name using the GetSound or GetMusicTrack method in each.

Some examples:
#### Sounds

To play a new sound:\
`AudioManager.Instance.Play(AudioManager.Instance.soundList.GetSound("OneShot"));`

To play a new random sound wrap multiple sounds in an array:\
`AudioManager.Instance.createSound().Play(["OneShot", "OneShot2"]);`

When playing a sound that is set to loop in the data, you'll need to manage it's lifetime manually in your code. So play it with a specific name by chaining the withName method as well:\
`SoundManager.Instance.createSound().WithName("AmbientSound").play(AudioManager.Instance.soundList.GetSound("Abience"));`

Then you can stop the sound by name whenever you need to:\
`AudioManager.Instance.StopSoundByName("AmbientSound");`

If You intend to play a lot of looping sounds of the same kind then you may need to iterate through names and keep and active list if you want to stop one loop specifically. If you give them all the same exact name then the stopSoundByName method will stop them all.

If you want to add sounds to UI buttons or toggles easily there's a SoundTrigger Component that you can just add to your button Node and specify a sound name in it.

#### Music

To play a music track:\
`AudioManager.Instance.PlayTrack(AudioManager.Instance.musicList.GetMusicTrack("Music1"));`
This will play the track use all the music settings in the data itself, i.e. fading in, or out, a specific volume. Etc.

The music play method also supports crossfading between tracks, this will override any fade time set in the music data and fade one track out and the other in over this period of time.
Simply pass in a time in seconds as a second arguement in the method call.
`AudioManager.Instance.PlayTrack(AudioManager.Instance.musicList.GetMusicTrack("Music2"), 2);`
This means both tracks will be playing for 2 seconds, while the old one fades down and the new one fades up.
<br/>
<br/>

# Architecture

The Audio System is made up of 4 core parts.

-   The Audio Manager, a persistent singleton (which initializes everything at first and handles moment to moment events and calls)
-   The Sound Builder (a new one is created as needed by the Sound Manager and sets up the next emitter to be used before being discarded)
-   The Sound Emitter (The sound pool is filled with emitter instances that only contain methods to manage their own usage of their AudioSource, the Sound Builder and Audio Manager control them.)
-   The Sound and Music Data classes (Which contain the structure of a MusicData and SoundData object, all sounds configured are stored these objects for usage.) And these are then children of the SoundList and MusicList classes which allow you to create Scriptable objects of those types to configure your sound data and reference in the Audio Manager

## Starting And Stopping Sounds

### One Off Sounds

When a sound is called to play with a `CreateSound().Play("SoundName")` call, the chained play call to the SoundBuilder will mean the sound is initialized and once ready it is told to play immediately.

The Sound Emitter then takes over and handles the lifetime of the sound. If a sound is not a looping sound then the Sound Emitter will stop the sound once it's duration has elapsed, reset all it's own data and put itself back into the pool.

### Looping Sounds

Looping sounds will play endlessly once told to play. This might be useful for ambience environmental soundsfor example.

Using the `WithName()` call chained into your `CreateSound()` method call you can force the sound builder to rename the emitter when it initializes it.

This then means you can use the `StopSoundByName` method in the Audio Manager to get that specific emitter from the list and tell it to stop. At which point it will clean itself up and return to the pool.

## Starting And Stopping Music

The music AudioSources are two child gameObjects of the Audio Manager and it will intelligently switch from one to the next when switching tracks to allow cross fading.

To start playing a music track simply call the `PlayTrack("musicName")` method in the SoundManager and pass it the name of a track from your data.

You can also add a second overload arguement which is a time to crossfade float in seconds. This will start fading one AudioSource up while the other is still playing but fading down.
By default (if you don't overload the PlayTrack call with a float as well the behaviour is to fade one track out based on it's fade out time in data and only then fade the other up in the same way, the crossfade time will override this.

Music is not pooled and doesn't have any lifetime management. If a track is set not to loop in data the AudioSource will simply stop playing once it ends.
If it is set to loop then you can switch it to a new track with a new PlayMusic call, as described above.

If you wish to stop all music for a time then simply call the `StopAllMusic()` method on the Audio Manager.
<br/>
<br/>

# Controlling Sound Volume
There are two methods in the AudioManager for controlling the set volume.
Ideally you should save the player's preferred volume for Music and Sounds in their player data and reload it when you reload their game.
There are TODOs in the AudioManager `Start()` method to indicate where you should be setting the player's audio levels from their data but this demo project doesn't include player persistence, so it's not implemented, just earmarked for implementation.

To control the music volume you pass a float between 0 and 1 into the `SetMusicVolume()` method of the AudioManager, like so:
`AudioManager.Instance.SetMusicVolume(0.7f);`

This can be hooked up to a slider, as in the demo, or if you prefer a toggle then just pass in a float of 0 or 1 based on your toggle bool, to turn the music on or off instead.

The Sound volume is managed in exactly the same way, like so:
`AudioManager.Instance.SetSoundVolume(0.7f);`

# NOTES
The SoundData includes manual configuration for each sound to set what mixer group it will use. This means two things:
1. You can have certain sounds assigned to a different mixer than the main sound mixer that has it's volume exposed and those sounds will not be controlled by the mixer.
2. You could easily create a UI mixer for example and duplicate the Set Volume methods to control that with a separate slider if you want granular volume control between gameplay sounds and UI sounds.