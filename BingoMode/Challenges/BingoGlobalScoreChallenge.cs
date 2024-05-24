using Expedition;
using Menu.Remix;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoGlobalScoreChallenge : BingoChallenge
    {
        public SettingBox<int> target;
        public int score;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Earn [<current_score>/<score_target>] points from creature kills").Replace("<score_target>", ValueConverter.ConvertToString<int>(target.Value)).Replace("<current_score>", ValueConverter.ConvertToString<int>(this.score));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase([new Icon("Multiplayer_Star", 1f, Color.white), new Counter(score, target.Value)], [1]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return !(challenge is BingoGlobalScoreChallenge);
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Overall Score");
        }

        public override Challenge Generate()
        {
            int num = Mathf.RoundToInt(Mathf.Lerp(150f, 300f, UnityEngine.Random.value) / 10f) * 10;
            return new BingoGlobalScoreChallenge
            {
                target = new(num, "Target Score", 0)
            };
        }

        public override void Reset()
        {
            score = 0;
            base.Reset();
        }

        public override int Points()
        {
            float num = 1f;
            if (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                num = 1.35f;
            }
            return (int)((float)(this.target.Value / 4) * num) * (int)(this.hidden ? 2f : 1f);
        }

        public override bool CombatRequired()
        {
            return true;
        }

        public override bool RespondToCreatureKill()
        {
            return true;
        }

        public override void CreatureKilled(Creature crit, int playerNumber)
        {
            Plugin.logger.LogMessage("creaturekil global score " + this);
            if (this.completed || this.game == null || crit == null)
            {
                return;
            }
            CreatureTemplate.Type type = crit.abstractCreature.creatureTemplate.type;
            if (type != null && ChallengeTools.creatureSpawns[ExpeditionData.slugcatPlayer.value].Find((ChallengeTools.ExpeditionCreature f) => f.creature == type) != null)
            {
                int points = ChallengeTools.creatureSpawns[ExpeditionData.slugcatPlayer.value].Find((ChallengeTools.ExpeditionCreature f) => f.creature == type).points;
                score += points;
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
            UpdateDescription();
            if (!RequireSave) Expedition.Expedition.coreFile.Save(false);
            if (score >= target.Value)
            {
                score = target.Value;
                CompleteChallenge();
            }
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoGlobalScoreChallenge",
                "~",
                ValueConverter.ConvertToString<int>(score),
                "><",
                target.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                hidden ? "1" : "0",
                "><",
                revealed ? "1" : "0"
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                score = int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                target = SettingBoxFromString(array[1]) as SettingBox<int>;
                completed = (array[2] == "1");
                hidden = (array[3] == "1");
                revealed = (array[4] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoGlobalScoreChallenge FromString() encountered an error: " + ex.Message);
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
