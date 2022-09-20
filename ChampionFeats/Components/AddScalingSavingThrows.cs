using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;

namespace ChampionFeats.Components
{
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [AllowMultipleComponents]
    [TypeId("D09E14DCC14748708E79AE101363676E")]
    class AddScalingSavingThrows : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleSavingThrow>, IRulebookHandler<RuleSavingThrow>, ISubscriber, IInitiatorRulebookSubscriber
    {
        public void OnEventAboutToTrigger(RuleSavingThrow evt)
        {
            int bonus = 2;
            evt.AddModifier(bonus, Fact, ModifierDescriptor.UntypedStackable);
        }

        public void OnEventDidTrigger(RuleSavingThrow evt)
        {
        }

        public override void OnTurnOn()
        {
            base.OnTurnOn();
        }
    }
}
