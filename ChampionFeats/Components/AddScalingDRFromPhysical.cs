using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.Validation;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace ChampionFeats.Components
{
    [ComponentName("Buffs/AddEffect/ScalingDR")]
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [AllowedOn(typeof(BlueprintUnit), false)]
    [AllowMultipleComponents]
    [TypeId("455624efd46242f2bc3e0da3f8e60116")]
    class AddScalingDRFromPhysical : AddDamageResistancePhysical
    {
        public override int CalculateValue(AddDamageResistanceBase.ComponentRuntime runtime)
        {
            return Value.Calculate(runtime.Fact.MaybeContext) * Math.Max(bonusValue.Calculate(runtime.Fact.MaybeContext), 1);
        }
        public ContextValue bonusValue;
    }

  
}
