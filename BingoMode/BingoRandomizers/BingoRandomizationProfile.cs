using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BingoMode.BingoChallenges;
using Expedition;
using UnityEngine;

namespace BingoMode.BingoRandomizer
{
    public class BingoRandomProfileException : Exception;

    public static class BingoRandomizationProfile
    {
        public static readonly string PROFILE_PATH = Application.persistentDataPath + Path.DirectorySeparatorChar.ToString() +
            "Bingo" + Path.DirectorySeparatorChar.ToString() +
            "RandomizerProfiles" + Path.DirectorySeparatorChar.ToString();
        public static readonly string FILE_EXTENSION = ".txt";

        private static bool _loaded;
        public static bool IsLoaded => _loaded;
        public static Dictionary<string, object> savedRandomizers = [];
        public static Randomizer<Challenge> profile;

        public static Challenge GetChallenge()
        {
            Challenge challenge = profile.Random();
            ExpeditionData.challengeList.Add(challenge);
            return challenge;
        }

        public static void LoadFromFile(string profileName)
        {
            string serialized = File.ReadAllText(PROFILE_PATH + profileName + FILE_EXTENSION);
            profile = Randomizer<Challenge>.InitDeserialize(serialized);
            _loaded = true;
        }

        public static void Unload() => _loaded = false;

        public static void SaveToFile(string profileName)
        {
            Directory.CreateDirectory(PROFILE_PATH);
            string serialized = profile.Serialize("").ToString();
            File.WriteAllText(PROFILE_PATH + profileName + FILE_EXTENSION, serialized);
            savedRandomizers = [];
        }

        public static void OpenSaveFolder()
        {
            Process.Start(PROFILE_PATH);
        }

        public static Randomizer<T> InstanceFromString<T>(string input)
        {
            switch (input)
            {
                case "ChallengeGroup": return new ChallengeGroup() as Randomizer<T>;
                case "Bool": return new BoolRandomizer() as Randomizer<T>;
                case "Range": return new RangeRandomizer() as Randomizer<T>;
                case "String": return new StringRandomizer() as Randomizer<T>;

                case "Achievement": return new BingoAchievementRandomizer() as Randomizer<T>;
                case "AllRegionsExcept": return new BingoAllRegionsExceptRandomizer() as Randomizer<T>;
                case "BombToll": return new BingoBombTollRandomizer() as Randomizer<T>;
                case "Broadcast": return new BingoBroadcastRandomizer() as Randomizer<T>;
                case "CollectPearl": return new BingoCollectPearlRandomizer() as Randomizer<T>;
                case "Craft": return new BingoCraftRandomizer() as Randomizer<T>;
                case "CreatureGate": return new BingoCreatureGateRandomizer() as Randomizer<T>;
                case "CycleScore": return new BingoCycleScoreRandomizer() as Randomizer<T>;
                case "Damage": return new BingoDamageRandomizer() as Randomizer<T>;
                case "Depths": return new BingoDepthsRandomizer() as Randomizer<T>;
                case "DodgeLeviathan": return new BingoDodgeLeviathanRandomizer() as Randomizer<T>;
                case "DodgeNoot": return new BingoDodgeNootRandomizer() as Randomizer<T>;
                case "DontKill": return new BingoDontKillChallenge() as Randomizer<T>;
                case "DontUseItem": return new BingoDontUseItemRandomizer() as Randomizer<T>;
                case "Eat": return new BingoEatRandomizer() as Randomizer<T>;
                case "Echo": return new BingoEchoRandomizer() as Randomizer<T>;
                case "EnterRegion": return new BingoEnterRegionRandomizer() as Randomizer<T>;
                case "EnterRegionFrom": return new BingoEnterRegionFromRandomizer() as Randomizer<T>;
                case "GlobalScoreChallenge": return new BingoGlobalScoreRandomizer() as Randomizer<T>;
                case "GourmandCrush": return new BingoGourmandCrushRandomizer() as Randomizer<T>;
                case "GreenNeuron": return new BingoGreenNeuronRandomizer() as Randomizer<T>;
                case "HatchNoodle": return new BingoHatchNoodleRandomizer() as Randomizer<T>;
                case "Hell": return new BingoHellRandomizer() as Randomizer<T>;
                case "ItemHoard": return new BingoItemHoardRandomizer() as Randomizer<T>;
                case "Iterator": return new BingoIteratorRandomizer() as Randomizer<T>;
                case "KarmaFlower": return new BingoKarmaFlowerRandomizer() as Randomizer<T>;
                case "Kill": return new BingoKillRandomizer() as Randomizer<T>;
                case "MaulTypes": return new BingoMaulTypesRandomizer() as Randomizer<T>;
                case "MaulX": return new BingoMaulXRandomizer() as Randomizer<T>;
                case "MoonCloak": return new BingoMoonCloakRandomizer() as Randomizer<T>;
                case "NeuronDelivery": return new BingoNeuronDeliveryRandomizer() as Randomizer<T>;
                case "NoNeedleTrading": return new BingoNoNeedleTradingRandomizer() as Randomizer<T>;
                case "NoRegion": return new BingoNoRegionRandomizer() as Randomizer<T>;
                case "PearlDelivery": return new BingoPearlDeliveryRandomizer() as Randomizer<T>;
                case "PearlHoard": return new BingoPearlHoardRandomizer() as Randomizer<T>;
                case "Pin": return new BingoPinRandomizer() as Randomizer<T>;
                case "Popcorn": return new BingoPopcornRandomizer() as Randomizer<T>;
                case "RivCell": return new BingoRivCellRandomizer() as Randomizer<T>;
                case "SaintDelivery": return new BingoSaintDeliveryRandomizer() as Randomizer<T>;
                case "SaintPopcorn": return new BingoSaintPopcornRandomizer() as Randomizer<T>;
                case "Steal": return new BingoStealRandomizer() as Randomizer<T>;
                case "Tame": return new BingoTameRandomizer() as Randomizer<T>;
                case "Trade": return new BingoTradeRandomizer() as Randomizer<T>;
                case "TradeTraded": return new BingoTradeTradedRandomizer() as Randomizer<T>;
                case "Transport": return new BingoTransportRandomizer() as Randomizer<T>;
                case "Unlock": return new BingoUnlockRandomizer() as Randomizer<T>;
                case "Vista": return new BingoVistaRandomizer() as Randomizer<T>;
                default:
                    Plugin.logger.LogError($"Error resolving type of {input}");
                    throw new BingoRandomProfileException();
            }
        }
    }


}
