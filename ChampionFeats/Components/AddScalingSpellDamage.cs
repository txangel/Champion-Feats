using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;
using UnityEngine.Serialization;

namespace ChampionFeats.Components
{
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("34f01b4e062075c408523fbf92871dc5")]
    class AddScalingSpellDamage : UnitBuffComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IInitiatorRulebookSubscriber
    {
        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            if (evt.Reason.Ability == null)
            {
                return;
            }
            if (evt.Reason.Context == null || !evt.Reason.Ability.Blueprint.IsSpell)
            {
                return;
            }

            foreach (BaseDamage baseDamage in evt.DamageBundle)
            {
                
                if(baseDamage.Dice.Dice == Kingmaker.RuleSystem.DiceType.Zero) // trying to account for getting spells that shouldn't be damaging, apparently?
                {
                    continue;
                }
                int bonus = Value.Calculate(Context);
                baseDamage.AddModifier(Math.Max(1, bonus), Fact);
            }
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
        }

        public ContextValue Value;
    }
}
