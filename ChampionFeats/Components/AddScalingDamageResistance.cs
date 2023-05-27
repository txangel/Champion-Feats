using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UnitLogic.FactLogic;
using System.Runtime.InteropServices;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Items;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.PubSubSystem;
using ChampionFeats.Extensions;
using ChampionFeats.Utilities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.EntitySystem.Entities;

namespace ChampionFeats.Components
{
    [AllowedOn(typeof(BlueprintUnit), false)]
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [AllowedOn(typeof(BlueprintFeature))]
    [AllowMultipleComponents]
    [TypeId("5EEED74D80FF46ACA9FB2A15025B4727")]
    /*
     * See comments in ContextValuePatches for why this class is basically barren.
     * Only exists to hold the blueprint name now.
     */
    public class AddScalingDamageResistance : UnitFactComponentDelegate, ISubscriber
    {
        public const string BLUEPRINTNAME = "RMChampionFeatDefenceDR";
    }
}
