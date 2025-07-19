using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoGlobalScoreChallenge : BingoChallenge
    {
        public SettingBox<int> target;
        public int score;

        public BingoGlobalScoreChallenge()
        {
            target = new(0, "Target Score", 0);
        }

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Earn [<current_score>/<score_target>] points from creature kills").Replace("<score_target>", ValueConverter.ConvertToString<int>(target.Value)).Replace("<current_score>", ValueConverter.ConvertToString<int>(this.score));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase(
                [[new Icon("Multiplayer_Star")],
                [new Counter(score, target.Value)]]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return !(challenge is BingoGlobalScoreChallenge);
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Scoring global points");
        }

        public override Challenge Generate()
        {
            int num = UnityEngine.Random.Range(80, 301);
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
                revealed ? "1" : "0",
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
                revealed = (array[3] == "1");
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
