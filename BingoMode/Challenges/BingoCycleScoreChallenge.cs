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
            description = ChallengeTools.IGT.Translate("Earn [<current_score>/<score_target>] points from creature kills this cycle").Replace("<score_target>", target.Value.ToString()).Replace("<current_score>", ValueConverter.ConvertToString<int>(value));
            base.UpdateDescription();
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
            return ChallengeTools.IGT.Translate("Cycle Score");
        }

        public override Challenge Generate()
        {
            int num = Mathf.RoundToInt(Mathf.Lerp(20f, 125f, ExpeditionData.challengeDifficulty) / 10f) * 10;
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
            Plugin.logger.LogMessage("creaturekil score " + this);
            if (this.completed || this.game == null || crit == null)
            {
                return;
            }
            CreatureTemplate.Type type = crit.abstractCreature.creatureTemplate.type;
            if (type != null && ChallengeTools.creatureSpawns[ExpeditionData.slugcatPlayer.value].Find((ChallengeTools.ExpeditionCreature f) => f.creature == type) != null)
            {
                int points = ChallengeTools.creatureSpawns[ExpeditionData.slugcatPlayer.value].Find((ChallengeTools.ExpeditionCreature f) => f.creature == type).points;
                this.score += points;
                ExpLog.Log(string.Concat(new string[]
                {
                    "Player ",
                    (playerNumber + 1).ToString(),
                    " killed ",
                    type.value,
                    " | +",
                    points.ToString()
                }));
            }
            this.UpdateDescription();
            if (!RequireSave) Expedition.Expedition.coreFile.Save(false);
            if (this.score >= this.target.Value)
            {
                this.score = this.target.Value;
                this.CompleteChallenge();
            }
        }

        public override void Update()
        {
            base.Update();
            if (this.game.cameras[0].room.shelterDoor != null && this.game.cameras[0].room.shelterDoor.IsClosing)
            {
                if (this.score != 0)
                {
                    this.score = 0;
                    this.UpdateDescription();
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
                this.hidden ? "1" : "0",
                "><",
                this.revealed ? "1" : "0"
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                this.target = SettingBoxFromString(array[0]) as SettingBox<int>;
                this.completed = (array[1] == "1");
                this.hidden = (array[2] == "1");
                this.revealed = (array[3] == "1");
                if (!this.completed)
                {
                    this.score = 0;
                }
                this.score = 0;
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
