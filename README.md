# Rain World Bingo
For the mod description/how it works, see https://steamcommunity.com/sharedfiles/filedetails/?id=3441764924

## Adding onto the mod
This mod is open source! Feel free to clone the code, make forks, pull requests, whatever else github has!

I encourage any person interested in growing this mod to contribute! Whether that'd be code improvements, new content, or any other cool stuff, I'd be more than happy to add your stuff to this mod!

If you're interested, I'd recommend starting with adding a new challenge, you can research Expedition challenges in vanilla game, and `BingoChallenge` classes in this repository. There's also a section in the Wiki for developers, good to read :)

We'd love to see your part in this mess! Oh right...

## This mod is a little bit of a mess

This mod was Nacu's first large programming project and since I have begun development, it's been my largest as well. All across the board there is jank and, dare I say, charm. Optimizations will always be welcome, though the mod is quite stable as of right now. Do not be afraid though! I got into it, so can you.

## How to build
### Make sure your c# language version is set to `13.0` in your .csproj file (for the release configuration). The code uses some of the modern c# version features in a lot of places so this is necessary.
Run the setup.bat file as administrator to setup up your build environment from your local repo entirely.

That does sound sketchy so feel free to check out the source yourself, here's what it does though:

Creates a lib folder for dependencies.<br>
Creates a bingo mod folder in your Rain World installation (by default `C:\Program Files (x86)\Steam\steamapps\common\Rain World\RainWorld_Data\StreamingAssets\mods\`, change this is in the batch file yourself to make your life easier.)<br>
Creates a symlink between that mod folder and your build from git (this means all you need to do is change what's in the repo directory to have it update in game.)<br>
Copies all necessary dependencies into the created lib folder, these are:

- HOOKS-Assembly-CSharp.dll
- Assembly-CSharp-firstpass.dll
- com.rlabrecque.steamworks.net.dll
- Rewired_Core.dll
- Unity.Mathematics.dll
- UnityEngine.AudioModule.dll
- UnityEngine.CoreModule.dll
- UnityEngine.dll
- UnityEngine.InputLegacyModule.dll
- UnityEngine.UnityWebRequestWWWModule.dll
- BepInEx.dll
- Mono.Cecil.dll
- MonoMod.dll
- MonoMod.RuntimeDetour.dll
- MonoMod.Utils.dll

Runs the included nstrip binary to strip and publicize Assembly-CSharp.dll while putting it in the lib folder.

This saves like, a lot of time so I would recommend using it lol.
