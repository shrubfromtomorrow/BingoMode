using Expedition;
using ItemType = AbstractPhysicalObject.AbstractObjectType;
using CreatureType = CreatureTemplate.Type;
using MSCItemType = MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType;
using DLCItemType = DLCSharedEnums.AbstractObjectType;
using System.Collections.Generic;
using MoreSlugcats;
using System.Linq;
using UnityEngine;
using System;
using System.IO;
using RWCustom;
using System.Data.SqlTypes;

namespace BingoMode.BingoChallenges
{
    public static class ChallengeUtils
    {
        public static Dictionary<string, Dictionary<string, Vector2>> BingoVistaLocations;

        public static void Apply()
        {
            On.Expedition.ChallengeTools.ItemName += ChallengeTools_ItemName;
            On.Expedition.ChallengeTools.CreatureName += ChallengeTools_CreatureName;
            On.Menu.ExpeditionMenu.ExpeditionSetup += ExpeditionMenu_ExpeditionSetup;
            FetchGatesFromFile();
            FetchAllEnterableRegions();
        }

        private static void ExpeditionMenu_ExpeditionSetup(On.Menu.ExpeditionMenu.orig_ExpeditionSetup orig, Menu.ExpeditionMenu self)
        {
            orig.Invoke(self);

            GenerateBingoVistaLocations();
        }

        public static void GenerateBingoVistaLocations()
        {
            BingoVistaLocations = ChallengeTools.VistaLocations.ToDictionary(x => x.Key, x => x.Value);
            foreach (string text in Custom.rainWorld.progression.regionNames)
            {
                if (!BingoVistaLocations.ContainsKey(text))
                {
                    BingoVistaLocations[text] = new Dictionary<string, Vector2>();
                }
                string path = AssetManager.ResolveFilePath(Path.Combine("world", text, "bingovistas.txt"));
                //if (File.Exists(path))
                //{
                //    string[] lines = File.ReadAllLines(path);
                //    for (int i = 0; i < lines.Length; i++)
                //    {
                //        string text2 = lines[i];
                //        if (string.IsNullOrEmpty(text2.Trim())) continue;
                //        if (text2.StartsWith("(MSC)"))
                //        {
                //            if (!ModManager.MSC) continue;
                //            text2 = text2.Substring(5);
                //        }

                //        string[] array2 = text2.Split(',');
                //        if (array2.Length >= 3)
                //        {
                //            string text3 = array2[0];
                //            int num;
                //            int num2;
                //            if (string.IsNullOrEmpty(text3) || !int.TryParse(array2[1], out num) || !int.TryParse(array2[2], out num2))
                //            {
                //                Custom.LogWarning("Failed to parse bingo vista " + text2);
                //            }
                //            else
                //            {
                //                BingoVistaLocations[text][text3] = new Vector2((float)num, (float)num2);
                //            }
                //        }
                //    }
                //}
            }
        }

        public static string ItemOrCreatureIconName(string thing)
        {
            int data = GetIconIntData(thing);
            thing = GetIconType(thing);
            string elementName = ItemSymbol.SpriteNameForItem(new(thing, false), data);
            if (elementName == "Futile_White")
            {
                elementName = CreatureSymbol.SpriteNameOfCreature(new IconSymbol.IconSymbolData(new CreatureType(thing, false), ItemType.Creature, data));
            }
            return elementName;
        }

        public static Color ItemOrCreatureIconColor(string thing)
        {
            int data = GetIconIntData(thing);
            thing = GetIconType(thing);
            Color color = ItemSymbol.ColorForItem(new(thing, false), data);
            if (color == Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey))
            {
                color = CreatureSymbol.ColorOfCreature(new IconSymbol.IconSymbolData(new CreatureType(thing, false), ItemType.Creature, data));
            }
            return color;
        }

        private static int GetIconIntData(string thing) => thing switch
        {
            "Centipede" => 2,
            "BigCentipede" => 3,
            "FireSpear" => 1,
            "ElectricSpear" => 2,
            "HellSpear" => 3,
            _ => 0
        };

        private static string GetIconType(string thing) => thing switch
        {
            "BigCentipede" => "Centipede",
            "FireSpear" => "Spear",
            "ElectricSpear" => "Spear",
            "HellSpear" => "Spear",
            _ => thing
        };

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
                case "pin": return ["Any Creature", .. Pinnable];
                case "tolls": return BombableOutposts;
                case "food": return FoodTypes;
                case "weapons": return Weapons;
                case "weaponsnojelly": return [.. Weapons.Where(x => x != "JellyFish")];
                case "theft": return [.. StealableStolable, "DataPearl"];
                case "ban": return Bannable;
                case "friend": return Befriendable;
                case "pearls": return CollectablePearls;
                case "craft": return CraftableItems;
                case "regions": return ["Any Region", .. SlugcatStats.SlugcatStoryRegions(ExpeditionData.slugcatPlayer).Where(x => x.ToLowerInvariant() != "hr"), .. SlugcatStats.SlugcatOptionalRegions(ExpeditionData.slugcatPlayer)];
                case "regionsreal": return [.. SlugcatStats.SlugcatStoryRegions(ExpeditionData.slugcatPlayer).Where(x => x.ToLowerInvariant() != "hr"), .. SlugcatStats.SlugcatOptionalRegions(ExpeditionData.slugcatPlayer)];
                case "echoes": return [.. GhostWorldPresence.GhostID.values.entries.Where(x => x != "NoGhost")];
                case "creatures": return ["Any Creature", .. CreatureType.values.entries.Where(x => ChallengeTools.creatureSpawns[ExpeditionData.slugcatPlayer.value].Any(g => g.creature.value == x))];
                case "depths": return Depthable;
                case "banitem": return [.. FoodTypes, .. Bannable];
                case "unlocks": return [.. BingoData.possibleTokens[0], .. BingoData.possibleTokens[1], .. BingoData.possibleTokens[2], .. BingoData.possibleTokens[3]];
                case "chatlogs": return [.. BingoData.possibleTokens[4]];
                case "passage": return [.. WinState.EndgameID.values.entries.Where(x => x != "Mother" && x != "Gourmand")];
                case "expobject": return Storable;
                case "vista": // hate
                    List<ValueTuple<string, string>> list = new List<ValueTuple<string, string>>();
                    foreach (KeyValuePair<string, Dictionary<string, Vector2>> keyValuePair in BingoVistaLocations)
                    {
                        if (GetSortedCorrectListForChallenge("regionsreal").Contains(keyValuePair.Key))
                        {
                            foreach (KeyValuePair<string, Vector2> keyValuePair2 in keyValuePair.Value)
                            {
                                list.Add(new ValueTuple<string, string>(keyValuePair.Key, keyValuePair2.Key));
                            }
                        }
                    }
                    List<string> strings = [];
                    for (int i = 0; i < list.Count; i++)
                    {
                        strings.Add(list[i].Item2);
                    }
                    return strings.ToArray();
            }
            return ["Whoops something went wrong"];
        }

        public static string[] GetSortedCorrectListForChallenge(string listName)
        {
            return GetCorrectListForChallenge(listName).Distinct().ToArray().SortArray();
        }

        private static string[] SortArray(this string[] array)
        {
            List<string> list = array.ToList();
            list.Sort();
            return list.ToArray();
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
            if (type == ItemType.SSOracleSwarmer) return translator.Translate("Pebbles Neurons");
            if (type == ItemType.EggBugEgg) return translator.Translate("Eggbug Eggs");
            if (type == ItemType.WaterNut) return translator.Translate("Bubble Fruit");
            if (type == ItemType.SlimeMold) return translator.Translate("Slime Mold");
            if (type == ItemType.BubbleGrass) return translator.Translate("Bubble Grass");
            if (type == DLCItemType.GlowWeed) return translator.Translate("Glow Weed");
            if (type == DLCItemType.DandelionPeach) return translator.Translate("Dandelion Peaches");
            if (type == DLCItemType.LillyPuck) return translator.Translate("Lillypucks");
            if (type == DLCItemType.GooieDuck) return translator.Translate("Gooieducks");

            return orig.Invoke(type);
        }

        public static void ChallengeTools_CreatureName(On.Expedition.ChallengeTools.orig_CreatureName orig, ref string[] creatureNames)
        {
            orig.Invoke(ref creatureNames);
            creatureNames[(int)CreatureType.SmallNeedleWorm] = ChallengeTools.IGT.Translate("Small Noodleflies");
            creatureNames[(int)CreatureType.VultureGrub] = ChallengeTools.IGT.Translate("Vulture Grubs");
            creatureNames[(int)CreatureType.Hazer] = ChallengeTools.IGT.Translate("Hazers");
            creatureNames[(int)CreatureType.Salamander] = ChallengeTools.IGT.Translate("Salamanders");
            creatureNames[(int)CreatureType.Spider] = ChallengeTools.IGT.Translate("Coalescipedes");
            if (ModManager.MSC) creatureNames[(int)DLCSharedEnums.CreatureTemplateType.Yeek] = ChallengeTools.IGT.Translate("Yeeks");
        }

        public static List<string> CreatureOriginRegions(string type, SlugcatStats.Name slug)
        {
            List<string> r = [];
            switch (type)
            {
                case "CicadaA":
                case "CicadaB":
                    r.AddRange(["SU", "LF", "SI"]);
                    if (slug != MoreSlugcatsEnums.SlugcatStatsName.Rivulet) r.Add("VS");
                    break;
                case "Hazer":
                    r.AddRange(["HI", "GW", "SL", slug == MoreSlugcatsEnums.SlugcatStatsName.Saint ? "UG" : "DS", "LF"]);
                    break;
                case "VultureGrub":
                    r.AddRange(["HI", "GW", "CC", "LF"]);
                    break;
                case "JetFish":
                    r.Add((slug == MoreSlugcatsEnums.SlugcatStatsName.Artificer || slug == MoreSlugcatsEnums.SlugcatStatsName.Spear) ? "LM" : "SL");
                    break;
                case "Yeek":
                    r.Add("OE"); if (slug == MoreSlugcatsEnums.SlugcatStatsName.Saint || slug == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
                    {
                        r.AddRange(["SB", "LF"]);
                        r.Remove("OE");
                    }
                    break;
            }

            return r;
        }

        private static void FetchAllEnterableRegions()
        {
            string path = AssetManager.ResolveFilePath(Path.Combine("world", "regions.txt"));
            if (File.Exists(path)) 
            {
                AllEnterableRegions = File.ReadAllLines(path);
            } 
        }

        private static void FetchGatesFromFile()
        {
            List<string> gatesToAdd = [];
            string path = AssetManager.ResolveFilePath(Path.Combine("world", "gates", "enterableGateCombos.txt"));
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    try 
                    {
                        string actualLine = line;
                        if (line.StartsWith("MSC-"))
                        {
                            if (!ModManager.MSC) continue;
                            actualLine = line.Substring(4);
                        }
                        string[] gate = actualLine.Split('_');
                        string regionNames = gate[0] + "_" + gate[1];
                        gatesToAdd.Add(regionNames);
                        //
                    }
                    catch
                    {
                        Plugin.logger.LogError("Couldnt read gate " + line);
                    }
                }
            }
            AllGates = gatesToAdd.ToArray();
        }

        public static string[] AllGates = [];
        public static string[] AllEnterableRegions = [];

        public static readonly string[] Depthable = 
        {
            "Hazer",
            "VultureGrub",
            "SmallNeedleWorm",
            "TubeWorm",
            "SmallCentipede",
            "Snail",
            "LanternMouse",
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
            "Salamander",
            "Dropbug",
            "Snail",
            "Centipede",
            "Centiwing",
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
                case "OE":
                    return "Light Purple";
                case "DM":
                    return "Light Yellow";
                case "LC":
                    return "Deep Green";
                case "LC_second":
                    return "Bronze";
                case "SU_filt":
                    return "Light Pink";
                case "MS":
                    return "Dull Yellow";
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
            "SSOracleSwarmer",

            // MSC
            "GooieDuck",
            "LillyPuck",
            "DandelionPeach",
            "GlowWeed",

            // Crits
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
            "JellyFish",
            "PuffBall",
            "LillyPuck"
        };

        public static readonly string[] StealableStolable =
        {
            "Spear",
            "Rock",
            "ScavengerBomb",
            "Lantern",
            "GooieDuck",
            "GlowWeed"
        };

        public static readonly string[] Bannable =
        {
            "Lantern",
            "PuffBall",
            "VultureMask",
            "ScavengerBomb",
            "FirecrackerPlant",
            "BubbleGrass",
            "Rock",
            "DataPearl"
        };

        public static readonly string[] Befriendable =
        {
            "CicadaA",
            "CicadaB",
            "GreenLizard",
            "PinkLizard",
            "Salamander",
            "YellowLizard",
            "BlackLizard",
            "CyanLizard",
            "WhiteLizard",
            "BlueLizard",
            "EelLizard",
            "SpitLizard",
            "ZoopLizard",
            "RedLizard"
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
            "VS",
            "OE",
            "LC",
            "DM",
            "LC_second",
            "SU_filt",
            "MS"
        };

        public static readonly string[] CraftableItems =
        {
            "FlareBomb",
            "SporePlant",
            "ScavengerBomb",
            "JellyFish",
            "DataPearl",
            "BubbleGrass",
            "FlyLure",
            "SlimeMold",
            "FirecrackerPlant",
            "PuffBall",
            "Mushroom",
            "Lantern",
            "GlowWeed",
            "GooieDuck",
            "FireEgg",
        };

        public static readonly string[] Storable =
        {
            "FirecrackerPlant",
            "SporePlant",
            "FlareBomb",
            "FlyLure",
            "JellyFish",
            "Lantern",
            "Mushroom",
            "PuffBall",
            "ScavengerBomb",
            "VultureMask",
            "DangleFruit",
            "SlimeMold",
            "BubbleGrass",

            // MSC
            "GooieDuck",
            "LillyPuck",
            "DandelionPeach"
        };
    }
}
