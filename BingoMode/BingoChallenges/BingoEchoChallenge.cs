﻿using BingoMode.BingoSteamworks;
using Expedition;
using MoreSlugcats;
using RWCustom;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    // Literally copied from base game, to add the starving thing easily, and to customize which echoes appear
    public class BingoEchoChallenge : BingoChallenge
    {
        public SettingBox<string> ghost; //GhostWorldPresence.GhostID
        public SettingBox<bool> starve;

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate("Visit the <echo_location> Echo" + (starve.Value ? " while starving" : "")).Replace("<echo_location>", ChallengeTools.IGT.Translate(Region.GetRegionFullName(ghost.Value, ExpeditionData.slugcatPlayer)));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = new Phrase([new Icon("echo_icon", 1f, Color.white), new Verse(ghost.Value)], []);
            if (starve.Value)
            {
                phrase.words.Add(new Icon("Multiplayer_Death", 1f, Color.white));
                phrase.newLines = [2];
            }
            return phrase;
        }

        public void SeeGhost(string spectre)
        {
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden || spectre != ghost.Value) return;
            CompleteChallenge();
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
                ghost = new(list[Random.Range(0, list.Count)], "Region", 0, listName: "echoes"),
                starve = new(Random.value < 0.1f, "While Starving", 1),
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
                ghost.ToString(),
                "><",
                starve.ToString(),
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
                ghost = SettingBoxFromString(array[0]) as SettingBox<string>;
                starve = SettingBoxFromString(array[1]) as SettingBox<bool>;
                completed = (array[2] == "1");
                revealed = (array[3] == "1");
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

        public override List<object> Settings() => [ghost, starve];
    }
}
