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
        private static bool _loaded;
        public static bool IsLoaded => _loaded;
        public static readonly string PROFILE_PATH = Application.persistentDataPath + Path.DirectorySeparatorChar.ToString() +
            "Bingo" + Path.DirectorySeparatorChar.ToString() +
            "RandomizerProfiles" + Path.DirectorySeparatorChar.ToString();
        public static readonly string FILE_EXTENSION = ".txt";
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

                default:
                    Plugin.logger.LogError($"Error resolving type of {input}");
                    throw new BingoRandomProfileException();
            }
        }
    }


}
