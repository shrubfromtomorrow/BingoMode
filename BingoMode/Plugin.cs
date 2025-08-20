﻿using BepInEx;
using BepInEx.Logging;
using System.Security;
using System.Security.Permissions;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;

#pragma warning disable CS0618
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace BingoMode
{
    using BingoSteamworks;
    using BingoChallenges;
    using BingoHUD;
    using System.IO;

    [BepInPlugin("nacu.bingomodebeta", "Bingo Beta", VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string VERSION = "1.25";
        public static bool AppliedAlreadyDontDoItAgainPlease;
        public static bool AppliedAlreadyDontDoItAgainPleasePartTwo;
        internal static ManualLogSource logger;
        private BingoModOptions _bingoConfig;
        public BingoModOptions BingoConfig => _bingoConfig;
        public static Plugin PluginInstance;

        public void OnEnable()
        {
            PluginInstance = this;
            _bingoConfig = new BingoModOptions(this);
            //new Hook(typeof(LogEventArgs).GetMethod("ToString", BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.InvokeMethod), AddTimeToLog);
            logger = Logger;
            On.RainWorld.OnModsInit += OnModsInit;
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;
            BingoHooks.EarlyApply();
            BingoSaveFile.Apply();
        }

        //public static string AddTimeToLog(Func<LogEventArgs, string> orig, LogEventArgs self)
        //{
        //    return "[" + DateTime.Now.Hour + ":" + (DateTime.Now.Minute < 10 ? "0" : "") + DateTime.Now.Minute + ":" + (DateTime.Now.Second < 10 ? "0" : "") + DateTime.Now.Second + "]" + orig.Invoke(self);
        //}

        public void OnDisable()
        {
            logger = null;
        }

        public void Update()
        {
            if (_bingoConfig == null || _bingoConfig.UseMapInput.Value) return;
            if (Input.anyKeyDown && (Input.GetKeyDown(_bingoConfig.HUDKeybindKeyboard.Value) || 
                                    Input.GetKeyDown(_bingoConfig.HUDKeybindC1.Value) ||
                                    Input.GetKeyDown(_bingoConfig.HUDKeybindC2.Value) ||
                                    Input.GetKeyDown(_bingoConfig.HUDKeybindC3.Value) ||
                                    Input.GetKeyDown(_bingoConfig.HUDKeybindC4.Value)))
            {
                BingoHUDMain.Toggled = !BingoHUDMain.Toggled;
            }
        }

        public static void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld raingame)
        { 
            orig(raingame);

            if (!AppliedAlreadyDontDoItAgainPlease)
            {
                AppliedAlreadyDontDoItAgainPlease = true;

                if (!File.Exists(AssetManager.ResolveFilePath("decals" + Path.DirectorySeparatorChar + "the_original.png")))
                {
                    logger.LogFatal("These modders are PISSING me off...");
                    return;
                }

                SteamTest.Apply();

                Futile.atlasManager.LoadAtlas("Atlases/bingomode");
                Futile.atlasManager.LoadAtlas("Atlases/bingoicons");
                BingoEnums.Register();
                
                BingoHooks.Apply();
                ChallengeHooks.Apply();
                ChallengeUtils.Apply();

                // Timeline fix
                IL.MainLoopProcess.RawUpdate += MainLoopProcess_RawUpdate;

                MachineConnector.SetRegisteredOI("nacu.bingomodebeta", PluginInstance.BingoConfig);
            }
        }

        private static void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig.Invoke(self);

            if (!AppliedAlreadyDontDoItAgainPleasePartTwo)
            {
                AppliedAlreadyDontDoItAgainPleasePartTwo = true;
                BingoData.LoadAllBannedChallengeLists();
            }
        }

        public static void MainLoopProcess_RawUpdate(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<MainLoopProcess>("Update")
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<MainLoopProcess>>((process) =>
                {
                    if (process is RainWorldGame or Menu.Menu)
                    {
                        SteamFinal.ReceiveMessagesUpdate();
                    }
                });
            }
            else logger.LogError("MainLoopProcess_RawUpdate IL fail " + il);
        }
    }
}
