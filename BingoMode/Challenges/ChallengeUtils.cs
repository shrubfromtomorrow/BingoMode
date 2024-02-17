using Expedition;
using ItemType = AbstractPhysicalObject.AbstractObjectType;
using CreatureType = CreatureTemplate.Type;
using MSCItemType = MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType;
using PearlType = DataPearl.AbstractDataPearl.DataPearlType;
using System.Collections.Generic;
using MoreSlugcats;

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
            if (type == ItemType.SporePlant) return translator.Translate("Bee Hives");
            if (type == ItemType.DataPearl) return translator.Translate("Pearls");
            // Food items
            if (type == ItemType.DangleFruit) return translator.Translate("Blue Fruit");
            if (type == ItemType.EggBugEgg) return translator.Translate("Eggbug Eggs");
            if (type == ItemType.WaterNut) return translator.Translate("Bubble Fruit");
            if (type == ItemType.SlimeMold) return translator.Translate("Slime Mold");
            if (type == ItemType.BubbleGrass) return translator.Translate("Bubble Grass");
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

        public static List<string> CreatureOriginRegions(string type, SlugcatStats.Name slug)
        {
            List<string> r = [];
            switch (type)
            {
                case "CicadaA":
                case "CicadaB":
                    r.AddRange(["SU", "HI", "LF", "SI"]);
                    if (slug != MoreSlugcatsEnums.SlugcatStatsName.Rivulet) r.Add("VS");
                    break;
                case "Hazer":
                    r.AddRange(["HI", "GW", "SL", "DS", "LF"]);
                    break;
                case "VultureGrub":
                    r.AddRange(["HI", "GW", "CC", "LF"]);
                    break;
                case "JetFish":
                    r.Add("SL"); //if (slug == SlugcatStats.Name.Red) r.Add("sb");
                    break;
                case "Yeek":
                    r.Add("OE"); if (slug == MoreSlugcatsEnums.SlugcatStatsName.Saint || slug == MoreSlugcatsEnums.SlugcatStatsName.Rivulet) r.AddRange(["SB", "LF"]);
                    break;
            }

            return r;
        }

        public static readonly string[] Transportable =
        {
            "JetFish",
            "Hazer",
            "VultureGrub",
            "CicadaA",
            "CicadaB",
            "Yeek"
        };

        public static readonly string[] Pinnable =
        {
            "CicadaA",
            "CicadaB",
            "Scavenger",
            "BlackLizard",
            "PinkLizard",
            "BlueLizard",
            "YellowLizard",
            "WhiteLizard",
            "GreenLizard",
            "Snail",
            "Centipede",
            "LanternMouse"
        };

        public static readonly string[] BombableOutposts =
        {
            "su_c02",
            "gw_c05",
            "gw_c11",
            "lf_e03",
            "ug_toll",
        };

        public static string NameForPearl(string pearl)
        {
            switch (pearl)
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

        public static readonly string[] ItemFoodTypes =
        {
            "DangleFruit",
            "EggBugEgg",
            "WaterNut",
            "SlimeMold",
            "JellyFish",
            "Mushroom",
            "GooieDuck",
            "LillyPuck",
            "DandelionPeach",
            "GlowWeed"
        };

        public static readonly string[] CreatureFoodTypes =
        {
            "VultureGrub",
            "Hazer",
            "SmallNeedleWorm",
            "Fly",
            "SmallCentipede"
        };

        public static readonly string[] Weapons =
        {
            "Spear",
            "Rock",
            "ScavengerBomb",
            "FlareBomb",
            "JellyFish",
            "PuffBall",
            "LillyPuck"
        };

        public static readonly string[] StealableStolable =
        {
            "Spear",
            "Rock",
            "ScavengerBomb"
        };

        public static readonly string[] Bannable =
        {
            "FlyLure",
            "Lantern",
            "PuffBall",
            "VultureMask",
            "ScavengerBomb",
            "BubbleGrass"
        };

        public static readonly string[] Befriendable =
        {
            "CicadaA",
            "CicadaB",
            "GreenLizard",
            "PinkLizard",
            "YellowLizard",
            "BlackLizard",
            "CyanLizard",
            "WhiteLizard",
            "BlueLizard",
            "EelLizard",
            "SpitLizard",
            "ZoopLizard"
        };

        public static readonly string[] CollectablePearls =
        {
            "UW",
            "SH",
            "LF_bottom",
            "LF_west",
            "SL_moon",
            "SL_bridge",
            "SL_chimney",
            "DS",
            "CC",
            "GW",
            "HI",
            "SB_filtration",
            "SB_ravine",
            "SU",
            "SI_top",
            "SI_west",
            "SI_chat3",
            "SI_chat4",
            "SI_chat5",
            "VS"
        };

        public static readonly string[] CraftableItems =
        {
            "Rock",
            "FlareBomb",
            "SporePlant",
            "ScavengerBomb",
            "VultureMask",
            "SlimeMold",
            "FirecrackerPlant",
            "PuffBall",
            "Mushroom",
            "GlowWeed",
            "GooieDuck",
            "FireEgg",
            "SingularityBomb"
        };
    }
}
