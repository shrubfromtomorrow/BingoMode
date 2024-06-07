using BepInEx;
using System.Security;
using System.Security.Permissions;

#pragma warning disable CS0618
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace BingoMode
{
    using BingoSteamworks;
    using Challenges;

    [BepInPlugin("nacu.bingomode", "Expedition Bingo", "0.9")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool AppliedAlreadyDontDoItAgainPlease;
        internal static BepInEx.Logging.ManualLogSource logger;

        public void OnEnable()
        {
            logger = Logger;
            On.RainWorld.OnModsInit += OnModsInit;
            BingoHooks.EarlyApply();
            On.RainWorld.Update += RainWorld_Update;
        }

        // Receiving data from yuh (networking)
        public void RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
        {
            orig.Invoke(self);

            //uint size;
            //while (SteamNetworking.IsP2PPacketAvailable(out size))
            //{
            //    Plugin.logger.LogMessage("PACKET AVAILABLE");
            //    byte[] data = new byte[size];
            //
            //    if (SteamNetworking.ReadP2PPacket(data, size, out uint bytesRead, out CSteamID remoteID))
            //    {
            //        // Converting the byte array to a string
            //        char[] chars = new char[bytesRead / sizeof(char)];
            //        Buffer.BlockCopy(data, 0, chars, 0, data.Length);
            //        string message = new string(chars, 0, chars.Length);
            //        Plugin.logger.LogMessage("RECEIVED MESSAGE FROM " + remoteID + " READING " + message);
            //        InnerWorkings.MessageReceived(message);
            //    }
            //}
            //
            //if (SteamTest.messagesToConfirm != null && SteamTest.messagesToConfirm.Keys != null && SteamTest.messagesToConfirm.Keys.Count > 0)
            //{
            //    if (SteamTest.repeatMessageCounter++ > 2000)
            //    {
            //        foreach (var kvp in SteamTest.messagesToConfirm)
            //        {
            //
            //        }
            //    }
            //}
            //else SteamTest.repeatMessageCounter = 0;

            SteamFinal.ReceiveMessagesUpdate();
        }

        public void OnDisable()
        {
            logger = null;
        }

        public static void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld raingame)
        {
            orig(raingame);

            if (!AppliedAlreadyDontDoItAgainPlease)
            {
                AppliedAlreadyDontDoItAgainPlease = true;

                SteamTest.Apply();

                Futile.atlasManager.LoadAtlas("Atlases/bingomode");
                Futile.atlasManager.LoadAtlas("Atlases/bingoicons");
                BingoEnums.Register();
                
                BingoHooks.Apply();
                ChallengeHooks.Apply();
                ChallengeUtils.Apply();
            }
        }
    }
}