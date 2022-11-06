using System;
using ChampionFeats.Config;
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
        public const string BLUEPRINTNAME = "RMChampionFeatOffenceAim";

        // Token: 0x0600BD67 RID: 48487 RVA: 0x002F5FA0 File Offset: 0x002F41A0
        public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            if (Blueprints.HasNPCImmortalityBuff(Fact.Owner))
            {
                return;
            }
            int bonus = (((this.Fact.Owner.Progression.CharacterLevel - 1) / Main.settings.ScalingABLevelsPerStep) + 1) * Main.settings.ScalingABBonusPerStep;
            evt.AddModifier(bonus, Fact, ModifierDescriptor.UntypedStackable);
        }

        // Token: 0x0600BD68 RID: 48488 RVA: 0x00003B1E File Offset: 0x00001D1E
        public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
        }
    }
}
