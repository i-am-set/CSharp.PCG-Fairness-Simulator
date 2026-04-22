using System;

namespace PCGFairnessSimulator
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new FairnessGame())
                game.Run();
        }
    }
}