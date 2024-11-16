using BepInEx;
using BepInEx.Logging;
using System.Security;
using System.Security.Permissions;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using MonoMod.Cil;
using Mono.Cecil.Cil;

#pragma warning disable CS0618
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace BingoMode
{
    using BingoSteamworks;
    using Challenges;
    using System.IO;
    using UnityEngine;

    [BepInPlugin("nacu.bingomode", "Bingo", VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string VERSION = "0.72";
        public static bool AppliedAlreadyDontDoItAgainPlease;
        internal static ManualLogSource logger;
        public static BingoModOptions bingoConfig;

        public void OnEnable()
        {
            Directory.CreateDirectory(Application.persistentDataPath + Path.DirectorySeparatorChar.ToString() + "Bingo");
            new Hook(typeof(LogEventArgs).GetMethod("ToString", BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.InvokeMethod), AddTimeToLog);
            logger = Logger;
            bingoConfig = new();
            On.RainWorld.OnModsInit += OnModsInit;
            BingoHooks.EarlyApply();
            BingoSaveFile.Apply();
        }

        public static string AddTimeToLog(Func<LogEventArgs, string> orig, LogEventArgs self)
        {
            return "[" + DateTime.Now.Hour + ":" + (DateTime.Now.Minute < 10 ? "0" : "") + DateTime.Now.Minute + ":" + (DateTime.Now.Second < 10 ? "0" : "") + DateTime.Now.Second + "]" + orig.Invoke(self);
        }

        public void OnDisable()
        {
            logger = null;
        }

        public void Update()
        {
            if (Input.anyKeyDown && (Input.GetKeyDown(bingoConfig.HUDKeybindKeyboard.Value) || 
                                    Input.GetKeyDown(bingoConfig.HUDKeybindC1.Value) ||
                                    Input.GetKeyDown(bingoConfig.HUDKeybindC2.Value) ||
                                    Input.GetKeyDown(bingoConfig.HUDKeybindC3.Value) ||
                                    Input.GetKeyDown(bingoConfig.HUDKeybindC4.Value)))
            {
                BingoHUD.Toggled = !BingoHUD.Toggled;
            }
        }

        public static void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld raingame)
        {
            orig(raingame);

            if (!AppliedAlreadyDontDoItAgainPlease)
            {
                AppliedAlreadyDontDoItAgainPlease = true;

                MachineConnector.SetRegisteredOI("nacu.bingomode", bingoConfig);

                SteamTest.Apply();

                Futile.atlasManager.LoadAtlas("Atlases/bingomode");
                Futile.atlasManager.LoadAtlas("Atlases/bingoicons");
                BingoEnums.Register();
                
                BingoHooks.Apply();
                ChallengeHooks.Apply();
                ChallengeUtils.Apply();

                // Timeline fix
                IL.MainLoopProcess.RawUpdate += MainLoopProcess_RawUpdate;
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