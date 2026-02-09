using System;
using System.Linq;

namespace PCGFairnessSimulator
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            bool headless = args.Contains("--headless");

            using (var game = new FairnessGame(headless))
                game.Run();
        }
    }
}