using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BingoMode.BingoChallenges;
using Expedition;
using UnityEngine;

namespace BingoMode.BingoRandomizer
{
    public class BingoRandomProfileException : Exception;

    public static class BingoRandomizationProfile
    {
        private const int ATTEMPT_CEILING = 100;
        public static readonly string PROFILE_PATH = Application.persistentDataPath + Path.DirectorySeparatorChar.ToString() +
            "Bingo" + Path.DirectorySeparatorChar.ToString() +
            "RandomizerProfiles" + Path.DirectorySeparatorChar.ToString();
        public static readonly string FILE_EXTENSION = ".txt";

        private static bool _loaded;
        public static bool IsLoaded => _loaded;
        public static Dictionary<string, object> savedRandomizers = [];
        public static Randomizer<Challenge> profile;

        /// <summary>
        /// Generate and return a random challenge from the currently loaded profile.
        /// </summary>
        /// <returns>A random <class>Challenge</class> generated from the loaded profile.</returns>
        /// <exception cref="BingoRandomProfileException"></exception>
        public static Challenge GetChallenge()
        {
            Challenge challenge = default;
            int attempt = 0;
            while (challenge == default(Challenge) && attempt < ATTEMPT_CEILING)
            {
                try
                { challenge = profile.Random(); }
                catch (FailedRandomChallengeException)
                { attempt++; }
            }
            if (attempt >= ATTEMPT_CEILING)
            {
                Plugin.logger.LogError("Failed to generate a challenge after too many attempts");
                throw new BingoRandomProfileException();
            }
            ExpeditionData.challengeList.Add(challenge);
            return challenge;
        }

        /// <summary>
        /// Reset the state of all randomizers. Call this before generating a new board.
        /// </summary>
        public static void Reset()
        {
            Weighted<bool>.Reset();
            Weighted<int>.Reset();
            Weighted<string>.Reset();
            Weighted<Randomizer<Challenge>>.Reset();
        }

        /// <summary>
        /// Get all the relative paths to files in the user's RandomizerProfiles directory and subdirectories.
        /// All values in the returned list should be good to use with <c>LoadFromFile(string)</c>.
        /// </summary>
        /// <param name="path">full path to subdirectory for recursive call. Default is <c>persistentDataPath/Bingo/RandomizerProfiles</c>.</param>
        /// <returns>A list containing all available profiles in the user's RandomizerProfiles directory.</returns>
        public static List<string> GetAvailableProfiles(string path = null)
        {
            Directory.CreateDirectory(PROFILE_PATH);

            if (path == null)
                path = PROFILE_PATH;
            List<string> profiles = [];

            foreach (string directory in Directory.EnumerateDirectories(path))
                profiles.AddRange(GetAvailableProfiles(directory));
            foreach (string profile in Directory.EnumerateFiles(path))
            {
                string[] splitPath = profile.Split(Path.DirectorySeparatorChar);
                int relativeStartIndex = Array.IndexOf(splitPath, "RandomizerProfiles") + 1;
                string relativePath = Path.Combine([.. splitPath.Skip(relativeStartIndex)]).Replace(FILE_EXTENSION, "");
                profiles.Add(relativePath);
            }
            return profiles;
        }

        /// <summary>
        /// Load a profile from the user's persistent data directory.
        /// </summary>
        /// <param name="profileName">The name of the file containing the profile to load, without path or file extension.</param>
        public static void LoadFromFile(string profileName)
        {
            Randomizer<bool>.ResetNamed();
            Randomizer<int>.ResetNamed();
            Randomizer<string>.ResetNamed();
            Randomizer<Challenge>.ResetNamed();
            string serialized = File.ReadAllText(PROFILE_PATH + profileName + FILE_EXTENSION);
            profile = Randomizer<Challenge>.InitDeserialize(serialized);
            _loaded = true;
        }

        /// <summary>
        /// Clear reference to the current profile, leaving the garbage collector free to clear it.
        /// </summary>
        public static void Unload()
        {
            Randomizer<bool>.ResetNamed();
            Randomizer<int>.ResetNamed();
            Randomizer<string>.ResetNamed();
            Randomizer<Challenge>.ResetNamed();
            profile = null;
            _loaded = false;
        }

        /// <summary>
        /// Save the currently loaded profile to a file in the user's persistent data directory.
        /// If the file already exists, overwrite it.
        /// </summary>
        /// <param name="profileName"></param>
        public static void SaveToFile(string profileName)
        {
            string fullPath = PROFILE_PATH + profileName + FILE_EXTENSION;
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string serialized = profile.Serialize("").ToString();
            savedRandomizers.Clear();
            File.WriteAllText(fullPath, serialized);
        }

        /// <summary>
        /// Open the folder containing randomization profiles in the file explorer.
        /// </summary>
        public static void OpenSaveFolder()
        {
            Directory.CreateDirectory(PROFILE_PATH);
            Process.Start(PROFILE_PATH);
        }

        /// <summary>
        /// Create and return a new instance of <c>Randomizer&lt;<typeparamref name="T"/>&gt;</c>.
        /// </summary>
        /// <typeparam name="T">The data type generated by the randomizer. e.g.:int, Challenge.</typeparam>
        /// <param name="input">The serialized name of the randomizer to be returned.</param>
        /// <returns>A new uninitialized randomizer.</returns>
        /// <exception cref="BingoRandomProfileException"></exception>
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
                case "Damage": return new BingoDamageRandomizer() as Randomizer<T>;
                case "Depths": return new BingoDepthsRandomizer() as Randomizer<T>;
                case "DodgeLeviathan": return new BingoDodgeLeviathanRandomizer() as Randomizer<T>;
                case "DodgeNoot": return new BingoDodgeNootRandomizer() as Randomizer<T>;
                case "DontKill": return new BingoDontKillRandomizer() as Randomizer<T>;
                case "DontUseItem": return new BingoDontUseItemRandomizer() as Randomizer<T>;
                case "Eat": return new BingoEatRandomizer() as Randomizer<T>;
                case "Echo": return new BingoEchoRandomizer() as Randomizer<T>;
                case "EnterRegion": return new BingoEnterRegionRandomizer() as Randomizer<T>;
                case "EnterRegionFrom": return new BingoEnterRegionFromRandomizer() as Randomizer<T>;
                case "GlobalScore": return new BingoScoreRandomizer() as Randomizer<T>;
                case "GourmandCrush": return new BingoGourmandCrushRandomizer() as Randomizer<T>;
                case "GreenNeuron": return new BingoGreenNeuronRandomizer() as Randomizer<T>;
                case "HatchNoodle": return new BingoHatchNoodleRandomizer() as Randomizer<T>;
                case "Hell": return new BingoHellRandomizer() as Randomizer<T>;
                case "ItemHoard": return new BingoItemHoardRandomizer() as Randomizer<T>;
                case "Iterator": return new BingoIteratorRandomizer() as Randomizer<T>;
                case "KarmaFlower": return new BingoKarmaFlowerRandomizer() as Randomizer<T>;
                case "Kill": return new BingoKillRandomizer() as Randomizer<T>;
                case "Lick": return new BingoLickRandomizer() as Randomizer<T>;
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
