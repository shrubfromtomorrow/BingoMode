using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Expedition;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;
using CreatureType = CreatureTemplate.Type;
using DLCItemType = DLCSharedEnums.AbstractObjectType;
using ItemType = AbstractPhysicalObject.AbstractObjectType;
using MSCItemType = MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType;
using WatcherItemType = Watcher.WatcherEnums.AbstractObjectType;
using SlugName = SlugcatStats.Name;

namespace BingoMode.BingoChallenges
{
    public static class ChallengeUtilsFiltering
    {
        public static readonly SlugName watchername = WatcherEnums.SlugcatStatsName.Watcher;
        public static readonly SlugName survivorname = SlugName.White;
        public static readonly SlugName monkname = SlugName.Yellow;
        public static readonly SlugName huntername = SlugName.Red;
        public static readonly SlugName artiname = MoreSlugcatsEnums.SlugcatStatsName.Artificer;
        public static readonly SlugName gourname = MoreSlugcatsEnums.SlugcatStatsName.Gourmand;
        public static readonly SlugName spearname = MoreSlugcatsEnums.SlugcatStatsName.Spear;
        public static readonly SlugName rivname = MoreSlugcatsEnums.SlugcatStatsName.Rivulet;
        public static readonly SlugName saintname = MoreSlugcatsEnums.SlugcatStatsName.Saint;

        
        public static string[] GetFilteredList(string listname, string[] origList, bool sorted)
        {
            string[] result = ListRules[listname](ExpeditionData.slugcatPlayer, origList);

            return sorted ? result.Distinct().OrderBy(x => x).ToArray() : result;
        }

        private static readonly Dictionary<string, Func<SlugName, string[], string[]>> ListRules = new()
        {
            {
                "transport",
                (slug, baselist) =>
                {
                    string[] watcherCreatures = { "Tardigrade", "Frog", "Rat" };
                    string[] watcherForbid = { "Yeek" };
                    string[] saintForbid = { "CicadaA", "CicadaB" };
                    string[] mscCreatures = { "Yeek" };
                    string[] hunterAllow = { "Jetfish" };
                    string[] spearHunterArtiForbid = { "Yeek" };

                    // mb lmao
                    return baselist
                        .Where(x =>
                            (slug == watchername ? !watcherForbid.Contains(x) : !watcherCreatures.Contains(x))

                            && (ModManager.MSC || !mscCreatures.Contains(x))

                            && (slug != saintname || !saintForbid.Contains(x))

                            && (slug == huntername || !hunterAllow.Contains(x))

                            && !( (slug == huntername || slug == spearname || slug == artiname)
                                  && spearHunterArtiForbid.Contains(x))
                        ).ToArray();
                }
            },
            {
                "pin",
                (slug, baselist) =>
                {
                    return new[] { "Any Creature" }.Concat(baselist).ToArray();
                }
            },
            {
                "tolls",
                (slug, baseList) =>
                {
                    if (slug == watchername) return baseList.Skip(baseList.Length - 3).ToArray();

                    int end = baseList.Length - 3;

                    if (!(ModManager.MSC && slug == saintname)) end -= 1;

                    return baseList.Take(end).ToArray();
                }
            },
            {
                "food",
                (slug, baseList) =>
                {
                    List<string> mutableBase = baseList.ToList();

                    string[] mscFoods = { "GooieDuck", "LillyPuck", "DandelionPeach", "GlowWeed" };
                    string[] watcherFoods = { "FireSpriteLarva", "Rat", "Tardigrade", "SandGrub", "Frog", "Barnacle" };

                    if (slug == watchername) mutableBase = mutableBase.Where(x => x != "SSOracleSwarmer").ToList();
                    if (!ModManager.MSC) mutableBase = mutableBase.Where(x => !mscFoods.Contains(x) || slug == watchername).ToList();

                    if (slug == monkname ||
                        slug == survivorname ||
                        slug == huntername ||
                        slug == rivname ||
                        slug == gourname)
                    {
                        mutableBase = mutableBase.Where(x => !watcherFoods.Contains(x)).ToList();
                    }

                    if (slug == spearname || slug == artiname)
                        mutableBase = mutableBase.Where(x => !watcherFoods.Contains(x) && x != "GlowWeed").ToList();

                    if (slug == saintname)
                        mutableBase = mutableBase.Where(x =>
                            !watcherFoods.Contains(x) &&
                            x != "EggBugEgg" &&
                            x != "DandelionPeach" &&
                            x != "SSOracleSwarmer" &&
                            x != "SmallNeedleWorm").ToList();

                    return mutableBase.ToArray();
                }
            },
            {
                "weapons",
                (slug, baselist) =>
                {
                    List<string> mutableBase = baselist.ToList();

                    string[] mscWeapons = { "LillyPuck" };
                    string[] watcherWeapons = { "Boomerang", "Frog", "GraffitiBomb" };

                    if (slug != watchername) mutableBase = mutableBase.Where(x => !watcherWeapons.Contains(x)).ToList();

                    if (!ModManager.MSC) mutableBase = mutableBase.Where(x => !mscWeapons.Contains(x) || slug == watchername).ToList();

                    return mutableBase.ToArray();

                }
            },
            {
                "weaponsnojelly",
                (slug, baselist) =>
                {
                    List<string> mutableBase = baselist.ToList();

                    string[] exclusions = { "JellyFish", "Frog" };

                    mutableBase = mutableBase.Where(x => !exclusions.Contains(x)).ToList();

                    return mutableBase.ToArray();

                }
            },
            {
                "theft",
                (slug, baselist) =>
                {
                    List<string> mutableBase = baselist.ToList();

                    string[] watcherItems = { "Boomerang", "GraffitiBomb" };
                    string[] mscItms = { "GooieDuck", "GlowWeed" };

                    if (slug != watchername) mutableBase = mutableBase.Where(x => !watcherItems.Contains(x)).ToList();
                    
                    if (!ModManager.MSC) mutableBase = mutableBase.Where(x => !mscItms.Contains(x) || slug == watchername).ToList();

                    return mutableBase.ToArray();
                }
            },
            {
                "friend",
                (slug, baselist) =>
                {
                    List<string> mutableBase = baselist.ToList();

                    string[] mscFriends = { "EelLizard", "SpitLizard" };
                    string[] watcherFriends = { "PeachLizard", "IndigoLizard", "BlizzardLizard", "BasiliskLizard" };
                    string[] saintFriends = { "ZoopLizard" };

                    mutableBase = mutableBase.Where(x => x != "ZoopLizard" || slug == saintname || slug == watchername).ToList();

                    if (slug != watchername) mutableBase = mutableBase.Where(x => !watcherFriends.Contains(x)).ToList();

                    if (!ModManager.MSC) mutableBase = mutableBase.Where(x => !mscFriends.Contains(x) || slug == saintname || slug == watchername).ToList();

                    return mutableBase.ToArray();

                }
            },
            {
                "pearls",
                (slug, baselist) =>
                {
                    List<string> mutableBase = baselist.ToList();

                    string[] noArtiSpearPearls = { "SL_chimney", "SL_bridge", "SL_moon" };
                    string[] watcherPearls = { "WORA_WORA",
                        "WAUA_WAUA",
                        "WPTA_DRONE",
                        "WSKC_ABSTRACT",
                        "WBLA_AUDIO_VOICEWIND1",
                        "WARE_AUDIO_VOICEWIND2",
                        "WTDA_AUDIO_JAM1",
                        "WSKD_AUDIO_JAM2",
                        "WTDB_AUDIO_JAM3",
                        "WRFB_AUDIO_JAM4",
                        "WARG_AUDIO_GROOVE",
                        "WAUA_TEXT_AUDIO_TALKSHOW",
                        "WMPA_TEXT_NOTIONOFSELF",
                        "WARB_TEXT_SECRET",
                        "WARC_TEXT_CONTEMPT",
                        "WARD_TEXT_STARDUST",
                        "WVWA_TEXT_KITESDAY"
                    };
                    string[] mscPearls = { "SI_chat3",
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

                    if (slug != watchername) mutableBase = mutableBase.Where(x => !watcherPearls.Contains(x)).ToList();
                    else mutableBase = mutableBase.Where(x => watcherPearls.Contains(x)).ToList();

                    if (!ModManager.MSC) mutableBase = mutableBase.Where(x => !mscPearls.Contains(x)).ToList();

                    if (slug == artiname || slug == spearname) mutableBase = mutableBase.Where(x => !noArtiSpearPearls.Contains(x)).ToList();
                    

                    return mutableBase.ToArray();

                }
            },
            {
                "craft",
                (slug, baselist) =>
                {
                    return baselist;
                }
            },
            {
                "regions",
                (slug, baselist) =>
                {
                    return new[] { "Any Region" }.Concat(SlugcatStats.SlugcatStoryRegions(ExpeditionData.slugcatPlayer).Where(x => x.ToLowerInvariant() != "hr"))
                    .Concat(SlugcatStats.SlugcatOptionalRegions(ExpeditionData.slugcatPlayer)).ToArray();
                }
            },
            {
                "regionsreal",
                (slug, baselist) =>
                {
                    return SlugcatStats.SlugcatStoryRegions(ExpeditionData.slugcatPlayer).Where(x => x.ToLowerInvariant() != "hr").Concat(SlugcatStats.SlugcatOptionalRegions(ExpeditionData.slugcatPlayer)).ToArray();
                }
            },
            {
                "nootregions",
                (slug, baselist) =>
                {
                    List<string> mutableBase = baselist.ToList();

                    string[] noShelterRegions = { "WRSA" };

                    return mutableBase.Where(x => !noShelterRegions.Contains(x)).ToArray();
                }
            },
            {
                "popcornRegions",
                (slug, baselist) =>
                {
                    List<string> mutableBase = baselist.ToList();

                    string[] excludedPopcornRegions = { "DS", "SH", "UW", "UG", "WARD", "WRFA", "WTDB", "WVWB", "WARE", "WPGA", "WRRA", "WPTA", "WSKC", "WSKA", "WTDA", "WVWA", "WARA", "WAUA", "WRSA", "WSSR" };

                    return mutableBase.Where(x => !excludedPopcornRegions.Contains(x)).ToArray();
                }
            },
            {
                "pomegranateRegions",
                (slug, baselist) =>
                {
                    return baselist;
                }
            },
            {
                "echoes",
                (slug, baseList) =>
                {
                    string[] allowedRegions = ChallengeUtils.GetCorrectListForChallenge("regionsreal", true);

                    return GhostWorldPresence.GhostID.values.entries
                        .Where(ghost =>
                            ghost != "NoGhost"
                            && ghost != "SpinningTop"

                            && (ModManager.MSC || ghost != "MS")

                            && (ghost != "SL" || (ModManager.MSC && slug == saintname))

                            && allowedRegions.Contains(ghost)
                        ).ToArray();
                }
            },
            {
                "spinners",
                (slug, baselist) =>
                {
                    //string[] spinningTopRegions = { "WARF", "WTDB", "WBLA", "WRFB", "WTDA", "WARE", "WSKC", "WVWA", "WPTA", "WARC", "WARB", "WVWB", "WARA", "WAUA" };

                    return ChallengeUtils.watcherSTSpots.Select(x => Regex.Split(Regex.Split(x, "-")[0], "_")[0]).Distinct().ToArray();
                }
            },
            {
                "weavers",
                (slug, baselist) =>
                {
                    return ChallengeUtils.watcherDWTSpots.Where(room => Regex.Split(room, "_")[0] != "WORA").ToArray();
                }
            },
            {
                "creatures",
                (slug, baselist) =>
                {
                    var allowed = CreatureType.values.entries.Where(x => ChallengeTools.creatureSpawns[slug.value].Any(g => g.creature.value == x)).Select(x => x.ToString());

                    return new[] { "Any Creature" }.Concat(allowed).ToArray();
                }
            },
            {
                "depths",
                (slug, baselist) =>
                {
                    return baselist;
                }
            },
            {
                "banitem",
                (slug, baselist) =>
                {
                    List<string> mutableBase = baselist.ToList();

                    string[] watcherBanItems = { "Boomerang", "GraffitiBomb" };

                    if (slug != watchername) mutableBase = mutableBase.Where(x => !watcherBanItems.Contains(x)).ToList();

                    return ChallengeUtils.GetCorrectListForChallenge("food").Concat(mutableBase).ToArray();
                }
            },
            {
                "unlocks",
                (slug, baselist) =>
                {
                    return BingoData.possibleTokens[0].Concat(BingoData.possibleTokens[1]).Concat(BingoData.possibleTokens[2]).Concat(BingoData.possibleTokens[3]).ToArray();
                }
            },
            {
                "chatlogs",
                (slug, baselist) =>
                {
                    return BingoData.possibleTokens[4].ToArray();
                }
            },
            {
                "passage",
                (slug, baselist) =>
                {
                    List<string> mutableBase =  WinState.EndgameID.values.entries;

                    string[] exclusions = { "Mother", "Gourmand" };
                    string[] watcherForbidPassages = { "Nomad", "Pilgrim", "Scholar", "Traveller", "Survivor" };

                    mutableBase = mutableBase.Where(x => !exclusions.Contains(x)).ToList();

                    if (slug == watchername) mutableBase = mutableBase.Where(x => !watcherForbidPassages.Contains(x)).ToList();

                    return mutableBase.ToArray();
                }
            },
            {
                "storable",
                (slug, baseList) =>
                {
                    string[] watcherItems = { "Boomerang", "GraffitiBomb", "FireSpriteLarva" };
                    string[] mscItems     = { "GooieDuck", "LillyPuck", "DandelionPeach" };

                    return baseList
                        .Where(x =>
                            (ModManager.MSC || !mscItems.Contains(x)) &&
                            (ModManager.Watcher || !watcherItems.Contains(x))

                            && !(ModManager.MSC &&
                                 (slug == artiname || slug == spearname) &&
                                 x == "BubbleGrass")

                            && !(ModManager.MSC && slug == saintname &&
                                 (x == "LillyPuck" ||
                                  x == "EggBugEgg" ||
                                  x == "SmallNeedleWorm" ||
                                  x == "SSOracleSwarmer"))
                        )
                        .ToArray();
                }
            },
            {
                "vista",
                (slug, baselist) =>
                {
                    List<ValueTuple<string, string>> list = new List<ValueTuple<string, string>>();
                    foreach (KeyValuePair<string, Dictionary<string, Vector2>> keyValuePair in ChallengeUtils.BingoVistaLocations)
                    {
                        if (ChallengeUtils.GetCorrectListForChallenge("regionsreal", true).Contains(keyValuePair.Key))
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
            },
        };
    }
}
