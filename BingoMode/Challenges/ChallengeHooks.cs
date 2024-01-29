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
    public interface IBingoChallenge
    {
        void AddHooks();
        void RemoveHooks();
    }

    public class ChallengeHooks
    {
        // Runtime detour hooks
        public static Hook tokenColorHook;

        // Normal/IL hooks
        public static void Apply()
        {
            // Clearing stuff that needs to be cleared at the end of the cycle
            On.SaveState.SessionEnded += ClearBs;
        }

        public static bool LillyPuck_HitSomething(On.MoreSlugcats.LillyPuck.orig_HitSomething orig, LillyPuck self, SharedPhysics.CollisionResult result, bool eu)
        {
            if (self.thrownBy is Player && result.obj is Creature victim)
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoDamageChallenge c)
                    {
                        c.Hit(self.abstractPhysicalObject.type, victim);
                    }
                }
            }

            return orig.Invoke(self, result, eu);
        }

        public static bool Rock_HitSomething(On.Rock.orig_HitSomething orig, Rock self, SharedPhysics.CollisionResult result, bool eu)
        {
            if (self.thrownBy is Player && result.obj is Creature victim)
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoDamageChallenge c)
                    {
                        c.Hit(self.abstractPhysicalObject.type, victim);
                    }
                }
            }

            return orig.Invoke(self, result, eu);
        }

        public static bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
        {
            if (self.thrownBy is Player && result.obj is Creature victim)
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoDamageChallenge c)
                    {
                        c.Hit(self.abstractPhysicalObject.type, victim);
                    }
                }
            }

            return orig.Invoke(self, result, eu);
        }

        public static void PlayerTracker_Update2(On.ScavengerOutpost.PlayerTracker.orig_Update orig, ScavengerOutpost.PlayerTracker self)
        {
            orig.Invoke(self);

            for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
            {
                if (self.PlayerOnOtherSide && ExpeditionData.challengeList[j] is BingoBombTollChallenge c && c.bombed)
                {
                    c.Pass(self.outpost.room.abstractRoom.name);
                }
            }
        }

        private static void ClearBs(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
        {
            orig.Invoke(self, game, survived, newMalnourished);

            if (BingoData.BingoMode)
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoBombTollChallenge c)
                    {
                        c.bombed = false;
                    }
                }
            }
        }

        public static void ScavengerBomb_Explode(On.ScavengerBomb.orig_Explode orig, ScavengerBomb self, BodyChunk hitChunk)
        {
            orig.Invoke(self, hitChunk);

            if (self.room.abstractRoom.scavengerOutpost && self.room.updateList.Find(x => x is ScavengerOutpost) is ScavengerOutpost outpost && Custom.DistLess(self.firstChunk.pos, outpost.placedObj.pos, 500f))
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoBombTollChallenge c)
                    {
                        c.Boom(self.room.abstractRoom.name);
                    }
                }
            }
        }

        public static void SmallNeedleWorm_PlaceInRoom(On.SmallNeedleWorm.orig_PlaceInRoom orig, SmallNeedleWorm self, Room placeRoom)
        {
            if (self.State.eggSpawn && placeRoom.abstractRoom.shelter && placeRoom.world.rainCycle.timer < 40)
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoHatchNoodleChallenge c)
                    {
                        c.Hatch();
                    }
                }
            }
            orig.Invoke(self, placeRoom);
        }

        public static ItemType Player_CraftingResults(On.Player.orig_CraftingResults orig, Player self)
        {
            ItemType origItem = orig.Invoke(self);

            for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
            {
                if (ExpeditionData.challengeList[j] is BingoCraftChallenge c)
                {
                    c.Crafted(origItem);
                }
            }

            return origItem;
        }

        public static void BigEel_JawsSnap(On.BigEel.orig_JawsSnap orig, BigEel self)
        {
            orig.Invoke(self);

            for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
            {
                if (!self.clampedObjects.Any(x => x.chunk.owner is Player) && ExpeditionData.challengeList[j] is BingoDodgeLeviathanChallenge c && c.wasInArea > 0)
                {
                    c.Dodged();
                }
            }
        }

        public static void FriendTracker_Update(On.FriendTracker.orig_Update orig, FriendTracker self)
        {
            orig.Invoke(self);

            // Copied from base game
            if (self.AI.creature.state.socialMemory != null && self.AI.creature.state.socialMemory.relationShips != null && self.AI.creature.state.socialMemory.relationShips.Count > 0)
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

        public static void WorldLoaderNoRegion1(On.WorldLoader.orig_ctor_RainWorldGame_Name_bool_string_Region_SetupValues orig, WorldLoader self, RainWorldGame game, SlugcatStats.Name playerCharacter, bool singleRoomWorld, string worldName, Region region, RainWorldGame.SetupValues setupValues)
        {
            orig.Invoke(self, game, playerCharacter, singleRoomWorld, worldName, region, setupValues);

            for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
            {
                if (ExpeditionData.challengeList[j] is BingoNoRegionChallenge c)
                {
                    c.Entered(worldName);
                }
            }
        }

        public static void WorldLoaderNoRegion2(On.WorldLoader.orig_ctor_RainWorldGame_Name_bool_string_Region_SetupValues orig, WorldLoader self, RainWorldGame game, SlugcatStats.Name playerCharacter, bool singleRoomWorld, string worldName, Region region, RainWorldGame.SetupValues setupValues)
        {
            orig.Invoke(self, game, playerCharacter, singleRoomWorld, worldName, region, setupValues);

            for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
            {
                if (ExpeditionData.challengeList[j] is BingoAllRegionsExcept r)
                {
                    r.Entered(worldName);
                }
            }
        }

        public static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig.Invoke(self, eu);

            // This is so if you dual wield something it doesnt add time twice
            ItemType ignore = null;
            for (int i = 0; i < self.grasps.Length; i++)
            {
                if (self.grasps[i] != null)
                {
                    ItemType heldType = self.grasps[i].grabbed.abstractPhysicalObject.type;
                    if (heldType != ignore)
                    {
                        ignore = heldType;
                        BingoData.heldItemsTime[(int)heldType]++;
                        //Plugin.logger.LogMessage(BingoData.heldItemsTime[(int)heldType]);
                    }
                }
            }
        }

        public static void Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
        {
            if (self.grasps[grasp] != null && self.grasps[grasp].grabbed is not Creature)
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

            for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
            {
                if (ExpeditionData.challengeList[j] is BingoGreenNeuronChallenge c && !c.moon)
                {
                    c.Delivered();
                }
            }
        }

        public static void SLOracleWakeUpProcedure_NextPhase(On.SLOracleWakeUpProcedure.orig_NextPhase orig, SLOracleWakeUpProcedure self)
        {
            orig.Invoke(self);

            if (self.phase == SLOracleWakeUpProcedure.Phase.GoToRoom || self.phase == SLOracleWakeUpProcedure.Phase.GoToAboveOracle)
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
                    if (BingoData.MoonDead) orig = 0;

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
                    if (weapon.thrownBy != null && weapon.thrownBy is Player p)
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

        public static void ScavengerAI_RecognizeCreatureAcceptingGift1(ILContext il)
        {
            ILCursor c = new(il);
        
            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<ItemTracker>("RepresentationForObject")
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg, 4);
                c.EmitDelegate<Action<ScavengerAI, PhysicalObject>>((self, item) =>
                {
                    if (self.tradeSpot != null && (self.creature.abstractAI as ScavengerAbstractAI).squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.Trade)
                    {
                        for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                        {
                            if (ExpeditionData.challengeList[j] is BingoTradeChallenge c)
                            {
                                c.Traded(self.CollectScore(item, false), item.abstractPhysicalObject.ID);
                            }
                        }
                    }
                });
            }
            else Plugin.logger.LogError("Uh oh, ScavengerAI_RecognizeCreatureAcceptingGift il fucked up " + il);
        }

        public static void ScavengerAI_RecognizeCreatureAcceptingGift2(ILContext il)
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
                    if (self.tradeSpot != null && (self.creature.abstractAI as ScavengerAbstractAI).squad.missionType == ScavengerAbstractAI.ScavengerSquad.MissionID.Trade)
                    {
                        for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                        {
                            if (ExpeditionData.challengeList[j] is BingoTradeTradedChallenge t)
                            {
                                EntityID givenItem = self.scavenger.room.socialEventRecognizer.ownedItemsOnGround[i].item.abstractPhysicalObject.ID;
                                EntityID receivedItem = item.abstractPhysicalObject.ID;
                                EntityID scavenger = self.scavenger.abstractCreature.ID;
                                Plugin.logger.LogMessage("GAVE " + givenItem);
                                Plugin.logger.LogMessage("RECEIVED " + receivedItem);

                                if (!t.traderItems.ContainsKey(givenItem) && givenItem != receivedItem) t.traderItems.Add(givenItem, scavenger);
                                t.Traded(receivedItem, scavenger);
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

            for (int j = 0; j < self.player.realizedCreature.grasps.Length; j++)
            {
                if (self.player.realizedCreature.grasps[j] != null)
                {
                    int k = 0;
                    while (k < self.outpost.outPostProperty.Count)
                    {
                        if (self.player.realizedCreature.grasps[j].grabbed.abstractPhysicalObject.ID == self.outpost.outPostProperty[k].ID)
                        {
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

        public static void SocialEventRecognizer_Theft(On.SocialEventRecognizer.orig_Theft orig, SocialEventRecognizer self, PhysicalObject item, Creature theif, Creature victim)
        {
            orig.Invoke(self, item, theif, victim);

            if (theif is Player && victim is Scavenger)
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
            if (chunk.owner is Creature victim && !victim.dead)
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
                    if (!victim.dead && Custom.DistLess(self.pos, victim.mainBodyChunk.pos, self.rad + victim.mainBodyChunk.rad + 20f))
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

            if (self is Creature victim && !victim.dead)
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
                    for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                    {
                        if (ExpeditionData.challengeList[j] is BingoDamageChallenge c)
                        {
                            c.Hit(self.abstractPhysicalObject.type, obj as Creature);
                        }
                    }
                });
            }
            else Plugin.logger.LogMessage("JellyFish_Collide failed! " + il);
        }

        public static bool MiscProgressionData_GetTokenCollected(On.PlayerProgression.MiscProgressionData.orig_GetTokenCollected_string_bool orig, PlayerProgression.MiscProgressionData self, string tokenString, bool sandbox)
        {
            if (BingoData.challengeTokens.Contains(tokenString)) return false;
            return orig.Invoke(self, tokenString, sandbox);
        }

        public static bool MiscProgressionData_GetTokenCollected_SlugcatUnlockID(On.PlayerProgression.MiscProgressionData.orig_GetTokenCollected_SlugcatUnlockID orig, PlayerProgression.MiscProgressionData self, MultiplayerUnlocks.SlugcatUnlockID classToken)
        {
            if (BingoData.challengeTokens.Contains(classToken.value)) return false;
            return orig.Invoke(self, classToken);
        }

        public static bool MiscProgressionData_GetTokenCollected_SafariUnlockID(On.PlayerProgression.MiscProgressionData.orig_GetTokenCollected_SafariUnlockID orig, PlayerProgression.MiscProgressionData self, MultiplayerUnlocks.SafariUnlockID safariToken)
        {
            if (BingoData.challengeTokens.Contains(safariToken.value)) return false;
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

        public static void Room_LoadedUnlock(ILContext il)
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
                    if (self.roomSettings.placedObjects[i].data is CollectToken.CollectTokenData c && BingoData.challengeTokens.Contains(c.tokenString)) orig = false;
                    return orig;
                });
            }
            else Plugin.logger.LogMessage("Challenge room loaded threw!!! " + il);
        }
        
        public static void Room_LoadedGreenNeuron(ILContext il)
        {
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
                    if (BingoHooks.GlobalBoard.AllChallenges.Any(x => x is BingoGreenNeuronChallenge))
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
