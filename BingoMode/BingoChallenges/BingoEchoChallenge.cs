using BingoMode.BingoSteamworks;
using Expedition;
using MoreSlugcats;
using RWCustom;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Menu.Remix;
using System.Globalization;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    // Literally copied from base game, to add the starving thing easily, and to customize which echoes appear
    public class BingoEchoChallenge : BingoChallenge
    {
        public SettingBox<string> ghost; //GhostWorldPresence.GhostID
        public SettingBox<bool> starve;
        public SettingBox<bool> specific;
        public int current;
        public SettingBox<int> amount;
        public List<string> visited = [];

        public override void UpdateDescription()
        {
            this.description = specific.Value ? 
                ChallengeTools.IGT.Translate("Visit the <echo_location> Echo" + (starve.Value ? " while starving" : ""))
                .Replace("<echo_location>", ChallengeTools.IGT.Translate(Region.GetRegionFullName(ghost.Value, ExpeditionData.slugcatPlayer)))
                :
                ChallengeTools.IGT.Translate("Visit the <amount> Echos" + (starve.Value ? " while starving" : ""))
                .Replace("<amount>", specific.Value ? "1" : ValueConverter.ConvertToString(amount.Value));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = new Phrase([new Icon("echo_icon", 1f, Color.white), specific.Value ? new Verse(ghost.Value) : new Counter(current, amount.Value)], []);
            if (starve.Value)
            {
                phrase.words.Add(new Icon("Multiplayer_Death", 1f, Color.white));
                phrase.newLines = [2];
            }
            return phrase;
        }

        public void SeeGhost(string spectre)
        {
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden) return;
            if (specific.Value)
            {
                if (spectre != ghost.Value) return;
                UpdateDescription();
                CompleteChallenge();
            }
            else
            {
                if (visited.Contains(spectre)) return;
                current++;
                visited.Add(spectre);
                UpdateDescription();
                if (current >= amount.Value) CompleteChallenge();
                else ChangeValue();
            }
        }

        public override void Update()
        {
            base.Update();
            if (Custom.rainWorld.processManager.upcomingProcess != null) return;
            for (int i = 0; i < game.Players.Count; i++)
            {
                if (game.Players[i] != null && game.Players[i].realizedCreature is Player player && player.room != null && (!starve.Value || player.Malnourished))
                {
                    for (int j = 0; j < player.room.updateList.Count; j++)
                    {
                        if (player.room.updateList[j] is Ghost echo && game.Players[i].world.worldGhost != null && (echo.fadeOut > 0f || echo.hasRequestedShutDown))
                        {
                            SeeGhost(game.Players[i].world.worldGhost.ghostID.value);
                            return;
                        }
                    }
                }
            }
        }

        public override int Points()
        {
            return 20;
        }

        public override Challenge Generate()
        {
            List<string> list = [];
            for (int i = 0; i < ExtEnum<GhostWorldPresence.GhostID>.values.entries.Count; i++)
            {
                if (ExtEnum<GhostWorldPresence.GhostID>.values.entries[i] != "NoGhost" && (!ModManager.MSC || !(ExtEnum<GhostWorldPresence.GhostID>.values.entries[i] == "MS")) && (!ModManager.MSC || !(ExtEnum<GhostWorldPresence.GhostID>.values.entries[i] == "SL") || !(ExpeditionData.slugcatPlayer != MoreSlugcatsEnums.SlugcatStatsName.Saint)) && ChallengeUtils.GetSortedCorrectListForChallenge("regionsreal").Contains(ExtEnum<GhostWorldPresence.GhostID>.values.entries[i]))
                {
                    list.Add(ExtEnum<GhostWorldPresence.GhostID>.values.entries[i]);
                }
            }
            return new BingoEchoChallenge
            {
                specific = new SettingBox<bool>(Random.value < 0.5f, "Specific Echo", 0),
                ghost = new(list[Random.Range(0, list.Count)], "Region", 1, listName: "echoes"),
                amount = new(Random.Range(2, 7), "Amount", 2),
                starve = new(Random.value < 0.1f, "While Starving", 3)
            };
        }

        public override bool RequireSave() => false;

        public override bool CombatRequired()
        {
            return false;
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoEchoChallenge c || c.ghost.Value != ghost.Value;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Visiting echoes");
        }

        public override string ToString()
        {
            return string.Concat(
            [
                "BingoEchoChallenge",
                "~",
                specific.ToString(),
                "><",
                ghost.ToString(),
                "><",
                starve.ToString(),
                "><",
                current.ToString(),
                "><",
                amount.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0",
                string.Join("|", visited),
            ]);
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                if (array.Length == 7)
                {
                    specific = SettingBoxFromString(array[0]) as SettingBox<bool>;
                    ghost = SettingBoxFromString(array[1]) as SettingBox<string>;
                    starve = SettingBoxFromString(array[2]) as SettingBox<bool>;
                    current = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
                    amount = SettingBoxFromString(array[4]) as SettingBox<int>;
                    completed = (array[5] == "1");
                    revealed = (array[6] == "1");
                    string[] arr = Regex.Split(array[7], "|");
                    visited = [.. arr];
                }
                else if (array.Length == 4)
                {
                    ghost = SettingBoxFromString(array[0]) as SettingBox<string>;
                    starve = SettingBoxFromString(array[1]) as SettingBox<bool>;
                    completed = (array[2] == "1");
                    revealed = (array[3] == "1");
                    specific = SettingBoxFromString("System.Boolean|true|Specific Echo|0|NULL") as SettingBox<bool>;
                    current = 0;
                    amount = SettingBoxFromString("System.Int32|2|Amount|1|NULL") as SettingBox<int>;
                    visited = [];
                }
                UpdateDescription();
            }
            catch (System.Exception ex)
            {
                ExpLog.Log("ERROR: BingoEchoChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.Ghost.StartConversation += Ghost_StartConversation;
        }

        public override void RemoveHooks()
        {

            On.Ghost.StartConversation -= Ghost_StartConversation;
        }

        public override List<object> Settings() => [ghost, specific, amount, starve];
    }
}
