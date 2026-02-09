using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Metrics;
using Core.Simulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Visualization
{
    public class Dashboard
    {
        private readonly SimulationController _controller;
        private SpriteFont _font;

        public Dashboard(SimulationController controller)
        {
            _controller = controller;
        }

        public void LoadContent(SpriteFont font, GraphicsDevice graphics)
        {
            _font = font;
            ChartRenderer.Initialize(graphics);
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            string status = $"Status: {_controller.StatusMessage} | Progress: {_controller.Progress:P1}";
            spriteBatch.DrawString(_font, status, new Vector2(20, 20), Color.White);

            string controls = "[SPACE] Start  [ESC] Exit";
            Vector2 controlsSize = _font.MeasureString(controls);
            spriteBatch.DrawString(_font, controls, new Vector2(screenBounds.Width - controlsSize.X - 20, 20), Color.Yellow);

            if (_controller.RawResults == null || _controller.RawResults.Count == 0)
            {
                string prompt = "Press SPACE to Start Simulation";
                Vector2 promptSize = _font.MeasureString(prompt);
                spriteBatch.DrawString(_font, prompt,
                    new Vector2(screenBounds.Center.X - promptSize.X / 2, screenBounds.Center.Y),
                    Color.Yellow);
                return;
            }

            int padding = 10;
            int topMargin = 50;
            int chartWidth = (screenBounds.Width / 2) - (padding * 2);
            int chartHeight = (screenBounds.Height / 2) - (padding * 2) - (topMargin / 2);

            Rectangle topLeft = new Rectangle(padding, topMargin, chartWidth, chartHeight);
            Rectangle topRight = new Rectangle(padding + chartWidth + padding, topMargin, chartWidth, chartHeight);
            Rectangle bottomLeft = new Rectangle(padding, topMargin + chartHeight + padding, chartWidth, chartHeight);
            Rectangle bottomRight = new Rectangle(padding + chartWidth + padding, topMargin + chartHeight + padding, chartWidth, chartHeight);

            var survivalCurves = new List<float[]>();
            var names = new List<string>();
            var colors = new List<Color> { Color.Red, Color.Cyan, Color.Lime, Color.Orange, Color.Magenta, Color.Yellow, Color.Teal };

            foreach (var group in _controller.RawResults)
            {
                survivalCurves.Add(CalculateSurvivalCurve(group.Value, _controller.Config.RunLength));
                names.Add(group.Key.Replace("Group ", "").Split(' ')[0]);
            }

            ChartRenderer.DrawLineChart(
                spriteBatch, _font, topLeft,
                "Survival Rate by Encounter",
                "Encounter Index",
                "Survival Rate",
                survivalCurves, colors, names);

            var controlGroup = _controller.BatchSummaries.FirstOrDefault(b => b.GroupName.Contains("Control"));
            var combinedGroup = _controller.BatchSummaries.FirstOrDefault(b => b.GroupName.Contains("Combined"));

            if (controlGroup != null && combinedGroup != null)
            {
                string[] labels = { "Spike", "Attrit", "Starve" };

                float[] controlDeaths = {
                    (float)controlGroup.DeathDistribution[DeathClassification.Spike],
                    (float)controlGroup.DeathDistribution[DeathClassification.Attrition],
                    (float)controlGroup.DeathDistribution[DeathClassification.Starvation]
                };

                float[] combinedDeaths = {
                    (float)combinedGroup.DeathDistribution[DeathClassification.Spike],
                    (float)combinedGroup.DeathDistribution[DeathClassification.Attrition],
                    (float)combinedGroup.DeathDistribution[DeathClassification.Starvation]
                };

                Rectangle trLeft = new Rectangle(topRight.X, topRight.Y, topRight.Width / 2 - 5, topRight.Height);
                Rectangle trRight = new Rectangle(topRight.X + topRight.Width / 2 + 5, topRight.Y, topRight.Width / 2 - 5, topRight.Height);

                ChartRenderer.DrawBarChart(
                    spriteBatch, _font, trLeft,
                    "Death Causes (Control)",
                    "Cause",
                    "Freq",
                    labels, controlDeaths, Color.Red,
                    fixedMax: 1.0f);

                ChartRenderer.DrawBarChart(
                    spriteBatch, _font, trRight,
                    "Death Causes (Combined)",
                    "Cause",
                    "Freq",
                    labels, combinedDeaths, Color.Green,
                    fixedMax: 1.0f);
            }

            string[] groupLabels = _controller.BatchSummaries.Select(b => b.GroupName.Substring(6, 1)).ToArray();
            float[] entropyValues = _controller.BatchSummaries.Select(b => (float)b.MeanEntropy).ToArray();

            ChartRenderer.DrawBarChart(
                spriteBatch, _font, bottomLeft,
                "Randomness (Shannon Entropy)",
                "Group",
                "Entropy (Bits)",
                groupLabels, entropyValues, Color.CornflowerBlue,
                fixedMax: 4.0f);

            float[] correctionValues = _controller.BatchSummaries.Select(b => (float)b.AverageCorrectionsPerRun).ToArray();

            ChartRenderer.DrawBarChart(
                spriteBatch, _font, bottomRight,
                "Avg Corrections / Run",
                "Group",
                "Count",
                groupLabels, correctionValues, Color.Orange,
                fixedMax: 20.0f);
        }

        private float[] CalculateSurvivalCurve(List<RunMetric> runs, int runLength)
        {
            float[] curve = new float[runLength + 1];
            int total = runs.Count;

            curve[0] = 1.0f;

            for (int i = 1; i <= runLength; i++)
            {
                int survivedCount = runs.Count(r => r.SurvivalLength >= i);
                curve[i] = (float)survivedCount / total;
            }

            return curve;
        }
    }
}