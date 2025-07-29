using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using MoreSlugcats;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoBombTollChallenge : BingoChallenge
    {
        public Dictionary<string, bool> bombed = [];
        public SettingBox<bool> pass;
        public SettingBox<string> roomName;
        public SettingBox<bool> specific;
        public SettingBox<int> amount;
        public int current;

        public override void UpdateDescription()
        {
            string action = specific.Value ? "a grenade" : "grenades";
            string tollPart;
            string passPart = pass.Value ? (specific.Value ? " and then pass it" : " and then pass them") : "";

            if (specific.Value)
            {
                string region = roomName.Value.Substring(0, 2);
                string regionName = Region.GetRegionFullName(region, ExpeditionData.slugcatPlayer);

                if (roomName.Value == "gw_c05")
                {
                    regionName += " surface";
                }
                else if (roomName.Value == "gw_c11")
                {
                    regionName += " underground";
                }

                tollPart = "the <toll> toll".Replace("<toll>", regionName);
            }
            else
            {
                tollPart = "[<current>/<amount>] unique tolls".Replace("<current>", ValueConverter.ConvertToString(current)).Replace("<amount>", ValueConverter.ConvertToString(amount.Value));
            }

            description = "Throw <action> at <toll><pass>".Replace("<action>", action).Replace("<toll>", tollPart).Replace("<pass>", passPart);
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = new(
                [[Icon.FromEntityName("ScavengerBomb"), Icon.SCAV_TOLL],
                [specific.Value ? new Verse(roomName.Value.ToUpperInvariant()) : new Counter(current, amount.Value)]]);
            if (pass.Value) phrase.InsertWord(new Icon("singlearrow"));
            return phrase;
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoBombTollChallenge c;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Throwing grenades at scavenger tolls");
        }

        public override Challenge Generate()
        {
            string toll = ChallengeUtils.BombableOutposts[UnityEngine.Random.Range(0, ChallengeUtils.BombableOutposts.Length - (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Saint ? 0 : 1))];

            return new BingoBombTollChallenge
            {
                specific = new(UnityEngine.Random.value < 0.5f, "Specific toll", 0),
                amount = new(UnityEngine.Random.Range(1, 4), "Amount", 1),
                pass = new(UnityEngine.Random.value < 0.5f, "Pass the Toll", 2),
                roomName = new(toll, "Scavenger Toll", 3, listName: "tolls")
            };
        }

        public void Boom(string room)
        {
            if (!completed && !revealed && !TeamsCompleted[SteamTest.team] && !hidden)
            {
                if (!pass.Value)
                {
                    if (specific.Value)
                    {
                        if (roomName.Value == room.ToLowerInvariant())
                        {
                            bombed[room] = false;
                            CompleteChallenge();
                            return;
                        }
                    }
                    else
                    {
                        if (!bombed.ContainsKey(room))
                        {
                            current++;
                            UpdateDescription();
                            if (current >= amount.Value)
                            {
                                CompleteChallenge();
                            }
                            else
                            {
                                ChangeValue();
                            }
                        }
                    }
                }
                if (!bombed.ContainsKey(room))
                {
                    bombed[room] = false;
                }
            }
        }

        public void Pass(string room)
        {
            if (!completed && !revealed && !hidden && !TeamsCompleted[SteamTest.team] && bombed.ContainsKey(room) && !bombed[room] && pass.Value)
            {
                if (specific.Value)
                {
                    if (roomName.Value == room.ToLowerInvariant())
                    {
                        bombed[room] = true;
                        CompleteChallenge();
                    }
                }
                else
                {
                    bombed[room] = true;
                    current++;
                    UpdateDescription();
                    if (current >= amount.Value)
                    {
                        CompleteChallenge();
                    }
                    else
                    {
                        ChangeValue();
                    }
                }
            }
        }

        public override void Reset()
        {
            base.Reset();
            bombed?.Clear();
            bombed = [];
        }

        public override int Points()
        {
            return 20;
        }

        public override bool CombatRequired()
        {
            return true;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return true;
        }

        public string BombedTollsToString()
        {
            List<string> joinLater = [];

            foreach (var kvp in bombed)
            {
                joinLater.Add(kvp.Key.ToString() + "|" + string.Join("|", kvp.Value));
            }

            if (joinLater.Count == 0) return "empty";

            return string.Join("%", joinLater);
        }

        public Dictionary<string, bool> BombedTollsFromString(string input)
        {
            Dictionary<string, bool> result = new();

            if (input == "empty") return result;

            string[] entries = input.Split('%');

            foreach (string entry in entries)
            {
                string[] parts = entry.Split('|');

                result[parts[0]] = bool.Parse(parts[1]);
            }

            return result;
        }

        public override string ToString()
        {
            return string.Concat(
            [
                "BingoBombTollChallenge",
                "~",
                specific.ToString(),
                "><",
                roomName.ToString(),
                "><",
                pass.ToString(),
                "><",
                current.ToString(),
                "><",
                amount.ToString(),
                "><",
                BombedTollsToString(),
                "><",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0"
            ]);
        }



        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                if (array.Length == 8)
                {
                    specific = SettingBoxFromString(array[0]) as SettingBox<bool>;
                    roomName = SettingBoxFromString(array[1]) as SettingBox<string>;
                    pass = SettingBoxFromString(array[2]) as SettingBox<bool>;
                    current = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
                    amount = SettingBoxFromString(array[4]) as SettingBox<int>;
                    bombed = BombedTollsFromString(array[5]);
                    completed = (array[6] == "1");
                    revealed = (array[7] == "1");
                }
                // Legacy board bomb toll challenge compatibility
                else
                {
                    roomName = SettingBoxFromString(array[0]) as SettingBox<string>;
                    pass = SettingBoxFromString(array[1]) as SettingBox<bool>;
                    completed = (array[2] == "1");
                    revealed = (array[3] == "1");
                    specific = SettingBoxFromString("System.Boolean|true|Specific toll|0|NULL") as SettingBox<bool>;
                    current = 0;
                    amount = SettingBoxFromString("System.Int32|2|Amount|1|NULL") as SettingBox<int>;
                    bombed = [];
                }
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoBombTollChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.ScavengerBomb.Explode += ScavengerBomb_Explode;
            On.ScavengerOutpost.PlayerTracker.Update += PlayerTracker_Update2;
        }

        public override void RemoveHooks()
        {
            On.ScavengerBomb.Explode -= ScavengerBomb_Explode;
            On.ScavengerOutpost.PlayerTracker.Update -= PlayerTracker_Update2;
        }

        public override List<object> Settings() => [specific, pass, amount, roomName];
    }
}