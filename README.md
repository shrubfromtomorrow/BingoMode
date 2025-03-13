# Bingo Mode
For the mod description/how it works, see https://steamcommunity.com/sharedfiles/filedetails/?id=3441764924

## Adding onto the mod
This mod is open source! Feel free to clone the code, make forks, pull requests, whatever else github has!!

I encourage any person interested in growing this mod to contribute! Whether that'd be code improvements, new content, or any other cool stuff, I'd be more than happy to add your stuff to this mod!

If you're interested, I'd recommend starting with adding a new challenge, you can research Expedition challenges in vanilla game, and `BingoChallenge` classes in this repository.

I personally won't be adding anything else to the mod, I'll only fix major bugs and make it work for the new versions, I might not even be the owner of this repo later! Who knows.
Either way I want to give anyone interested an option to expand or improve this mod, cause that's cool i think :)) And there are always many ideas left on the cutting room floor that someone might pick up.
When you do work on a build, make sure to change your VERSION in `Plugin.cs` to be unique to avoid stuff breaking when playing multiplayer.

I'd love to see your part in this mess! Oh right...

## This mod is a little bit of a mess
It's true! This was my very first large project like this, so a lot of the code is structured pretty messy, especially the menu stuff... However do not be discouraged, it's all not too complicated, 
just that I would've done a loot of things very differently if I was making this again. I won't though, since I have been developing the mod for a bit over a year(with breaks) and im so done!.

This mod allowed me to grow as a developer and programmer, which I appreciate greatly... its just that the mod's turned out way more messy than i wanted because of the learning haha.

## How to build
### Make sure your c# language version is set to `13.0` in your .csproj file (for the release configuration). The code uses some of the modern c# version features in a lot of places so this is necessary.
### Change your post build event destination to your bingo mod plugins folder
### Place the required dependencies in the lib folder, these include:
- Assembly-CSharp-nstrip.dll: A stripped and publicized (important to use the `-p` flag when running the nstrip command) version of the Assembly-CSharp found in `Rain World\RainWorld_Data\Managed` using NStrip (https://github.com/bbepis/NStrip). This was necessary due to some menu issues (PUBLIC-Assembly-CSharp doesn't work as far as i know)
- HOOKS-Assembly-CSharp.dll from `Rain World\BepInEx\plugins`

Found in `Rain World\RainWorld_Data\Managed`:
- Assembly-CSharp-firstpass.dll
- com.rlabrecque.steamworks.net.dll
- Rewired_Core.dll
- Unity.Mathematics.dll
- UnityEngine.AudioModule.dll
- UnityEngine.CoreModule.dll
- UnityEngine.dll
- UnityEngine.InputLegacyModule.dll
- UnityEngine.UnityWebRequestWWWModule.dll

Found in `Rain World\BepInEx\core`:
- BepInEx
- Mono.Cecil.dll
- MonoMod.dll
- MonoMod.RuntimeDetour.dll
- MonoMod.Utils.dll
