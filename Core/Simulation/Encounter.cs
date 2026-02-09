namespace Core.Simulation
{
    public struct Encounter
    {
        public int Index;

        public int Difficulty;
        public int Reward;

        public int OriginalDifficulty;
        public int OriginalReward;

        public Encounter(int index, int difficulty, int reward)
        {
            Index = index;
            Difficulty = difficulty;
            Reward = reward;
            OriginalDifficulty = difficulty;
            OriginalReward = reward;
        }
    }
}