using Expedition;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BingoMode.BingoSteamworks;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    //Using counts as either throwing an item, or holding it for more than 5 seconds
    public class BingoDontUseItemChallenge : BingoChallenge
    {
        public SettingBox<string> item;
        public bool isFood;
        public bool isCreature;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Never " + (isFood ? "eat" : "use") + " <item>")
                .Replace("<item>", isFood && isCreature ? ChallengeTools.IGT.Translate(ChallengeTools.creatureNames[new CreatureTemplate.Type(item.Value).Index]) : ChallengeTools.ItemName(new(item.Value)));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase([new Icon("buttonCrossA", 1f, Color.red), new Icon(ChallengeUtils.ItemOrCreatureIconName(item.Value), 1f, ChallengeUtils.ItemOrCreatureIconColor(item.Value))], []);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoDontUseItemChallenge c || (c.item.Value != item.Value);
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Avoiding items");
        }

        public override Challenge Generate()
        {
            bool edible = UnityEngine.Random.value < 0.5f;
            string type;
            bool c = false;
            if (edible)
            {
                List<string> foob = [.. ChallengeUtils.FoodTypes];
                if (!ModManager.MSC) foob.RemoveRange(ChallengeUtils.FoodTypes.IndexOf("GooieDuck"), 4);
                if (ExpeditionData.slugcatPlayer.value != "Rivulet" && ExpeditionData.slugcatPlayer.value != "Saint") foob.Remove("GlowWeed");
                type = foob[UnityEngine.Random.Range(0, foob.Count)];
                c = ChallengeUtils.FoodTypes.IndexOf(type) >= ChallengeUtils.FoodTypes.IndexOf("VultureGrub");
            } 
            else type = ChallengeUtils.Bannable[UnityEngine.Random.Range(0, ChallengeUtils.Bannable.Length)];
            BingoDontUseItemChallenge ch = new BingoDontUseItemChallenge
            {
                item = new(type, "Item type", 0, listName: "banitem"),
                isFood = edible,
                isCreature = c
            };
            return ch;
        }

        public override bool RequireSave() => false;
        public override bool ReverseChallenge() => true;

        public void Used(AbstractPhysicalObject.AbstractObjectType used)
        {
            if (used.value == item.Value && !TeamsFailed[SteamTest.team] && completed)
            {
                FailChallenge(SteamTest.team);
            }
        }

        public void Eated(IPlayerEdible used)
        {
            if (TeamsFailed[SteamTest.team] || !completed) return;
            if (used is PhysicalObject p && (isCreature ? (p.abstractPhysicalObject is AbstractCreature g && g.creatureTemplate.type.value == item.Value) : (p.abstractPhysicalObject.type.value == item.Value)))
            {
                FailChallenge(SteamTest.team);
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
                revealed ? "1" : "0",
                "><",
                isCreature ? "1" : "0",
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
                revealed = (array[3] == "1");
                isCreature = array[4] == "1";
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoDontUseItemChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.Player.ThrowObject += Player_ThrowObject;
            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.ObjectEaten += Player_ObjectEaten2;
        }

        public override void RemoveHooks()
        {
            On.Player.ThrowObject -= Player_ThrowObject;
            On.Player.GrabUpdate -= Player_GrabUpdate;
            On.Player.ObjectEaten -= Player_ObjectEaten2;
        }

        public override List<object> Settings() => [item];
    }
}