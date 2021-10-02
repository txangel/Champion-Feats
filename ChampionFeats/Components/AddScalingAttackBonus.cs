using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;

namespace ChampionFeats.Components
{
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [AllowMultipleComponents]
    [TypeId("40059667d9b245fdae9d8524d669b125")]
    class AddScalingAttackBonus : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, IRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, ISubscriber, IInitiatorRulebookSubscriber
    {
        // Token: 0x0600BD67 RID: 48487 RVA: 0x002F5FA0 File Offset: 0x002F41A0
        public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            int bonus = value.Calculate(Context) * Math.Max(Bonus.Calculate(base.Context), 1);
            evt.AddModifier(bonus, Fact, Descriptor);

        }

        // Token: 0x0600BD68 RID: 48488 RVA: 0x00003B1E File Offset: 0x00001D1E
        public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
        }
        public ModifierDescriptor Descriptor;
        // Token: 0x04007BE2 RID: 31714
        public ContextValue value;
        // Token: 0x04007BE4 RID: 31716
        public ContextValue Bonus;
    }
}
