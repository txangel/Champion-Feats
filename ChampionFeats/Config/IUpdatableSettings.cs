namespace ChampionFeats.Config {
    public interface IUpdatableSettings {
        void OverrideSettings(IUpdatableSettings userSettings);
    }
}
