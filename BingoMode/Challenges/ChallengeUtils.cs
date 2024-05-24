using Expedition;
using ItemType = AbstractPhysicalObject.AbstractObjectType;
using CreatureType = CreatureTemplate.Type;
using MSCItemType = MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType;
using PearlType = DataPearl.AbstractDataPearl.DataPearlType;
using System.Collections.Generic;
using MoreSlugcats;
using System.Linq;
using UnityEngine;

namespace BingoMode.Challenges
{
    public class ChallengeUtils
    {
        public static void Apply()
        {
            On.Expedition.ChallengeTools.ItemName += ChallengeTools_ItemName;
            On.Expedition.ChallengeTools.CreatureName += ChallengeTools_CreatureName;
        }

        public static string ItemOrCreatureIconName(string thing)
        {
            string elementName = ItemSymbol.SpriteNameForItem(new(thing, false), 0);
            if (elementName == "Futile_White")
            {
                elementName = CreatureSymbol.SpriteNameOfCreature(new IconSymbol.IconSymbolData(new CreatureType(thing, false), ItemType.Creature, 0));
            }
            return elementName;
        }

        public static Color ItemOrCreatureIconColor(string thing)
        {
            Color color = ItemSymbol.ColorForItem(new(thing, false), 0);
            if (color == Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey))
            {
                color = CreatureSymbol.ColorOfCreature(new IconSymbol.IconSymbolData(new CreatureType(thing, false), ItemType.Creature, 0));
            }
            return color;
        }

        public static string[] GetCorrectListForChallenge(string listName)
        {
            string ln = listName;
            //bool addEmpty = false;
            //if (ln[0] == '_')
            //{
            //    addEmpty = true;
            //    ln = ln.Substring(1);
            //}
            switch (ln)
            {
                case "transport": return Transportable;
                case "pin": return Pinnable;
                case "tolls": return BombableOutposts;
                case "food": return FoodTypes;
                case "weapons": return Weapons;
                case "theft": return StealableStolable;
                case "ban": return Bannable;
                case "friend": return Befriendable;
                case "pearls": return CollectablePearls;
                case "craft": return CraftableItems;
                case "regions": return ["Any Region", .. SlugcatStats.SlugcatStoryRegions(ExpeditionData.slugcatPlayer), ..SlugcatStats.SlugcatOptionalRegions(ExpeditionData.slugcatPlayer)];
                case "echoes": return [.. GhostWorldPresence.GhostID.values.entries.Where(x => x != "NoGhost")];
                case "creatures": return ["Any Creature", .. CreatureType.values.entries.Where(x => ChallengeTools.creatureSpawns[ExpeditionData.slugcatPlayer.value].Any(g => g.creature.value == x))];
                case "depths": return ["Hazer", "VultureGrub"];
                case "banitem": return [.. FoodTypes, .. Bannable];
                case "unlocks": return [.. BingoData.possibleTokens[0], .. BingoData.possibleTokens[1], .. BingoData.possibleTokens[2], .. BingoData.possibleTokens[3]];
                case "passage": return [.. WinState.EndgameID.values.entries.Where(x => x != "Mother" && x != "Gourmand")];
                case "expobject": return ["FirecrackerPlant", "FlareBomb", "FlyLure", "JellyFish", "Lantern", "Mushroom", "PuffBall", "ScavengerBomb", "VultureMask"];
                case "vista": return [.. ChallengeTools.VistaLocations.Keys];
                case "subregions": return ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Saint ? [.. SaintSubregions] : [.. AllSubregions];
            }
            return ["Whoops something went wrong"];
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
            creatureNames[(int)MoreSlugcatsEnums.CreatureTemplateType.Yeek] = ChallengeTools.IGT.Translate("Yeeks");
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

        public static readonly string[] AllSubregions =
        {
            "Chimney Canopy",
            "Drainage System",
            "Garbage Wastes",
            "Industrial Complex",
            "Farm Arrays",
            "Subterranean",
            "Depths",
            "Filtration System",
            "Shaded Citadel",
            "Memory Crypts",
            "Sky Islands",
            "Communications Array",
            "Shoreline",
            "Looks to the Moon",
            "Five Pebbles",
            "Five Pebbles (Memory Conflux)",
            "Five Pebbles (Recursive Transform Array)",
            "Five Pebbles (Unfortunate Development)",
            "Five Pebbles (General Systems Bus)",
            "Outskirts",
            "The Leg",
            "Underhang",
            "The Wall",
            // msc (THERES TOO MANY OF THEM
            "The Gutter",
            "The Precipice",
            "Frosted Cathedral",
            "The Husk",
            "Silent Construct",
            "Looks to the Moon (Abstract Convergence Manifold)",
            "Struts",
            "Looks to the Moon (Neural Terminus)",
            "Luna",
            "Looks to the Moon (Memory Conflux)",
            "Looks to the Moon (Vents)",
            "Metropolis",
            "Atop the Tallest Tower",
            "The Floor",
            "12th Council Pillar, the House of Braids",
            "Waterfront Facility",
            "Waterfront Facility",
            "The Shell",
            "Submerged Superstructure",
            "Submerged Superstructure (The Heart)",
            "Auxiliary Transmission Array",
            "Submerged Superstructure (Vents)",
            "Bitter Aerie",
            "Outer Expanse",
            "Sunken Pier",
            "Facility Roots (Western Intake)",
            "Journey's End",
            "The Rot",
            "Five Pebbles (Primary Cortex)",
            "The Rot (Depths)",
            "The Rot (Cystic Conduit)",
            "Five Pebbles (Linear Systems Rail)",
            "Undergrowth",
            "Pipeyard",
            "Sump Tunnel"
        };

        public static readonly string[] SaintSubregions =
        {
            "Solitary Towers",
            "Forgotten Conduit",
            "Frosted Cathedral",
            "The Husk",
            "Silent Construct",
            "Five Pebbles",
            "Glacial Wasteland",
            "Icy Monument",
            "Desolate Fields",
            "Primordial Underground",
            "...",
            "Ancient Labyrinth",
            "Windswept Spires",
            "Frozen Mast",
            "Frigid Coast",
            "Looks to the Moon",
            "The Precipice",
            "Suburban Drifts",
            "Undergrowth",
            "Barren Conduits",
            "Desolate Canal",
            "???"
        };

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

        public static readonly string[] FoodTypes =
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
            "GlowWeed",
            "VultureGrub",
            "Hazer",
            "SmallNeedleWorm",
            "Fly",
            "SmallCentipede"
        };

        public static readonly string[] Weapons =
        {
            "Any Weapon",
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
