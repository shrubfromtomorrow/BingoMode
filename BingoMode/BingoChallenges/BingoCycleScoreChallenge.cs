using BingoMode.BingoRandomizer;
using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;

    public class BingoCycleScoreRandomizer : ChallengeRandomizer
    {
        public Randomizer<int> target;

        public override Challenge Random()
        {
            BingoCycleScoreChallenge challenge = new();
            challenge.target.Value = target.Random();
            return challenge;
        }

        public override StringBuilder Serialize(string indent)
        {
            string surindent = indent + INDENT_INCREMENT;
            StringBuilder serializedContent = new();
            serializedContent.AppendLine($"{surindent}target-{target.Serialize(surindent)}");
            return base.Serialize(indent).Replace("__Type__", "CycleScore").Replace("__Content__", serializedContent.ToString());
        }

        public override void Deserialize(string serialized)
        {
            Dictionary<string, string> dict = ToDict(serialized);
            target = Randomizer<int>.InitDeserialize(dict["target"]);
        }
    }

    public class BingoCycleScoreChallenge : BingoChallenge
    {
        public SettingBox<int> target;
        public int score;

        public BingoCycleScoreChallenge()
        {
            target = new(0, "Target Score", 0);
        }

        public override void UpdateDescription()
        {
            int value = completed ? target.Value : score;
            description = ChallengeTools.IGT.Translate("Earn [<current_score>/<score_target>] points from creature kills in one cycle").Replace("<score_target>", target.Value.ToString()).Replace("<current_score>", ValueConverter.ConvertToString<int>(value));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase(
                [[new Icon("Multiplayer_Star"), new Icon("cycle_limit")],
                [new Counter(score, target.Value)]]);
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
            int num = UnityEngine.Random.Range(20, 151);
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

        public override void Update()
        {
            base.Update();
            if (revealed || completed) return;
            if (this.game?.cameras[0]?.room?.shelterDoor != null && this.game.cameras[0].room.shelterDoor.IsClosing)
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
