using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using System.Linq;
using ItemType = AbstractPhysicalObject.AbstractObjectType;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoStealChallenge : Challenge, IBingoChallenge
    {
        public int current;
        public int amount;
        public bool toll;
        public ItemType subject;
        public List<EntityID> checkedIDs;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Steal [<current>/<amount>] <item> from " + (toll ? "a Scavenger toll" : "Scavengers"))
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount))
                .Replace("<item>", ChallengeTools.ItemName(subject));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoStealChallenge || ((challenge as BingoStealChallenge).subject != subject && (challenge as BingoStealChallenge).toll != toll);
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Theft");
        }

        public override Challenge Generate()
        {
            bool taxEvasion = UnityEngine.Random.value < 0.5f;
            ItemType itme = ItemType.Spear;
            if (taxEvasion)
            {
                itme = UnityEngine.Random.value < 0.5f ? ItemType.Spear : ItemType.DataPearl;
            }
            else itme = ChallengeUtils.StealableStolable[UnityEngine.Random.Range(0, ChallengeUtils.StealableStolable.Length)];

            return new BingoStealChallenge
            {
                checkedIDs = [],
                toll = taxEvasion,
                subject = itme,
                amount = UnityEngine.Random.Range(1, subject == ItemType.ScavengerBomb ? 3 : 5)
            };
        }

        public override int Points()
        {
            return amount * 10;
        }

        public override bool CombatRequired()
        {
            return false;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return true;
        }

        public void Stoled(AbstractPhysicalObject item, bool tollCheck)
        {
            if (!completed && item.type == subject && tollCheck == toll && !checkedIDs.Contains(item.ID))
            {
                current++;
                UpdateDescription();
                if (current >= amount)
                {
                    CompleteChallenge();
                }
                checkedIDs.Add(item.ID);
            }
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoStealChallenge",
                "~",
                ValueConverter.ConvertToString(subject),
                "><",
                toll ? "1" : "0",
                "><",
                current.ToString(),
                "><",
                amount.ToString(),
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
                subject = new (array[0], false);
                toll = (array[1] == "1");
                current = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                amount = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
                completed = (array[4] == "1");
                hidden = (array[5] == "1");
                revealed = (array[6] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoStealChallenge FromString() encountered an error: " + ex.Message);
            }
        }

        public void AddHooks()
        {
            On.ScavengerOutpost.PlayerTracker.Update += PlayerTracker_Update;
            On.SocialEventRecognizer.Theft += SocialEventRecognizer_Theft;
        }

        public void RemoveHooks()
        {
            On.ScavengerOutpost.PlayerTracker.Update -= PlayerTracker_Update;
            On.SocialEventRecognizer.Theft -= SocialEventRecognizer_Theft;
        }
    }
}