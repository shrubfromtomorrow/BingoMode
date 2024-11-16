using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoCycleScoreChallenge : BingoChallenge
    {
        public SettingBox<int> target;
        public int score;

        public override void UpdateDescription()
        {
            int value = completed ? target.Value : score;
            description = ChallengeTools.IGT.Translate("Earn [<current_score>/<score_target>] points from creature kills in one cycle").Replace("<score_target>", target.Value.ToString()).Replace("<current_score>", ValueConverter.ConvertToString<int>(value));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase([new Icon("Multiplayer_Star", 1f, Color.white), new Icon("cycle_limit", 1f, Color.white), new Counter(score, target.Value)], [2]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return !(challenge is BingoCycleScoreChallenge);
        }

        public override void Reset()
        {
            this.score = 0;
            base.Reset();
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Scoring cycle points");
        }

        public override Challenge Generate()
        {
            int num = Mathf.RoundToInt(Mathf.Lerp(20f, 150f, UnityEngine.Random.value) / 10f) * 10;
            return new BingoCycleScoreChallenge
            {
                target = new(num, "Target Score", 0)
            };
        }

        public override bool CombatRequired()
        {
            return true;
        }

        public override int Points()
        {
            float num = 1f;
            if (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                num = 1.35f;
            }
            return (int)((float)(this.target.Value / 3) * num) * (int)(this.hidden ? 2f : 1f);
        }

        public override bool RespondToCreatureKill()
        {
            return true;
        }

        public override void CreatureKilled(Creature crit, int playerNumber)
        {
            Plugin.logger.LogInfo($"(1)Player {playerNumber} killed {crit.abstractCreature}");
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden || game == null || crit == null)
            {
                return;
            }
            int lastPoints = score;
            CreatureTemplate.Type type = crit.abstractCreature.creatureTemplate.type;
            if (type != null && ChallengeTools.creatureSpawns[ExpeditionData.slugcatPlayer.value].Find((ChallengeTools.ExpeditionCreature f) => f.creature == type) != null)
            {
                int points = ChallengeTools.creatureSpawns[ExpeditionData.slugcatPlayer.value].Find((ChallengeTools.ExpeditionCreature f) => f.creature == type).points;
                score += points;
                Plugin.logger.LogFatal("Points for kill: " + points);
            }
            if (score != lastPoints)
            {
                UpdateDescription();
                if (score >= target.Value)
                {
                    score = target.Value;
                    CompleteChallenge();
                }
                else ChangeValue();
            }
        }

        public override void Update()
        {
            base.Update();
            if (revealed || completed) return;
            if (this.game.cameras[0].room.shelterDoor != null && this.game.cameras[0].room.shelterDoor.IsClosing)
            {
                if (this.score != 0)
                {
                    this.score = 0;
                    this.UpdateDescription();
                    ChangeValue();
                }
                return;
            }
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoCycleScoreChallenge",
                "~",
                target.ToString(),
                "><",
                this.completed ? "1" : "0",
                "><",
                this.revealed ? "1" : "0",
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                this.target = SettingBoxFromString(array[0]) as SettingBox<int>;
                this.completed = (array[1] == "1");
                this.revealed = (array[2] == "1");
                if (revealed || completed)
                {
                    score = target.Value;
                }
                else score = 0;
                this.UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoCycleScoreChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
        }

        public override void RemoveHooks()
        {
        }

        public override List<object> Settings() => [target];
    }
}
