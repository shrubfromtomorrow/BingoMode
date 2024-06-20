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

            SteamFinal.ReceiveMessagesUpdate(self);
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