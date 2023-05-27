using System;
using ChampionFeats.Config;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;

namespace ChampionFeats.Components
{
    [ComponentName("Buffs/Damage scaling bonus")]
    [AllowedOn(typeof(BlueprintBuff), false)]
    [TypeId("dd0309ebf4c847c4941fe4a2c9dcbb98")]
    public class AddScalingDamageBonus : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateWeaponStats>, IRulebookHandler<RuleCalculateWeaponStats>, ISubscriber, IInitiatorRulebookSubscriber
    {
        public const string BLUEPRINTNAME = "RMChampionFeatOffenceDam";

        // Token: 0x0600A58A RID: 42378 RVA: 0x0029FB50 File Offset: 0x0029DD50
        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (Blueprints.HasNPCImmortalityBuff(Fact.Owner))
            {
                return;
            }
            int totalBonus = (((this.Fact.Owner.Progression.CharacterLevel - 1) / Main.settings.ScalingDamageLevelsPerStep) + 1) * Main.settings.ScalingDamageBonusPerStep;
            evt.AddDamageModifier(totalBonus, Fact, ModifierDescriptor.UntypedStackable);
        }

        // Token: 0x0600A58B RID: 42379 RVA: 0x00003B1E File Offset: 0x00001D1E
        public void OnEventDidTrigger(RuleCalculateWeaponStats evt)
        {
        }
    }
}
