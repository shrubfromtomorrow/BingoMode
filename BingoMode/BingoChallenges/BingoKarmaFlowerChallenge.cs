﻿using BingoMode.BingoSteamworks;
using Expedition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoKarmaFlowerChallenge : BingoChallenge
    {
        public int current;
        public SettingBox<int> amound;

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate("Consume [<current>/<amount>] Karma Flowers")
                .Replace("<current>", current.ToString())
                .Replace("<amount>", amound.Value.ToString());
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new(
                [[new Icon("foodSymbol"), new Icon("FlowerMarker", 1f, RainWorld.SaturatedGold)],
                [new Counter(current, amound.Value)]]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoKarmaFlowerChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Consuming karma flowers");
        }

        public override Challenge Generate()
        {
            BingoKarmaFlowerChallenge ch = new();
            ch.amound = new(UnityEngine.Random.Range(3, 8), "Amount", 0);
            return ch;
        }

        public void Karmad()
        {
            if (!completed && !revealed && !hidden && !TeamsCompleted[SteamTest.team])
            {
                current++;
                UpdateDescription();
                if (current >= amound.Value) CompleteChallenge();
                else ChangeValue();
            }
        }

        public override int Points()
        {
            return 20;
        }

        public override bool CombatRequired()
        {
            return false;
        }

        public override void Reset()
        {
            base.Reset();
            current = 0;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return slugcat != SlugcatStats.Name.Red;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoKarmaFlowerChallenge",
                "~",
                current.ToString(),
                "><",
                amound.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0",
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                current = int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                amound = SettingBoxFromString(array[1]) as SettingBox<int>;
                completed = (array[2] == "1");
                revealed = (array[3] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoKarmaFlowerChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.Player.ObjectEaten += Player_ObjectEatenKarmaFlower;
            IL.Room.Loaded += Room_LoadedKarmaFlower;
            On.Spear.HitSomethingWithoutStopping += Spear_HitSomethingWithoutStopping;
            placeKarmaFlowerHook = new(typeof(Player).GetProperty("PlaceKarmaFlower", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetGetMethod(), Player_PlaceKarmaFlower_get);

            IL.Player.FoodInRoom_Room_bool += Player_FoodInRoom_Room_bool;
        }

        public override void RemoveHooks()
        {
            On.Player.ObjectEaten -= Player_ObjectEatenKarmaFlower;
            IL.Room.Loaded -= Room_LoadedKarmaFlower;
            On.Spear.HitSomethingWithoutStopping -= Spear_HitSomethingWithoutStopping;
            placeKarmaFlowerHook?.Dispose();
            IL.Player.FoodInRoom_Room_bool -= Player_FoodInRoom_Room_bool;
        }

        public override List<object> Settings() => [amound];
    }
}
