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
using System.Linq;
using System.Reflection;
using MonoMod.RuntimeDetour;
using static Rewired.Controller;

namespace BingoMode.Challenges
{
    internal class ChallengeHooks
    {
        public static void Apply()
        {
            // Eated
            On.Player.ObjectEaten += Player_ObjectEaten;

            // Item and creatuer name utils
            On.Expedition.ChallengeTools.ItemName += ChallengeTools_ItemName;
            On.Expedition.ChallengeTools.CreatureName += ChallengeTools_CreatureName;

            // Sandbox unlock getting
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_string_bool += MiscProgressionData_GetTokenCollected;
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_SafariUnlockID += MiscProgressionData_GetTokenCollected_SafariUnlockID;
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_SlugcatUnlockID += MiscProgressionData_GetTokenCollected_SlugcatUnlockID;
            // Unlock sandbox unlocks when a challenge contains them
            IL.Room.Loaded += Room_Loaded;
            // Special token color for challenge sandbox unlocks
            Hook tokenColorHook = new Hook(typeof(CollectToken).GetProperty("TokenColor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetGetMethod(), typeof(ChallengeHooks).GetMethod("CollectToken_TokenColor_get", BindingFlags.Static | BindingFlags.Public));
            // Complete challenge
            On.CollectToken.Pop += CollectToken_Pop;

            // Damage with wepon shit
            On.SocialEventRecognizer.WeaponAttack += SocialEventRecognizer_WeaponAttack;
            On.PhysicalObject.HitByExplosion += PhysicalObject_HitByExplosion;
            IL.SporeCloud.Update += SporeCloud_Update;
        }

        public static void SporeCloud_Update(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(
                x => x.MatchLdfld<InsectoidCreature>("poison"),
                x => x.MatchLdcR4(0.025f)
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_3);
                c.EmitDelegate<Action<SporeCloud, int>>((self, i) =>
                {
                    Creature victim = self.room.abstractRoom.creatures[i].realizedCreature;
                    if (BingoData.BingoMode && !victim.dead)
                    {
                        for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                        {
                            if (ExpeditionData.challengeList[j] is BingoDamageChallenge c)
                            {
                                c.Hit(ItemType.PuffBall, victim, self);
                            }
                        }
                    }
                });
            }
            else Plugin.logger.LogError("Uh oh, SporeCloud_Update il fucked up " + il);
        }

        public static void PhysicalObject_HitByExplosion(On.PhysicalObject.orig_HitByExplosion orig, PhysicalObject self, float hitFac, Explosion explosion, int hitChunk)
        {
            orig.Invoke(self, hitFac, explosion, hitChunk);

            if (BingoData.BingoMode && self is Creature victim && !victim.dead)
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoDamageChallenge c)
                    {
                        c.Hit(ItemType.ScavengerBomb, victim, self);
                    }
                }
            }
        }

        public static void SocialEventRecognizer_WeaponAttack(On.SocialEventRecognizer.orig_WeaponAttack orig, SocialEventRecognizer self, PhysicalObject weapon, Creature thrower, Creature victim, bool hit)
        {
            orig.Invoke(self, weapon, thrower, victim, hit);

            if (BingoData.BingoMode && thrower is Player && hit && !victim.dead)
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoDamageChallenge c)
                    {
                        c.Hit(weapon.abstractPhysicalObject.type, victim);
                    }
                }
            }
        }

        public static bool MiscProgressionData_GetTokenCollected(On.PlayerProgression.MiscProgressionData.orig_GetTokenCollected_string_bool orig, PlayerProgression.MiscProgressionData self, string tokenString, bool sandbox)
        {
            if (ModManager.Expedition && BingoData.BingoMode && BingoData.challengeTokens.Contains(tokenString)) return false;
            return orig.Invoke(self, tokenString, sandbox);
        }

        public static bool MiscProgressionData_GetTokenCollected_SlugcatUnlockID(On.PlayerProgression.MiscProgressionData.orig_GetTokenCollected_SlugcatUnlockID orig, PlayerProgression.MiscProgressionData self, MultiplayerUnlocks.SlugcatUnlockID classToken)
        {
            if (ModManager.Expedition && BingoData.BingoMode && BingoData.challengeTokens.Contains(classToken.value)) return false;
            return orig.Invoke(self, classToken);
        }

        public static bool MiscProgressionData_GetTokenCollected_SafariUnlockID(On.PlayerProgression.MiscProgressionData.orig_GetTokenCollected_SafariUnlockID orig, PlayerProgression.MiscProgressionData self, MultiplayerUnlocks.SafariUnlockID safariToken)
        {
            if (ModManager.Expedition && BingoData.BingoMode && BingoData.challengeTokens.Contains(safariToken.value)) return false;
            return orig.Invoke(self, safariToken);
        }

        public static void CollectToken_Pop(On.CollectToken.orig_Pop orig, CollectToken self, Player player)
        {
            if (self.expand > 0f)
            {
                return;
            }
            if (self.placedObj.data is CollectToken.CollectTokenData d && BingoData.challengeTokens.Contains(d.tokenString) && BingoHooks.GlobalBoard.AllChallengeList().Any(x => x is BingoUnlockChallenge b && b.unlock == d.tokenString))
            {
                foreach (Challenge ch in BingoHooks.GlobalBoard.AllChallengeList())
                {
                    if (ch is BingoUnlockChallenge b && b.unlock == d.tokenString)
                    {
                        ch.CompleteChallenge();
                        if (BingoData.challengeTokens.Contains(d.tokenString)) BingoData.challengeTokens.Remove(d.tokenString);
                    }
                }

                self.expandAroundPlayer = player;
                self.expand = 0.01f;
                self.room.PlaySound(SoundID.Token_Collect, self.pos);

                int num = 0;
                while ((float)num < 10f)
                {
                    self.room.AddObject(new CollectToken.TokenSpark(self.pos + Custom.RNV() * 2f, Custom.RNV() * 11f * UnityEngine.Random.value + Custom.DirVec(player.mainBodyChunk.pos, self.pos) * 5f * UnityEngine.Random.value, self.GoldCol(self.glitch), self.underWaterMode));
                    num++;
                }
            }
            else orig.Invoke(self, player);
        }

        public delegate Color orig_TokenColor(CollectToken self);
        public static Color CollectToken_TokenColor_get(orig_TokenColor orig, CollectToken self)
        {
            if (self.placedObj.data is CollectToken.CollectTokenData d && BingoData.challengeTokens.Contains(d.tokenString)) return new Color(1f,1f,1f,1f);
            else return orig.Invoke(self);
        }

        public static void Room_Loaded(ILContext il)
        {
            ILCursor c = new(il);
        
            if (c.TryGotoNext(
                x => x.MatchLdfld("PlacedObject", "active")
                ) && 
                c.TryGotoNext(
                x => x.MatchLdsfld("ModManager", "Expedition")
                ))
            {
                c.Index++;
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc, 21);
                c.EmitDelegate<Func<bool, Room, int, bool>>((orig, self, i) =>
                {
                    if (BingoData.BingoMode && self.roomSettings.placedObjects[i].data is CollectToken.CollectTokenData c && BingoData.challengeTokens.Contains(c.tokenString)) orig = false;
                    return orig;
                });
            }
            else Plugin.logger.LogMessage("Challenge room loaded threw!!! " + il);
        }

        public static void ChallengeTools_CreatureName(On.Expedition.ChallengeTools.orig_CreatureName orig, ref string[] creatureNames)
        {
            orig.Invoke(ref creatureNames);
            creatureNames[(int)CreatureType.SmallNeedleWorm] = ChallengeTools.IGT.Translate("Small Noodleflies");
            creatureNames[(int)CreatureType.VultureGrub] = ChallengeTools.IGT.Translate("Vulture Grubs");
            creatureNames[(int)CreatureType.Hazer] = ChallengeTools.IGT.Translate("Hazers");
        }

        // Register food for the eat food challenge if its on
        public static void Player_ObjectEaten(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible edible)
        {
            orig.Invoke(self, edible);
            if (ModManager.Expedition && self.room.game.rainWorld.ExpeditionMode)
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoEatChallenge c)
                    {
                        c.FoodEated(edible);
                    }
                }
            }
        }

        public static string ChallengeTools_ItemName(On.Expedition.ChallengeTools.orig_ItemName orig, AbstractPhysicalObject.AbstractObjectType type)
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
            if (type == MSCItemType.GlowWeed) return translator.Translate("Glow Weed");
            if (type == MSCItemType.DandelionPeach) return translator.Translate("Dandelion Peaches");
            if (type == MSCItemType.LillyPuck) return translator.Translate("Lillypucks");
            if (type == MSCItemType.GooieDuck) return translator.Translate("Gooieducks");

            return orig.Invoke(type);
        }
    }
}
