using Expedition;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoDodgeLeviathanChallenge : BingoChallenge
    {
        public int wasInArea;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Dodge a leviathan's bite");
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase([new Icon("leviathan_dodge", 1f, Color.white)], []);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoDodgeLeviathanChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Leviathan Dodging");
        }

        public void Dodged()
        {
            if (!completed && !revealed) CompleteChallenge();
        }

        public override Challenge Generate()
        {
            return new BingoDodgeLeviathanChallenge
            {
            };
        }

        public override void Update()
        {
            base.Update();
            if (completed) return;
            wasInArea = Mathf.Max(0, wasInArea - 1);
            for (int i = 0; i < game.Players.Count; i++)
            {
                if (game.Players[i] != null
                    && game.Players[i].realizedCreature != null
                    && game.Players[i].realizedCreature.room != null)
                {
                    Player player = game.Players[i].realizedCreature as Player;
                    Room room = player.room;

                    for (int j = 0; j < room.physicalObjects.Length; j++)
                    {
                        for (int k = 0; k < room.physicalObjects[j].Count; k++)
                        {
                            if (!room.physicalObjects[j][k].slatedForDeletetion && room.physicalObjects[j][k] is BigEel levi)
                            {
                                if (levi.InBiteArea(player.bodyChunks[1].pos, 10f))
                                {
                                    wasInArea = 60;
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Reset()
        {
            base.Reset();
            wasInArea = 0;
        }

        public override int Points()
        {
            return 20;
        }

        public override bool CombatRequired()
        {
            return false;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return true;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoDodgeLeviathanChallenge",
                "~",
                completed ? "1" : "0",
                "><",
                hidden ? "1" : "0",
                "><",
                revealed ? "1" : "0",
                "><",
                TeamsToString()
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                completed = (array[0] == "1");
                hidden = (array[1] == "1");
                revealed = (array[2] == "1");
                TeamsFromString(array[3]);
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoDodgeLeviathanChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.BigEel.JawsSnap += BigEel_JawsSnap;
        }

        public override void RemoveHooks()
        {
            On.BigEel.JawsSnap -= BigEel_JawsSnap;
        }

        public override List<object> Settings() => [];
    }
}
