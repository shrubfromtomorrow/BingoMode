﻿using BingoMode.BingoRandomizer;
using BingoMode.BingoSteamworks;
using Expedition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;

    public class BingoHellRandomizer : ChallengeRandomizer
    {
        public Randomizer<int> amount;

        public override Challenge Random()
        {
            BingoHellChallenge challenge = new();
            challenge.amount.Value = amount.Random();
            return challenge;
        }

        public override StringBuilder Serialize(string indent)
        {
            string surindent = indent + INDENT_INCREMENT;
            StringBuilder serializedContent = new();
            serializedContent.AppendLine($"{surindent}amount-{amount.Serialize(surindent)}");
            return base.Serialize(indent).Replace("__Type__", "Hell").Replace("__Content__", serializedContent.ToString());
        }

        public override void Deserialize(string serialized)
        {
            Dictionary<string, string> dict = ToDict(serialized);
            amount = Randomizer<int>.InitDeserialize(dict["amount"]);
        }
    }

    public class BingoHellChallenge : BingoChallenge
    {
        public int current;
        public SettingBox<int> amount;

        public BingoHellChallenge()
        {
            amount = new(0, "Amount", 0);
        }

        public override bool RequireSave() => true;
        public override bool SaveOnDeath() => true;

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate("Complete [<current>/<amount>] bingo challenges in a row without dying")
                .Replace("<current>", current.ToString())
                .Replace("<amount>", amount.Value.ToString());
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase() => new(
            [[new Icon("completechallenge"), new Counter(current, amount.Value)],
            [new Icon("buttonCrossA", 1f, Color.red), new Icon("Multiplayer_Death")]]);

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoHellChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Avoiding death while completing challenges");
        }

        public override Challenge Generate()
        {
            BingoHellChallenge ch = new();
            ch.amount = new(UnityEngine.Random.Range(2, 5), "Amount", 0);
            return ch;
        }

        public override void Update()
        {
            base.Update();
            if (TeamsFailed[SteamTest.team] || completed) return;
            if ((BingoHooks.GlobalBoard.completableChallenges + current < amount.Value))
            {
                Plugin.logger.LogInfo("Failing");
                FailChallenge(SteamTest.team);
            }
        }

        public void GetChallenge(BingoChallenge chal)
        {
            if (!completed && !revealed && !hidden && !TeamsCompleted[SteamTest.team] && !TeamsFailed[SteamTest.team])
            {
                if (current >= amount.Value)
                {
                    CompleteChallenge();
                }
                else
                {
                    current++;
                    UpdateDescription();
                    if (current >= amount.Value)
                    {
                        CompleteChallenge();
                    }
                    else ChangeValue();
                }
            }
        }

        public void SessionEnded(int revealedChallenges)
        {
            if (!completed && !revealed && !hidden && !TeamsCompleted[SteamTest.team] && !TeamsFailed[SteamTest.team])
            {
                if (current + revealedChallenges >= amount.Value)
                {
                    current = amount.Value;
                    UpdateDescription();
                    CompleteChallenge();
                }
            }
        }

        public void Die()
        {
            if (TeamsFailed[SteamTest.team] || TeamsCompleted[SteamTest.team] || completed || current >= amount.Value) return;

            //Plugin.logger.LogInfo("Die");

            //if (current != 0)
            //{
            //    current = 0;
            //    UpdateDescription();
            //    ChangeValue();
            //}
            UpdateHellChallengeOnDeath();
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
            return true;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoHellChallenge",
                "~",
                current.ToString(),
                "><",
                amount.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0"
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                current = int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                amount = SettingBoxFromString(array[1]) as SettingBox<int>;
                completed = (array[2] == "1");
                revealed = (array[3] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoHellChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.Player.Die += Player_DieHell;
            On.RainWorldGame.GoToDeathScreen += RainWorldGame_GoToDeathScreenHell;
            On.RainWorldGame.GoToStarveScreen += RainWorldGame_GoToStarveScreenHell;
            On.SaveState.SessionEnded += DantesInferno;
        }

        public override void RemoveHooks()
        {
            On.Player.Die -= Player_DieHell;
            On.RainWorldGame.GoToDeathScreen -= RainWorldGame_GoToDeathScreenHell;
            On.RainWorldGame.GoToStarveScreen -= RainWorldGame_GoToStarveScreenHell;
            On.SaveState.SessionEnded -= DantesInferno;
        }

        public override List<object> Settings() => [amount];
    }
}
