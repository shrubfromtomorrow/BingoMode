using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using System.Linq; 
using static BingoMode.Challenges.ChallengeHooks;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoDodgeLeviathanChallenge : Challenge, IBingoChallenge
    {
        public int wasInArea;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Dodge a leviathan's bite");
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoDodgeLeviathanChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Dodge Leviathan Bite");
        }

        public void Dodged()
        {
            if (!completed) CompleteChallenge();
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
                "DodgeLeviathan",
                "~",
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
                completed = (array[0] == "1");
                hidden = (array[1] == "1");
                revealed = (array[2] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: DodgeLeviathan FromString() encountered an error: " + ex.Message);
            }
        }

        public void AddHooks()
        {
            On.BigEel.JawsSnap += BigEel_JawsSnap;
        }

        public void RemoveHooks()
        {
            On.BigEel.JawsSnap -= BigEel_JawsSnap;
        }
    }
}
