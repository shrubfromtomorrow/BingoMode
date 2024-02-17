using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Runtime.CompilerServices;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    //Using counts as either throwing an item, or holding it for more than 5 seconds
    public class BingoDontUseItemChallenge : Challenge, IBingoChallenge
    {
        public SettingBox<string> item;
        public bool isFood;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Never " + (isFood ? "eat" : "use") + " <item>").Replace("<item>", ChallengeTools.ItemName(new(item.Value)));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoDontUseItemChallenge c || (c.item != item && c.isFood != isFood);
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Item Avoiding");
        }

        public override Challenge Generate()
        {
            bool edible = UnityEngine.Random.value < 0.5f;
            string type;
            if (edible)
            {
                type = ChallengeUtils.FoodTypes[UnityEngine.Random.Range(0, ChallengeUtils.FoodTypes.Length - (ModManager.MSC ? 5 : 1))];
            } 
            else type = ChallengeUtils.Bannable[UnityEngine.Random.Range(0, ChallengeUtils.Bannable.Length)];
            return new BingoDontUseItemChallenge
            {
                item = new(type, "Item type", 0, listName: "banitem"),
                isFood = edible,
                completed = true
            };
        }

        public void Used(AbstractPhysicalObject.AbstractObjectType used)
        {
            if (used.value == item.Value)
            {
                completed = false;
                //
            }
        }

        public override void Update()
        {
            base.Update();

            if (isFood) return;
            for (int i = 0; i < BingoData.heldItemsTime.Length; i++)
            {
                if (i == (int)new AbstractPhysicalObject.AbstractObjectType(item.Value) && BingoData.heldItemsTime[i] > 200) Used(new(item.Value)); 
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
                "BingoDontUseItemChallenge",
                "~",
                item.ToString(),
                "><",
                isFood ? "1" : "0",
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
                item = SettingBoxFromString(array[0]) as SettingBox<string>;
                isFood = (array[1] == "1");
                completed = (array[2] == "1");
                hidden = (array[3] == "1");
                revealed = (array[4] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoDontUseItemChallenge FromString() encountered an error: " + ex.Message);
            }
        }

        public void AddHooks()
        {
            On.Player.ThrowObject += Player_ThrowObject;
            On.Player.GrabUpdate += Player_GrabUpdate;
        }

        public void RemoveHooks()
        {
            On.Player.ThrowObject -= Player_ThrowObject;
            On.Player.GrabUpdate -= Player_GrabUpdate;
        }

        public List<object> Settings() => [item];
    }
}