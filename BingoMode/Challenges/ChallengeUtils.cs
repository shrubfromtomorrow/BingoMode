using UnityEngine;
using RWCustom;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using MonoMod.Utils;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Expedition;
using ItemType = AbstractPhysicalObject.AbstractObjectType;
using CreatureType = CreatureTemplate.Type;
using MSCItemType = MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType;
using PearlType = DataPearl.AbstractDataPearl.DataPearlType;
using System.Linq;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace BingoMode.Challenges
{
    public class ChallengeUtils
    {
        public static void Apply()
        {
            On.Expedition.ChallengeTools.ItemName += ChallengeTools_ItemName;
            On.Expedition.ChallengeTools.CreatureName += ChallengeTools_CreatureName;
        }

        public static string ChallengeTools_ItemName(On.Expedition.ChallengeTools.orig_ItemName orig, ItemType type)
        {
            InGameTranslator translator = ChallengeTools.IGT;
            // Weapons
            if (type == ItemType.Spear) return translator.Translate("Spears");
            if (type == ItemType.Rock) return translator.Translate("Rocks");
            // Food items
            if (type == ItemType.DangleFruit) return translator.Translate("Blue Fruit");
            if (type == ItemType.EggBugEgg) return translator.Translate("Eggbug Eggs");
            if (type == ItemType.WaterNut) return translator.Translate("Bubble Fruit");
            if (type == ItemType.SlimeMold) return translator.Translate("Slime Mold");
            if (type == ItemType.BubbleGrass) return translator.Translate("Slime Mold");
            if (type == MSCItemType.GlowWeed) return translator.Translate("Glow Weed");
            if (type == MSCItemType.DandelionPeach) return translator.Translate("Dandelion Peaches");
            if (type == MSCItemType.LillyPuck) return translator.Translate("Lillypucks");
            if (type == MSCItemType.GooieDuck) return translator.Translate("Gooieducks");

            return orig.Invoke(type);
        }

        public static void ChallengeTools_CreatureName(On.Expedition.ChallengeTools.orig_CreatureName orig, ref string[] creatureNames)
        {
            orig.Invoke(ref creatureNames);
            creatureNames[(int)CreatureType.SmallNeedleWorm] = ChallengeTools.IGT.Translate("Small Noodleflies");
            creatureNames[(int)CreatureType.VultureGrub] = ChallengeTools.IGT.Translate("Vulture Grubs");
            creatureNames[(int)CreatureType.Hazer] = ChallengeTools.IGT.Translate("Hazers");
        }

        public static string NameForPearl(PearlType pearl)
        {
            switch(pearl.value)
            {
                case "SU":
                    return "Light Blue";
                case "UW":
                    return "Pale Green";
                case "SH":
                    return "Deep Magenta";
                case "LF_bottom":
                    return "Bright Red";
                case "LF_west":
                    return "Deep Pink";
                case "SL_moon":
                    return "Pale Yellow";
                case "SL_chimney":
                    return "Bright Magenta";
                case "SL_bridge":
                    return "Bright Purple";
                case "DS":
                    return "Bright Green";
                case "GW":
                    return "Viridian";
                case "CC":
                    return "Gold";
                case "HI":
                    return "Bright Blue";
                case "SB_filtration":
                    return "Teal";
                case "SB_ravine":
                    return "Dark Magenta";
                case "SI_top":
                    return "Dark Blue";
                case "SI_west":
                    return "Dark Green";
                case "SI_chat3":
                    return "Dark Purple";
                case "SI_chat4":
                    return "Olive Green";
                case "SI_chat5":
                    return "Dark Magenta";
                case "VS":
                    return "Deep Purple";
            }

            return "NULL";
        }

        public static ItemType[] ItemFoodTypes =
        {
            ItemType.DangleFruit,
            ItemType.EggBugEgg,
            ItemType.WaterNut,
            ItemType.SlimeMold,
            ItemType.Mushroom,
            ItemType.JellyFish,
            new("GooieDuck", false),
            new("LillyPuck", false),
            new("DandelionPeach", false),
            new("GlowWeed", false),
        };

        public static CreatureType[] CreatureFoodTypes =
        {
            CreatureType.VultureGrub,
            CreatureType.Hazer,
            CreatureType.SmallNeedleWorm,
            CreatureType.Fly,
            CreatureType.SmallCentipede
        };

        public static ItemType[] Weapons =
        {
            ItemType.Spear,
            ItemType.Rock,
            ItemType.ScavengerBomb,
            ItemType.PuffBall,
            ItemType.SporePlant,
            ItemType.JellyFish,
            new("LillyPuck", false),
        };

        public static ItemType[] StealableStolable =
        {
            ItemType.Spear,
            ItemType.Rock,
            ItemType.ScavengerBomb,
        };

        public static ItemType[] Bannable =
        {
            ItemType.FlyLure,
            ItemType.Lantern,
            ItemType.PuffBall,
            ItemType.VultureMask,
            ItemType.ScavengerBomb,
            ItemType.BubbleGrass,
            ItemType.DangleFruit,
            ItemType.SlimeMold,
            ItemType.WaterNut
        };

        public static CreatureType[] Befriendable =
        {
            CreatureType.CicadaA,
            CreatureType.CicadaB,
            CreatureType.GreenLizard,
            CreatureType.PinkLizard,
            CreatureType.YellowLizard,
            CreatureType.BlackLizard,
            CreatureType.CyanLizard,
            CreatureType.WhiteLizard,
            CreatureType.BlueLizard,
            new ("EelLizard", false),
            new ("SpitLizard", false),
            new ("ZoopLizard", false)
        };

        public static PearlType[] CollectablePearls = 
        {
            PearlType.UW,
            PearlType.SH,
            PearlType.LF_bottom,
            PearlType.LF_west,
            PearlType.SL_moon,
            PearlType.SL_bridge,
            PearlType.SL_chimney,
            PearlType.DS,
            PearlType.CC,
            PearlType.GW,
            PearlType.HI,
            PearlType.SB_filtration,
            PearlType.SB_ravine,
            PearlType.SU,
            PearlType.SI_top,
            PearlType.SI_west,
            new("SI_chat3", false),
            new("SI_chat4", false),
            new("SI_chat5", false),
            new("VS", false),
        };
    }
}
