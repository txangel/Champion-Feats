using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using UnityEngine;

namespace ChampionFeats.Components
{
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("bc4a8135433e49019fb903dd4edd2de1")]
    public class AddScalingSpellPenetration : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleSpellResistanceCheck>, IRulebookHandler<RuleSpellResistanceCheck>, ISubscriber, IInitiatorRulebookSubscriber
    {

        public BlueprintUnitFact spellPen
        {
            get
            {
                BlueprintUnitFactReference greater = m_SpellPen;
                if (greater == null)
                {
                    return null;
                }
                return greater.Get();
            }
        }
        public void OnEventAboutToTrigger(RuleSpellResistanceCheck evt)
        {
            int bonus = this.Fact.Owner.Progression.CharacterLevel * Main.settings.ScalingSpellPenBonusPerLevel;
            //it's caster level / 2 for without spell penetration, caster level if it does. This is a weird case where I don't mind paying a feat tax if I want to completely eliminate spell resistance
            // I just want to make it more easy in my favour without needing to take 3 feats for it effectively
            evt.AddSpellPenetration(evt.Initiator.HasFact(spellPen) ? bonus : Math.Max(1, bonus / 2), ModifierDescriptor.UntypedStackable);
        }


        public void OnEventDidTrigger(RuleSpellResistanceCheck evt)
        {
        }

        [ShowIf("CheckFact")]
        [SerializeField]
        public BlueprintUnitFactReference m_SpellPen;

        public ContextValue Value;
    }

}
