using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using System.Linq;
using CreatureType = CreatureTemplate.Type;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    //public class BingoDeathPitChallenge : Challenge, IBingoChallenge
    //{
    //    public CreatureType crit;
    //    public string region;
    //
    //    public override void UpdateDescription()
    //    {
    //        this.description = ChallengeTools.IGT.Translate("Shove a <crit> into a death pit<region>")
    //            .Replace("<crit>", ChallengeTools.creatureNames[crit.Index].TrimEnd('s'))
    //            .Replace("<region>", region != null ? " in " + Region.GetRegionFullName(region, ExpeditionData.slugcatPlayer) : "");
    //        base.UpdateDescription();
    //    }
    //
    //    public override bool Duplicable(Challenge challenge)
    //    {
    //        return challenge is not BingoDeathPitChallenge;
    //    }
    //
    //    public override string ChallengeName()
    //    {
    //        return ChallengeTools.IGT.Translate("Creature shoving");
    //    }
    //
    //    public override Challenge Generate()
    //    {
    //        return new BingoDeathPitChallenge
    //        {
    //
    //        };
    //    }
    //
    //    public void Pitted(Creature c)
    //    {
    //        if (!completed && c.Template.type == crit)
    //        {
    //            UpdateDescription();
    //            CompleteChallenge();
    //        }
    //    }
    //
    //    public override int Points()
    //    {
    //        return 20;
    //    }
    //
    //    public override bool CombatRequired()
    //    {
    //        return false;
    //    }
    //
    //    public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
    //    {
    //        return true;
    //    }
    //
    //    public override string ToString()
    //    {
    //        return string.Concat(new string[]
    //        {
    //            "DeathPit",
    //            "~",
    //            current.ToString(),
    //            "><",
    //            amount.ToString(),
    //            "><",
    //            completed ? "1" : "0",
    //            "><",
    //            hidden ? "1" : "0",
    //            "><",
    //            revealed ? "1" : "0"
    //        });
    //    }
    //
    //    public override void FromString(string args)
    //    {
    //        try
    //        {
    //            string[] array = Regex.Split(args, "><");
    //            current = int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
    //            amount = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
    //            completed = (array[2] == "1");
    //            hidden = (array[3] == "1");
    //            revealed = (array[4] == "1");
    //            UpdateDescription();
    //        }
    //        catch (Exception ex)
    //        {
    //            ExpLog.Log("ERROR: DeathPit FromString() encountered an error: " + ex.Message);
    //        }
    //    }
    //
    //    public void AddHooks()
    //    {
    //
    //    }
    //
    //    public void RemoveHooks()
    //    {
    //    }
    //}
}