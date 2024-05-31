using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using CreatureType = CreatureTemplate.Type;
using ItemType = AbstractPhysicalObject.AbstractObjectType;
using MSCItemType = MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType;

namespace BingoMode.Challenges
{
    public class SettingBox<T> : IStrongBox // Basically just strongbox but i needed morer data to store (i dont know if this works to be honest)
    {
        public Type type;
        public string name;
        public int index;
        public int[] locks; // mutually exclusive settings
        public T Value;
        public string listName;
        public SettingBox(T value, string displayName, int index, int[] locks = null, string listName = null)
        {
            Value = value;
            type = typeof(T);
            name = displayName;
            this.index = index;
            this.locks = locks;
            this.listName = listName;
        }

        object IStrongBox.Value
        {
            get
            {
                return Value;
            }
            set
            {
                Value = (T)((object)value);
            }
        }

        public override string ToString()
        {
            string excl = "NULL";
            //if (locks != null)
            //{
            //    excl = locks[0].ToString();
            //    for (int i = 1; i < locks.Length; i++)
            //    {
            //        excl += "," + locks[i];
            //    }
            //}
            if (listName != null)
            {
                excl = listName;
            }
            return string.Concat(
                type.ToString(),
                "|",
                ValueConverter.ConvertToString(Value),
                "|",
                name,
                "|",
                index.ToString(),
                "|",
                excl
                );
        }
    }

    //public interface BingoChallenge
    //{
    //    void AddHooks();
    //    void RemoveHooks();
    //    List<object> Settings();
    //    bool RequireSave { get; set; }
    //    bool Failed { get; set; }
    //    bool[] TeamsCompleted { get; set; }
    //}

    public static class ChallengeHooks
    {
        public static object SettingBoxFromString(string save)
        {
            string[] settings = save.Split('|');
            try
            {
                //Plugin.logger.LogMessage("Attempting to recreate settingbox from string:");
                object bocks = null;
                //int[] locks = null;
                string listName = null;
                if (settings[4] != "NULL")
                {
                    //List<int> tempLocks = [];
                    //foreach (var g in settings[4].Split(','))
                    //{
                    //    tempLocks.Add(int.Parse(g));
                    //}
                    //locks = tempLocks.ToArray();
                    listName = settings[4];
                }
                switch (settings[0])
                {
                    case "Int32":
                    case "Int64":
                    case "System.Int32":
                    case "System.Int64":
                        //Plugin.logger.LogMessage("Generating it as int!");
                        bocks = new SettingBox<int>(int.Parse(settings[1]), settings[2], int.Parse(settings[3]), null, listName);
                        break;
                    case "Boolean":
                    case "System.Boolean":
                        //Plugin.logger.LogMessage("Generating it as bool!");
                        bocks = new SettingBox<bool>(settings[1].ToLowerInvariant() == "true", settings[2], int.Parse(settings[3]), null, listName);
                        break;
                    case "String":
                    case "System.String":
                        //Plugin.logger.LogMessage("Generating it as string!");
                        bocks = new SettingBox<string>(settings[1], settings[2], int.Parse(settings[3]), null, listName);
                        break;
                }
                //Plugin.logger.LogMessage("Recreation successful!");
                //Plugin.logger.LogMessage("Final grug: " + bocks.ToString());
                return bocks;
            }
            catch(Exception ex)
            {
                Plugin.logger.LogMessage("Errored guy:");
                foreach (var j in settings)
                {
                    Plugin.logger.LogMessage(j);
                }; 
                Plugin.logger.LogError("Failed to recreate SettingBox from string!!!" + ex);
                return null;
            }
        }

        // Runtime detour hooks
        public static Hook tokenColorHook;

        // Sporecloud fix for owner recognition
        public static Dictionary<UpdatableAndDeletable, EntityID> ownerOfUAD = [];

        // Normal/IL hooks
        public static void Apply()
        {
            // Clearing stuff that needs to be cleared at the end of the cycle
            On.SaveState.SessionEnded += ClearBs;

            // For damage and kill challenges, i put them here since theres so many and both challenges would have to do the same hooks
            On.Spear.HitSomething += Spear_HitSomething;
            On.Rock.HitSomething += Rock_HitSomething;
            On.MoreSlugcats.LillyPuck.HitSomething += LillyPuck_HitSomething;
            IL.Explosion.Update += Explosion_Update;
            IL.SporeCloud.Update += SporeCloud_Update;
            IL.JellyFish.Collide += JellyFish_Collide;
            IL.PuffBall.Explode += PuffBall_Explode;
            IL.FlareBomb.Update += FlareBomb_Update;
            //On.Expedition.Challenge.CompleteChallenge += Challenge_CompleteChallenge;
            //IL.Expedition.Challenge.CompleteChallenge += Challenge_CompleteChallengeIL;
        }

        public static void Challenge_CompleteChallengeIL(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("unl-passage")
                ))
            {
                c.Index++;
                c.EmitDelegate<Func<bool, bool>>((orig) =>
                {
                    if (BingoData.BingoMode) orig = false;
                    return orig;
                });
            }
            else Plugin.logger.LogError("Uh oh, Challenge_CompleteChallengeIL il fucked up " + il);
        }

        public static void Challenge_CompleteChallenge(On.Expedition.Challenge.orig_CompleteChallenge orig, Challenge self)
        {
            if (self.completed) return;
            if (self is BingoChallenge c)
            {
                if (self.hidden) return; // Hidden means locked out here in bingo
                if (c.RequireSave && !self.revealed) // I forgot what this does
                {
                    self.revealed = true;
                    return;
                }

                if (SteamTest.LobbyMembers.Count > 0)
                {
                    SteamTest.BroadcastCompletedChallenge(self);
                }
            }

            orig.Invoke(self);
            if (BingoData.BingoMode) Expedition.Expedition.coreFile.Save(false);
        }

        public static void WinState_CycleCompleted(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(
                x => x.MatchStloc(42),
                x => x.MatchLdloc(42),
                x => x.MatchIsinst<AchievementChallenge>()
                ))
            {
                c.Index++;
                c.Emit(OpCodes.Ldloc, 42);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<Challenge, WinState>>((ch, self) =>
                {
                    if (ch is BingoAchievementChallenge c) c.CheckAchievementProgress(self);
                });
            }
            else Plugin.logger.LogError("Uh oh, WinState_CycleCompleted il fucked up " + il);
        }

        public static void FlareBomb_Update(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Room>("VisualContact")
                ))
            {
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_0);
                c.EmitDelegate<Action<FlareBomb, int>>((self, i) =>
                {
                    Plugin.logger.LogMessage("gruh");
                    if (BingoData.BingoMode && self.thrownBy != null && self.thrownBy.abstractCreature.creatureTemplate.type == CreatureType.Slugcat && self.room.abstractRoom.creatures[i].realizedCreature is Creature victim)
                    {
                        ReportHit(self.abstractPhysicalObject.type, victim, self.abstractPhysicalObject.ID);
                    }
                });
            }
            else Plugin.logger.LogError("Uh oh, FlareBomb_Update il fucked up " + il);
        }

        public static void PuffBall_Explode(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchNewobj<SporeCloud>(),
                x => x.MatchCallOrCallvirt<Room>("AddObject")
                ))
            {
                c.Index++;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<SporeCloud, PuffBall, SporeCloud>>((orig, self) =>
                {
                    if (BingoData.BingoMode && self.thrownBy != null && self.thrownBy.abstractCreature.creatureTemplate.type == CreatureType.Slugcat)
                    {
                        ownerOfUAD[orig] = self.abstractPhysicalObject.ID;
                    }

                    return orig;
                });
            }
            else Plugin.logger.LogError("Uh oh, PuffBall_Explode il fucked up " + il);
        }

        // So we recognize the explosion before the creature dies!!!
        public static void Explosion_Update(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(
                x => x.MatchLdloc(6),
                x => x.MatchLdcI4(-1),
                x => x.MatchBle(out ILLabel lable)
                ) &&
                c.TryGotoNext(MoveType.Before,
                x => x.MatchStloc(12)
                ))
            {
                c.Index--;
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_2);
                c.Emit(OpCodes.Ldloc_3);
                c.EmitDelegate<Action<Explosion, int, int>>((explosion, j, k) =>
                {
                    if (BingoData.BingoMode && explosion.sourceObject != null && explosion.killTagHolder is Player && explosion.room.physicalObjects[j][k] is Creature victim)
                    {
                        if (!victim.dead) ReportHit(explosion.sourceObject.abstractPhysicalObject.type, victim, explosion.sourceObject.abstractPhysicalObject.ID);
                    }
                });
            }
            else Plugin.logger.LogError("Uh oh, Explosion_Update il fucked up " + il);
        }

        public static void ReportHit(ItemType weapon, Creature victim, EntityID source, bool report = true)
        {
            if (weapon == null || victim == null) return;
            Plugin.logger.LogMessage($"Report hit! {weapon} {victim.Template.type} {source}");

            if (source != null && report)
            {
                if (BingoData.blacklist.TryGetValue(victim, out var gruh) && gruh.Contains(source)) return;
                if (!BingoData.blacklist.ContainsKey(victim)) BingoData.blacklist.Add(victim, []);
                if (BingoData.blacklist.TryGetValue(victim, out var list) && !list.Contains(source)) list.Add(source);
            }

            Plugin.logger.LogMessage($"Hit {weapon} {victim.Template.type} {source} went through!");

            for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
            {
                if (ExpeditionData.challengeList[j] is BingoDamageChallenge c)
                {
                    c.Hit(weapon, victim);
                }
            }

            EntityID id = victim.abstractCreature.ID;
            if (!BingoData.hitTimeline.ContainsKey(id)) BingoData.hitTimeline.Add(id, []);
            if (BingoData.hitTimeline.TryGetValue(id, out var gru) && (gru.Count == 0 || gru.Last() != weapon)) { gru.Remove(weapon); gru.Add(weapon); Plugin.logger.LogMessage($"Added {weapon} to id: {id} and creature type: {victim.abstractCreature.creatureTemplate.type}"); }
        }

        public static void Creature_UpdateIL(ILContext il)
        {
            ILCursor c = new(il);
        
            if (c.TryGotoNext(
                x => x.MatchLdstr("{0} Fell out of room!")
                ) && 
                c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Creature>("Die")
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<Creature>>((self) =>
                {
                    if (self.killTag != null && self.killTag.creatureTemplate.type == CreatureType.Slugcat && self.killTag.realizedCreature is Player p)
                    {
                        for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                        {
                            if (ExpeditionData.challengeList[j] is BingoKillChallenge c)
                            {
                                c.DeathPit(self, p);
                            }
                        }
                    }
                });
            }
            else Plugin.logger.LogError("Uh oh, Creature_UpdateIL il fucked up " + il);
        }

        public static void Player_SlugcatGrab(On.Player.orig_SlugcatGrab orig, Player self, PhysicalObject obj, int graspUsed)
        {
            orig.Invoke(self, obj, graspUsed);

            if (obj is Creature crit)
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoTransportChallenge c)
                    {
                        c.Grabbed(crit);
                    }
                }
            }
        }

        public static void RegionGate_NewWorldLoaded2(On.RegionGate.orig_NewWorldLoaded orig, RegionGate self)
        {
            orig.Invoke(self);
            Plugin.logger.LogMessage("HALO HALOO 2");

            for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
            {
                if (ExpeditionData.challengeList[j] is BingoTransportChallenge c)
                {
                    c.Gated(self.room.world.region.name);
                }
            }
        }

        public static void RegionGate_NewWorldLoaded1(On.RegionGate.orig_NewWorldLoaded orig, RegionGate self)
        {
            orig.Invoke(self);
            Plugin.logger.LogMessage("HALO HALOO");

            for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
            {
                if (ExpeditionData.challengeList[j] is BingoCreatureGateChallenge c)
                {
                    c.Gate(self.room.abstractRoom.name);
                }
            }
        }

        public static bool LillyPuck_HitSomething(On.MoreSlugcats.LillyPuck.orig_HitSomething orig, LillyPuck self, SharedPhysics.CollisionResult result, bool eu)
        {
            if (BingoData.BingoMode && self.thrownBy is Player && result.obj is Creature victim && !victim.dead)
            {
                ReportHit(self.abstractPhysicalObject.type, victim, self.abstractPhysicalObject.ID, false);
            }

            return orig.Invoke(self, result, eu);
        }

        public static bool Rock_HitSomething(On.Rock.orig_HitSomething orig, Rock self, SharedPhysics.CollisionResult result, bool eu)
        {
            if (BingoData.BingoMode && self.thrownBy is Player && result.obj is Creature victim && !victim.dead)
            {
                ReportHit(self.abstractPhysicalObject.type, victim, self.abstractPhysicalObject.ID, false);
            }

            return orig.Invoke(self, result, eu);
        }

        public static bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
        {
            if (BingoData.BingoMode && self.thrownBy is Player && result.obj is Creature victim && !victim.dead)
            {
                ReportHit(self.abstractPhysicalObject.type, victim, self.abstractPhysicalObject.ID, false);
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
            if (BingoData.BingoMode)
            {
                for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                {
                    if (ExpeditionData.challengeList[j] is BingoChallenge g && g.RequireSave)
                    {
                        if (survived && ExpeditionData.challengeList[j].revealed)
                        {
                            ExpeditionData.challengeList[j].CompleteChallenge();
                        }
                        ExpeditionData.challengeList[j].revealed = false;
                    }
                }
                ownerOfUAD.Clear();
                BingoData.hitTimeline.Clear();
                BingoData.blacklist.Clear();
                BingoData.heldItemsTime = new int[ExtEnum<ItemType>.values.Count];
            }

            orig.Invoke(self, game, survived, newMalnourished);
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
                if (ExpeditionData.challengeList[j] is BingoGreenNeuronChallenge c && !c.moon.Value)
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
                    if (ExpeditionData.challengeList[j] is BingoGreenNeuronChallenge c && c.moon.Value)
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
                x => x.MatchLdsfld<ModManager>("MSC")
                ))
            {
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

        //public static void Bee_Attach(On.SporePlant.Bee.orig_Attach orig, SporePlant.Bee self, BodyChunk chunk)
        //{
        //    if (BingoData.BingoMode && chunk.owner is Creature victim && !victim.dead)
        //    {
        //        ReportHit(self.owner.abstractPhysicalObject.type, victim, self.owner.abstractPhysicalObject.ID);
        //    }
        //
        //    orig.Invoke(self, chunk);
        //}

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
                    if (!self.slatedForDeletetion && BingoData.BingoMode && ownerOfUAD.ContainsKey(self) && self != null && self.killTag != null && self.killTag.creatureTemplate.type == CreatureType.Slugcat)
                    {
                        Creature victim = self.room.abstractRoom.creatures[i].realizedCreature;
                        if (victim != null && !victim.dead && Custom.DistLess(self.pos, victim.mainBodyChunk.pos, self.rad + victim.mainBodyChunk.rad + 20f))
                        {
                            ReportHit(ItemType.PuffBall, victim, ownerOfUAD[self]);
                        }
                    }
                });
            }
            else Plugin.logger.LogError("Uh oh, SporeCloud_Update il fucked up " + il);
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
                    if (BingoData.BingoMode && self.thrownBy is Player)
                    {
                        ReportHit(self.abstractPhysicalObject.type, obj as Creature, self.abstractPhysicalObject.ID, false);
                    }
                });
            }
            else Plugin.logger.LogMessage("JellyFish_Collide failed! " + il);
        }

        public static bool MiscProgressionData_GetTokenCollected(On.PlayerProgression.MiscProgressionData.orig_GetTokenCollected_string_bool orig, PlayerProgression.MiscProgressionData self, string tokenString, bool sandbox)
        {
            Plugin.logger.LogMessage("Checkunlock " + tokenString);
            if (BingoData.challengeTokens.Contains(tokenString)) return false;
            return orig.Invoke(self, tokenString, sandbox);
        }

        public static bool MiscProgressionData_GetTokenCollected_SlugcatUnlockID(On.PlayerProgression.MiscProgressionData.orig_GetTokenCollected_SlugcatUnlockID orig, PlayerProgression.MiscProgressionData self, MultiplayerUnlocks.SlugcatUnlockID classToken)
        {
            Plugin.logger.LogMessage("Checkunlock " + classToken.value);
            if (BingoData.challengeTokens.Contains(classToken.value)) return false;
            return orig.Invoke(self, classToken);
        }

        public static bool MiscProgressionData_GetTokenCollected_SafariUnlockID(On.PlayerProgression.MiscProgressionData.orig_GetTokenCollected_SafariUnlockID orig, PlayerProgression.MiscProgressionData self, MultiplayerUnlocks.SafariUnlockID safariToken)
        {
            Plugin.logger.LogMessage("Checkunlock " + safariToken.value + "-safari");
            if (BingoData.challengeTokens.Contains(safariToken.value + "-safari")) return false;
            return orig.Invoke(self, safariToken);
        }

        public static void CollectToken_Pop(On.CollectToken.orig_Pop orig, CollectToken self, Player player)
        {
            if (self.expand > 0f)
            {
                return;
            }
            if (self.placedObj.data is CollectToken.CollectTokenData d && BingoData.challengeTokens.Contains(d.tokenString + (d.isRed ? "-safari" : "")) && ExpeditionData.challengeList.Any(x => x is BingoUnlockChallenge b && b.unlock.Value == d.tokenString + (d.isRed ? "-safari" : "")))
            {
                foreach (Challenge ch in ExpeditionData.challengeList)
                {
                    if (ch is BingoUnlockChallenge b && !b.completed && !b.TeamsCompleted[SteamTest.team] && !b.revealed && !b.hidden && b.unlock.Value == (d.tokenString + (d.isRed ? "-safari" : "")))
                    {
                        ch.CompleteChallenge();
                        if (BingoData.challengeTokens.Contains(d.tokenString)) BingoData.challengeTokens.Remove(d.tokenString + (d.isRed ? "-safari" : ""));
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
            if (self.placedObj.data is CollectToken.CollectTokenData d && BingoData.challengeTokens.Contains(d.tokenString + (d.isRed ? "-safari" : ""))) return Color.white;//Color.Lerp(orig.Invoke(self), Color.white, 0.8f);
            else return orig.Invoke(self);
        }

        public static void Room_LoadedUnlock(ILContext il)
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(
                x => x.MatchLdfld("PlacedObject", "active")
                ) && 
                c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld("ModManager", "Expedition")
                ))
            {
                Plugin.logger.LogMessage(c.Prev);

                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc, 23);
                c.EmitDelegate<Func<bool, Room, int, bool>>((orig, self, i) =>
                {
                    if (self.roomSettings.placedObjects[i].data is CollectToken.CollectTokenData c && BingoData.challengeTokens.Contains(c.tokenString + (c.isRed ? "-safari" : ""))) orig = false;
                    return orig;
                });
            }
            else Plugin.logger.LogError("Challenge room loaded threw!!! " + il);
        }
        
        public static void Room_LoadedGreenNeuron(ILContext il)
        {
            //Plugin.logger.LogMessage("Ass " + il);
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
                    if (ExpeditionData.challengeList.Any(x => x is BingoGreenNeuronChallenge))
                    {
                        AbstractPhysicalObject startItem = new (room.world, ItemType.NSHSwarmer, null, new WorldCoordinate(room.abstractRoom.index, room.shelterDoor.playerSpawnPos.x, room.shelterDoor.playerSpawnPos.y, 0), room.game.GetNewID());
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
                    Plugin.logger.LogMessage("Eated fo " + edible);
                }
                else if (ExpeditionData.challengeList[j] is BingoDontUseItemChallenge g && g.isFood && edible is PhysicalObject p)
                {
                    g.Used(p.abstractPhysicalObject.type);
                }
            }
        }

        public static void Player_ObjectEaten2(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible edible)
        {
            orig.Invoke(self, edible);

            for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
            {
                if (ExpeditionData.challengeList[j] is BingoDontUseItemChallenge g && g.isFood && edible is PhysicalObject p)
                {
                    g.Used(p.abstractPhysicalObject.type);
                }
            }
        }
    }
}
