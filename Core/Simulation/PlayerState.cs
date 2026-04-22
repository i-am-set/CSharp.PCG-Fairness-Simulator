using System;

namespace Core.Simulation
{
    public readonly record struct PlayerState
    {
        public int CurrentHP { get; }
        public int MaxHP { get; }
        public int Resources { get; }
        public bool IsAlive => CurrentHP > 0;

        public PlayerState(int currentHp, int maxHp, int resources)
        {
            CurrentHP = currentHp;
            MaxHP = maxHp;
            Resources = resources;
        }

        public static PlayerState Initial(SimulationConfig config)
        {
            return new PlayerState(config.InitialHP, config.MaxHP, config.InitialResources);
        }

        public PlayerState ApplyDamage(int difficulty, double multiplier)
        {
            int damage = (int)(difficulty * multiplier);
            return new PlayerState(CurrentHP - damage, MaxHP, Resources);
        }

        public PlayerState AddReward(int reward)
        {
            return new PlayerState(CurrentHP, MaxHP, Resources + reward);
        }

        public PlayerState PerformGreedyHealing(int cost, int healAmount)
        {
            int hp = CurrentHP;
            int res = Resources;

            if (cost <= 0) return this;

            while (res >= cost && hp < MaxHP)
            {
                hp = Math.Min(hp + healAmount, MaxHP);
                res -= cost;
            }

            return new PlayerState(hp, MaxHP, res);
        }
    }
}