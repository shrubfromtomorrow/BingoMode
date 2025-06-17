using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;
using UnityEngine;

namespace BingoMode
{
    using System.Runtime.CompilerServices;
    using BingoHUD;
    using Rewired.ControllerExtensions;

    public static class SpectatorHooks
    {
        public static void Hook()
        {
            BingoData.SpectatorMode = true;
            On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
            On.SoundLoader.ShouldSoundPlay += SoundLoader_ShouldSoundPlay;
            On.Player.checkInput += Player_checkInput;
            On.RainCycle.Update += RainCycle_Update;
            IL.WorldLoader.ctor_RainWorldGame_Name_Timeline_bool_string_Region_SetupValues += WorldLoader_ctor_RainWorldGame_Name_Timeline_bool_string_Region_SetupValues;
            On.Player.Die += Player_Die;
        }

        private static void Player_Die(On.Player.orig_Die orig, Player self)
        {
        }

        public static void UnHook()
        {
            BingoData.SpectatorMode = false;
            On.HUD.HUD.InitSinglePlayerHud -= HUD_InitSinglePlayerHud;
            On.SoundLoader.ShouldSoundPlay -= SoundLoader_ShouldSoundPlay;
            On.Player.checkInput -= Player_checkInput;
            On.RainCycle.Update -= RainCycle_Update;
            IL.WorldLoader.ctor_RainWorldGame_Name_Timeline_bool_string_Region_SetupValues -= WorldLoader_ctor_RainWorldGame_Name_Timeline_bool_string_Region_SetupValues;
            On.Player.Die -= Player_Die;
        }

        private static void WorldLoader_ctor_RainWorldGame_Name_Timeline_bool_string_Region_SetupValues(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdcI4(100),
                x => x.MatchStfld<WorldLoader>("simulateUpdateTicks")
                ))
            {
                c.MoveAfterLabels();
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<WorldLoader>>((worldLoader) =>
                {
                    int line = worldLoader.lines.FindIndex(s => s.StartsWith("SU_S01"));
                    worldLoader.lines[line] = "SU_S01 : DISCONNECTED : SHELTER";
                    int line2 = worldLoader.lines.FindIndex(s => s.StartsWith("SU_A22"));
                    worldLoader.lines[line2] = "SU_A22 : SU_A43, SU_A23, DISCONNECTED";
                });
            }
            else Plugin.logger.LogError("Uh oh this thinj for spectator didnt wor " + il);
        }

        private static void RainCycle_Update(On.RainCycle.orig_Update orig, RainCycle self)
        {
            self.timer = Mathf.Min(500, self.timer);
            orig.Invoke(self);
        }

        private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            // Don't
        }

        private static SoundID[] allowedSounds =
        {
            SoundID.HUD_Karma_Reinforce_Bump,
            SoundID.Moon_Wake_Up_Swarmer_Ping,
            BingoEnums.BINGO_FINAL_BONG,
            new("Tock", false),
            new("Tick", false),
        };

        private static bool SoundLoader_ShouldSoundPlay(On.SoundLoader.orig_ShouldSoundPlay orig, SoundLoader self, SoundID soundID)
        {
            if (!BingoData.BingoMode) return orig.Invoke(self, soundID);
            return orig.Invoke(self, soundID) && allowedSounds.Contains(soundID);
        }

        private static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            orig.Invoke(self, cam);

            for (int i = 0; i < cam.SpriteLayers.Length - 2; i++)
            {
                cam.SpriteLayers[i].RemoveAllChildren();
                cam.SpriteLayers[i].RemoveFromContainer();
            }
            if (cam.preLoadedTexture != null)
            {
                cam.preLoadedTexture = new byte[0];
            }
            if (cam.preLoadedBKG != null)
            {
                cam.preLoadedBKG = new byte[0];
            }
            for (int i = 0; i < self.parts.Count; i++)
            {
                self.parts[i].ClearSprites();
            }
            for (int j = 0; j < self.fContainers.Length; j++)
            {
                self.fContainers[j].RemoveAllChildren();
            }
            if (self.jollyMeter != null) self.parts.Remove(self.jollyMeter);
            self.AddPart(new BingoHUDMain(self));
            if (cam.virtualMicrophone == null || cam.virtualMicrophone.ambientSoundPlayers == null) return;
            for (int j = 0; j < cam.virtualMicrophone.ambientSoundPlayers.Count; j++)
            {
                cam.virtualMicrophone.ambientSoundPlayers[j].Destroy();
            }
        }

    }
}
