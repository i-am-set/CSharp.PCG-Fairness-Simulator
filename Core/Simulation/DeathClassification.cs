namespace Core.Simulation
{
    public enum DeathClassification
    {
        None, // survived
        Spike,  // single encounter damage > max survivable amount
        Attrition, // total damage was more than healing capacity over time
        Starvation  // resources werent enough to buy healing despite survivable damage
    }
}