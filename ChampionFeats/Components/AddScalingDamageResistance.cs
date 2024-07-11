using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;

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

        /*
        public class AddDamageResistanceAllScaling : AddDamageResistanceBase
        {
            public override bool Bypassed(ComponentRuntime runtime, BaseDamage damage, ItemEntityWeapon weapon)
            {
                return runtime == null || runtime.Owner == null || Blueprints.HasNPCImmortalityBuff(runtime.Owner);
            }

            public override int CalculateValue(ComponentRuntime runtime)
            {
                return (((runtime.Owner.Progression.CharacterLevel - 1) / Main.settings.ScalingDRLevelsPerStep) + 1) * Main.settings.ScalingDRBonusPerStep;
            }
        }
        */
    }
}
