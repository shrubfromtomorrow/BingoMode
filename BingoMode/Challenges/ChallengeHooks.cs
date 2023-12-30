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

namespace BingoMode.Challenges
{
    internal class ChallengeHooks
    {
        public static void Apply()
        {
            // Eated
            On.Player.ObjectEaten += Player_ObjectEaten;

            // Sandbox unlock getting
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_string_bool += MiscProgressionData_GetTokenCollected;
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_SafariUnlockID += MiscProgressionData_GetTokenCollected_SafariUnlockID;
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_SlugcatUnlockID += MiscProgressionData_GetTokenCollected_SlugcatUnlockID;
            // Unlock sandbox unlocks when a challenge contains them
            IL.Room.Loaded += Room_Loaded;
            // Special token color for challenge sandbox unlocks
            Hook tokenColorHook = new (typeof(CollectToken).GetProperty("TokenColor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetGetMethod(), typeof(ChallengeHooks).GetMethod("CollectToken_TokenColor_get", BindingFlags.Static | BindingFlags.Public));
            // Complete challenge
            On.CollectToken.Pop += CollectToken_Pop;

            // Damage with wepon shit
            On.SocialEventRecognizer.WeaponAttack += SocialEventRecognizer_WeaponAttack;
            On.PhysicalObject.HitByExplosion += PhysicalObject_HitByExplosion;
            IL.SporeCloud.Update += SporeCloud_Update;
            On.SporePlant.Bee.Attach += Bee_Attach;
            IL.JellyFish.Collide += JellyFish_Collide;

            // Theft ing
            On.ScavengerOutpost.PlayerTracker.Update += PlayerTracker_Update;
            On.SocialEventRecognizer.Theft += SocialEventRecognizer_Theft;

            // Trading
            IL.ScavengerAI.RecognizeCreatureAcceptingGift += ScavengerAI_RecognizeCreatureAcceptingGift;

            // Popcorn
            IL.SeedCob.HitByWeapon += SeedCob_HitByWeapon;

            // Green neuron
            IL.SaveState.ctor += SaveState_ctor;
            On.SLOracleWakeUpProcedure.NextPhase += SLOracleWakeUpProcedure_NextPhase;
            On.SSOracleBehavior.SSOracleGetGreenNeuron.ctor += SSOracleGetGreenNeuron_ctor;

            // Dont use item
            On.Player.ThrowObject += Player_ThrowObject;
            On.Player.GrabUpdate += Player_GrabUpdate;

            // Tame creature
            On.FriendTracker.Update += FriendTracker_Update;

            // Region entering
            On.WorldLoader.ctor_RainWorldGame_Name_bool_string_Region_SetupValues += WorldLoader_ctor_RainWorldGame_Name_bool_string_Region_SetupValues;

            // Dodging leviathans

        }

        public static void FriendTracker_Update(On.FriendTracker.orig_Update orig, FriendTracker self)
        {
            orig.Invoke(self);

            // Copied from base game
            if (BingoData.BingoMode && self.AI.creature.state.socialMemory != null && self.AI.creature.state.socialMemory.relationShips != null && self.AI.creature.state.socialMemory.relationShips.Count > 0)
            {
                for (int j = 0; j < self.AI.creature.state.socialMemory.relationShips.Count; j++)
                {
                    if (self.AI.creature.state.socialMemory.relationShips[j].like > 0.5f && self.AI.creature.state.socialMemory.relationShips[j].tempLike > 0.5f)
                    {
                        for (int k = 0; k < self.AI.creature.Room.creatures.Count; k++)
                        {
                            if (self.AI.creature.Room.creatures[k].ID == self.AI.creature.state.socialMemory.relationShips[j].subjectID && self.AI.creature.Room.creatures[k].realizedCreature != null)
                            {
                                for (int p = 0; p < ExpeditionData.challengeList.Count; p++)
                                {
                                    if (ExpeditionData.challengeList[p] is BingoTameChallenge c)
                                    {
                                        c.Fren(self.AI.creature.creatureTemplate.type);
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
                return;
            }
        }

        public static void WorldLoader_ctor_RainWorldGame_Name_bool_string_Region_SetupValues(On.WorldLoader.orig_ctor_RainWorldGame_Name_bool_string_Region_SetupValues orig, WorldLoader self, RainWorldGame game, SlugcatStats.Name playerCharacter, bool singleRoomWorld, string worldName, Region region, RainWorldGame.SetupValues setupValues)
        {
            orig.Invoke(self, game, playerCharacter, singleRoomWorld, worldName, region, setupValues);

            if (BingoData.BingoMode)
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoNoRegionChallenge c)
                    {
                        c.Entered(worldName);
                    }

                    if (ExpeditionData.challengeList[j] is BingoAllRegionsExcept r)
                    {
                        r.Entered(worldName);
                    }
                }
            } 
        }

        public static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig.Invoke(self, eu);

            if (BingoData.BingoMode) 
            {
                for (int i = 0; i < self.grasps.Length; i++)
                {
                    // This is so if you dual wield something it doesnt add time twice
                    ItemType ignore = null;

                    if (self.grasps[i] != null)
                    {
                        ItemType heldType = self.grasps[i].grabbed.abstractPhysicalObject.type;
                        if (heldType != ignore)
                        {
                            ignore = heldType;
                            BingoData.heldItemsTime[(int)heldType]++;
                        }
                    }
                }
            }
        }

        public static void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
        {
            if (BingoData.BingoMode && self.grasps[grasp] != null && self.grasps[grasp].grabbed is not Creature)
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoDontUseItemChallenge c && !c.isFood)
                    {
                        c.Used(self.grasps[grasp].grabbed.abstractPhysicalObject.type);
                    }
                }
            }

            orig.Invoke(self, grasp, eu);
        }

        public static void SSOracleGetGreenNeuron_ctor(On.SSOracleBehavior.SSOracleGetGreenNeuron.orig_ctor orig, SSOracleBehavior.SSOracleGetGreenNeuron self, SSOracleBehavior owner)
        {
            orig.Invoke(self, owner);

            if (BingoData.BingoMode)
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoGreenNeuronChallenge c && !c.moon)
                    {
                        c.Delivered();
                    }
                }
            }
        }

        public static void SLOracleWakeUpProcedure_NextPhase(On.SLOracleWakeUpProcedure.orig_NextPhase orig, SLOracleWakeUpProcedure self)
        {
            orig.Invoke(self);

            if (BingoData.BingoMode && self.phase == SLOracleWakeUpProcedure.Phase.GoToRoom || self.phase == SLOracleWakeUpProcedure.Phase.GoToAboveOracle)
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoGreenNeuronChallenge c && c.moon)
                    {
                        c.Delivered();
                    }
                }
            }
        }

        public static void SaveState_ctor(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(
                x => x.MatchCallOrCallvirt<RainWorld>("get_ExpeditionMode")
                ) && c.TryGotoNext(
                x => x.MatchCallOrCallvirt<SLOrcacleState>("set_neuronsLeft")   
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<int, SaveState, int>>((orig, self) =>
                {
                    if (BingoData.BingoMode && BingoData.MoonDead) orig = 0;

                    return orig;
                });
            }
            else Plugin.logger.LogError("Uh oh, SaveState_ctor il fucked up " + il);
        }

        public static void SeedCob_HitByWeapon(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(
                x => x.MatchCallOrCallvirt<SeedCob>("Open")
                ))
            {
                c.Index++;
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Action<Weapon>>((weapon) =>
                {
                    if (BingoData.BingoMode && weapon.thrownBy != null && weapon.thrownBy is Player p)
                    {
                        for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                        {
                            if (ExpeditionData.challengeList[j] is BingoPopcornChallenge c)
                            {
                                c.Pop();
                            }
                        }
                    }
                });
            }
            else Plugin.logger.LogError("Uh oh, SeedCob_HitByWeapon il fucked up " + il);
        }

        public static void ScavengerAI_RecognizeCreatureAcceptingGift(ILContext il)
        {
            ILCursor c = new(il);
        
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<ItemTracker>("RepresentationForObject")
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg, 4);
                c.Emit(OpCodes.Ldloc, 15);
                c.EmitDelegate<Action<ScavengerAI, PhysicalObject, int>>((self, item, i) =>
                {
                    if (BingoData.BingoMode && self.tradeSpot != null && (self.creature.abstractAI as ScavengerAbstractAI).squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.Trade)
                    {
                        Plugin.logger.LogMessage("GURG");
                        for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                        {
                            if (ExpeditionData.challengeList[j] is BingoTradeChallenge c)
                            {
                                c.Traded(self.CollectScore(item, false), item.abstractPhysicalObject.ID);
                            }
                            else if (ExpeditionData.challengeList[j] is BingoTradeTradedChallenge t)
                            {
                                EntityID id = self.scavenger.room.socialEventRecognizer.ownedItemsOnGround[i].item.abstractPhysicalObject.ID;
                                EntityID id2 = item.abstractPhysicalObject.ID;
                                //Plugin.logger.LogMessage($"{id} - {id2}");
                                if (!t.traderItems.Contains(id) && id != id2) t.traderItems.Add(id);
                                t.Traded(id2);
                            }
                        }
                    }
                });
            }
            else Plugin.logger.LogError("Uh oh, ScavengerAI_RecognizeCreatureAcceptingGift il fucked up " + il);
        }

        public static void PlayerTracker_Update(On.ScavengerOutpost.PlayerTracker.orig_Update orig, ScavengerOutpost.PlayerTracker self)
        {
            orig.Invoke(self);

            if (BingoData.BingoMode)
            {
                for (int j = 0; j < self.player.realizedCreature.grasps.Length; j++)
                {
                    if (self.player.realizedCreature.grasps[j] != null)
                    {
                        int k = 0;
                        while (k < self.outpost.outPostProperty.Count)
                        {
                            if (self.player.realizedCreature.grasps[j].grabbed.abstractPhysicalObject.ID == self.outpost.outPostProperty[k].ID)
                            {
                                Plugin.logger.LogMessage("Stefth!!");
                                foreach (var g in self.outpost.outPostProperty) Plugin.logger.LogMessage("Property: " + g);
                                bool gruh = false;
                                for (int w = 0; w < ExpeditionData.challengeList.Count; w++)
                                {
                                    if (ExpeditionData.challengeList[w] is BingoStealChallenge c)
                                    {
                                        Plugin.logger.LogMessage("Stoled report " + self.outpost.outPostProperty[k].type);
                                        c.Stoled(self.outpost.outPostProperty[k], true);
                                        gruh = true;
                                    }
                                }
                                if (gruh) break;
                            }
                            k++;
                        }
                    }
                }
            }
        }

        public static void SocialEventRecognizer_Theft(On.SocialEventRecognizer.orig_Theft orig, SocialEventRecognizer self, PhysicalObject item, Creature theif, Creature victim)
        {
            orig.Invoke(self, item, theif, victim);

            if (BingoData.BingoMode && theif is Player && victim is Scavenger)
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoStealChallenge c)
                    {
                        c.Stoled(item.abstractPhysicalObject, false);
                    }
                }
            }
        }

        public static void Bee_Attach(On.SporePlant.Bee.orig_Attach orig, SporePlant.Bee self, BodyChunk chunk)
        {
            if (BingoData.BingoMode && chunk.owner is Creature victim && !victim.dead)
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoDamageChallenge c)
                    {
                        c.Hit(self.owner.abstractPhysicalObject.type, victim, self.owner);
                    }
                }
            }

            orig.Invoke(self, chunk);
        }

        public static void SporeCloud_Update(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(
                x => x.MatchCallOrCallvirt<AbstractCreature>("get_realizedCreature")
                ))
            {
                c.Index += 3;
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_3);
                c.EmitDelegate<Action<SporeCloud, int>>((self, i) =>
                {
                    Creature victim = self.room.abstractRoom.creatures[i].realizedCreature;
                    if (BingoData.BingoMode && !victim.dead && Custom.DistLess(self.pos, victim.mainBodyChunk.pos, self.rad + victim.mainBodyChunk.rad + 20f))
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
                        c.Hit(explosion.sourceObject.abstractPhysicalObject.type, victim, explosion);
                    }
                }
            }
        }

        public static void SocialEventRecognizer_WeaponAttack(On.SocialEventRecognizer.orig_WeaponAttack orig, SocialEventRecognizer self, PhysicalObject weapon, Creature thrower, Creature victim, bool hit)
        {
            orig.Invoke(self, weapon, thrower, victim, hit);

            if (BingoData.BingoMode && weapon is not PuffBall && weapon is not SporePlant && thrower is Player && hit && !victim.dead)
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

        public static void JellyFish_Collide(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(
                x => x.MatchLdarg(1),
                x => x.MatchIsinst<BigEel>()
                ))
            {
                c.Index++;
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Action<JellyFish, PhysicalObject>>((self, obj) =>
                {
                    if (BingoData.BingoMode)
                    {
                        for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                        {
                            if (ExpeditionData.challengeList[j] is BingoDamageChallenge c)
                            {
                                c.Hit(self.abstractPhysicalObject.type, obj as Creature);
                            }
                        }
                    }
                });
            }
            else Plugin.logger.LogMessage("JellyFish_Collide failed! " + il);
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
            if (self.placedObj.data is CollectToken.CollectTokenData d && BingoData.challengeTokens.Contains(d.tokenString) && BingoHooks.GlobalBoard.AllChallenges.Any(x => x is BingoUnlockChallenge b && b.unlock == d.tokenString))
            {
                foreach (Challenge ch in BingoHooks.GlobalBoard.AllChallenges)
                {
                    if (ch is BingoUnlockChallenge b && b.unlock == (d.tokenString + (d.isRed ? "-safari" : "")))
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

            ILCursor b = new(il);
            if (b.TryGotoNext(
                x => x.MatchLdsfld("Expedition.ExpeditionData", "startingDen")
                ) &&
                b.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<WorldCoordinate>(".ctor")
                ))
            {
                b.Emit(OpCodes.Ldarg_0);
                b.Emit(OpCodes.Ldloc, 72);
                b.EmitDelegate<Action<Room, WorldCoordinate>>((room, pos) =>
                {
                    if (BingoData.BingoMode && BingoHooks.GlobalBoard.AllChallenges.Any(x => x is BingoGreenNeuronChallenge))
                    {
                        AbstractPhysicalObject startItem = new (room.world, ItemType.NSHSwarmer, null, pos, room.game.GetNewID());
                        room.abstractRoom.entities.Add(startItem);
                        startItem.Realize();
                        //Player player = room.game.Players[0].realizedCreature as Player;
                        //if (player != null && startItem.realizedObject != null)
                        //{
                        //    startItem.realizedObject.firstChunk.HardSetPosition(player.mainBodyChunk.pos + new Vector2(-30f, 0f));
                        //    player.SlugcatGrab(startItem.realizedObject, 0);
                        //}
                    }
                });
            }
            else Plugin.logger.LogError("Ass " + il);
        }

        // Register food for the eat food challenge if its on
        public static void Player_ObjectEaten(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible edible)
        {
            orig.Invoke(self, edible);
            if (BingoData.BingoMode)
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoEatChallenge c)
                    {
                        c.FoodEated(edible);
                    }
                    else if (ExpeditionData.challengeList[j] is BingoDontUseItemChallenge g && g.isFood && edible is PhysicalObject p)
                    {
                        g.Used(p.abstractPhysicalObject.type);
                    }
                }
            }
        }
    }
}
