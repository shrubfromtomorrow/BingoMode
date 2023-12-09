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
    public class ChallengeUtils
    {
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
    }
}
