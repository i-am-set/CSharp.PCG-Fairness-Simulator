using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Metrics;
using Core.Simulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Visualization
{
    public class Dashboard
    {
        private readonly SimulationController _controller;
        private SpriteFont _font;
        private bool _showDataIndex = false;

        private List<Point> _chunkOrder;
        private int _lastRunCount = -1;
        private int _lastMasterSeed = -1;

        public bool IsSeedMenuOpen { get; private set; } = false;
        private string _seedInputText = "";

        public Dashboard(SimulationController controller)
        {
            _controller = controller;
        }

        public void LoadContent(SpriteFont font, GraphicsDevice graphics)
        {
            _font = font;
            ChartRenderer.Initialize(graphics);
        }

        public void ToggleDataView()
        {
            _showDataIndex = !_showDataIndex;
        }

        public void OpenSeedMenu()
        {
            IsSeedMenuOpen = true;
            _seedInputText = _controller.UseRandomMasterSeed ? "" : _controller.Config.MasterSeed.ToString();
        }

        public void UpdateSeedInput(KeyboardState current, KeyboardState previous)
        {
            if (current.IsKeyDown(Keys.Escape) && previous.IsKeyUp(Keys.Escape))
            {
                IsSeedMenuOpen = false;
                return;
            }

            if (current.IsKeyDown(Keys.Enter) && previous.IsKeyUp(Keys.Enter))
            {
                if (string.IsNullOrWhiteSpace(_seedInputText))
                {
                    _controller.UseRandomMasterSeed = true;
                }
                else if (int.TryParse(_seedInputText, out int newSeed))
                {
                    _controller.UseRandomMasterSeed = false;
                    _controller.Config.MasterSeed = newSeed;
                }
                IsSeedMenuOpen = false;
                return;
            }

            if (current.IsKeyDown(Keys.Back) && previous.IsKeyUp(Keys.Back))
            {
                if (_seedInputText.Length > 0)
                {
                    _seedInputText = _seedInputText.Substring(0, _seedInputText.Length - 1);
                }
                return;
            }

            foreach (Keys key in current.GetPressedKeys())
            {
                if (previous.IsKeyUp(key))
                {
                    if (key >= Keys.D0 && key <= Keys.D9)
                    {
                        _seedInputText += (char)('0' + (key - Keys.D0));
                    }
                    else if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
                    {
                        _seedInputText += (char)('0' + (key - Keys.NumPad0));
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            if (_controller.IsRunning)
            {
                DrawChunkLoader(spriteBatch, screenBounds);
            }

            if (_controller.RawResults != null && _controller.RawResults.Count > 0)
            {
                if (_showDataIndex)
                {
                    DrawDataIndex(spriteBatch, screenBounds);
                }
                else
                {
                    DrawCharts(spriteBatch, screenBounds);
                }
            }
            else if (!_controller.IsRunning)
            {
                DrawPrompt(spriteBatch, screenBounds);
            }

            DrawHeader(spriteBatch, screenBounds);

            if (IsSeedMenuOpen)
            {
                DrawSeedMenu(spriteBatch, screenBounds);
            }
        }

        private void DrawHeader(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            ChartRenderer.DrawRect(spriteBatch, new Rectangle(0, 0, screenBounds.Width, 40), new Color(30, 30, 30));
            ChartRenderer.DrawRect(spriteBatch, new Rectangle(0, 40, screenBounds.Width, 2), new Color(70, 70, 70));

            string status = $"Status: {_controller.StatusMessage} | Progress: {_controller.Progress:P1}";
            spriteBatch.DrawString(_font, status, new Vector2(20, 10), Color.White);

            bool hasData = _controller.RawResults != null && _controller.RawResults.Count > 0;
            string controls = hasData ? "[SPACE] Start  [S] Seed  [TAB] Toggle View  [ESC] Exit" : "[SPACE] Start  [S] Seed  [ESC] Exit";

            Vector2 controlsSize = _font.MeasureString(controls);
            spriteBatch.DrawString(_font, controls, new Vector2(screenBounds.Width - controlsSize.X - 20, 10), Color.LightSkyBlue);

            if (_controller.OutputDirectory != null)
            {
                string openText = "[O] Open Output Folder";
                string dirText = $"Output: {_controller.OutputDirectory}";

                spriteBatch.DrawString(_font, openText, new Vector2(20, screenBounds.Height - 45), Color.LightSkyBlue);
                spriteBatch.DrawString(_font, dirText, new Vector2(20, screenBounds.Height - 25), Color.Gray);
            }
        }

        private void DrawPrompt(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            string prompt = "Press SPACE to Start Simulation";
            Vector2 promptSize = _font.MeasureString(prompt);
            spriteBatch.DrawString(_font, prompt,
                new Vector2(screenBounds.Center.X - promptSize.X / 2, screenBounds.Center.Y),
                Color.Yellow);
        }

        private void DrawChunkLoader(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            int totalRuns = _controller.Config.RunCount;
            int currentSeed = _controller.Config.MasterSeed;

            if (_chunkOrder == null || _lastRunCount != totalRuns || _lastMasterSeed != currentSeed)
            {
                _lastRunCount = totalRuns;
                _lastMasterSeed = currentSeed;

                int side = (int)Math.Ceiling(Math.Sqrt(totalRuns));
                float center = (side - 1) / 2f;

                var tempOrder = new List<Point>(side * side);
                for (int y = 0; y < side; y++)
                {
                    for (int x = 0; x < side; x++)
                    {
                        tempOrder.Add(new Point(x, y));
                    }
                }

                Random rng = new Random(currentSeed);

                float phase1 = (float)(rng.NextDouble() * Math.PI * 2);
                float phase2 = (float)(rng.NextDouble() * Math.PI * 2);
                float phase3 = (float)(rng.NextDouble() * Math.PI * 2);

                float freq1 = 3f + (float)rng.NextDouble() * 2f;
                float freq2 = 7f + (float)rng.NextDouble() * 4f;
                float freq3 = 13f + (float)rng.NextDouble() * 6f;

                tempOrder = tempOrder.OrderBy(p => {
                    float dx = p.X - center;
                    float dy = p.Y - center;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (dist < 1.0f) return 0f;

                    float angle = (float)Math.Atan2(dy, dx);

                    float tendril = (float)(
                        Math.Sin(angle * freq1 + phase1) * 0.15 +
                        Math.Sin(angle * freq2 + phase2) * 0.08 +
                        Math.Sin(angle * freq3 + phase3) * 0.04
                    );

                    float distMod = dist * (1.0f - tendril);

                    float noise = (float)rng.NextDouble() * 3.0f;

                    return distMod + noise;
                }).ToList();

                _chunkOrder = tempOrder.Take(totalRuns).ToList();
            }

            int sideLen = (int)Math.Ceiling(Math.Sqrt(totalRuns));
            int chunkSize = 4;
            int spacing = 1;

            if (sideLen > 100) { chunkSize = 2; spacing = 1; }
            if (sideLen < 50) { chunkSize = 8; spacing = 2; }

            int gridWidth = sideLen * (chunkSize + spacing);
            int gridHeight = sideLen * (chunkSize + spacing);

            int startX = screenBounds.Center.X - gridWidth / 2;
            int startY = screenBounds.Center.Y - gridHeight / 2;

            int chunksToDraw = (int)(_controller.Progress * totalRuns);

            string loadingText = $"SIMULATING... {(int)(_controller.Progress * 100)}%";
            Vector2 textSize = _font.MeasureString(loadingText);
            spriteBatch.DrawString(_font, loadingText, new Vector2(screenBounds.Center.X - textSize.X / 2, startY - 40), Color.Yellow);

            for (int i = chunksToDraw; i < _chunkOrder.Count; i++)
            {
                Point p = _chunkOrder[i];
                int x = startX + p.X * (chunkSize + spacing);
                int y = startY + p.Y * (chunkSize + spacing);
                ChartRenderer.DrawRect(spriteBatch, new Rectangle(x, y, chunkSize, chunkSize), new Color(40, 40, 40));
            }

            for (int i = 0; i < chunksToDraw; i++)
            {
                Point p = _chunkOrder[i];
                int x = startX + p.X * (chunkSize + spacing);
                int y = startY + p.Y * (chunkSize + spacing);

                Color chunkColor;
                int rand = (i * 37) % 100;

                if (rand < 75) chunkColor = new Color(50, 220, 50);
                else if (rand < 85) chunkColor = new Color(220, 50, 50);
                else if (rand < 95) chunkColor = new Color(220, 150, 50);
                else chunkColor = new Color(200, 50, 200);

                ChartRenderer.DrawRect(spriteBatch, new Rectangle(x, y, chunkSize, chunkSize), chunkColor);
            }
        }

        private void DrawCharts(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            int padding = 20;
            int topMargin = 60;
            int bottomMargin = 60;
            int chartWidth = (screenBounds.Width / 2) - (padding * 2) + (padding / 2);
            int chartHeight = (screenBounds.Height / 2) - (padding * 2) - (topMargin / 2) - (bottomMargin / 2);

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

                Rectangle trLeft = new Rectangle(topRight.X, topRight.Y, topRight.Width / 2 - 10, topRight.Height);
                Rectangle trRight = new Rectangle(topRight.X + topRight.Width / 2 + 10, topRight.Y, topRight.Width / 2 - 10, topRight.Height);

                ChartRenderer.DrawBarChart(
                    spriteBatch, _font, trLeft,
                    "Death Causes (Control)",
                    "Cause",
                    "Freq",
                    labels, controlDeaths, new Color(220, 50, 50),
                    fixedMax: 1.0f, fixedMin: 0f);

                ChartRenderer.DrawBarChart(
                    spriteBatch, _font, trRight,
                    "Death Causes (Combined)",
                    "Cause",
                    "Freq",
                    labels, combinedDeaths, new Color(50, 220, 50),
                    fixedMax: 1.0f, fixedMin: 0f);
            }

            string[] groupLabels = _controller.BatchSummaries.Select(b => b.GroupName.Substring(6, 1)).ToArray();
            float[] entropyValues = _controller.BatchSummaries.Select(b => (float)b.MeanEntropy).ToArray();

            float minEntropyVal = entropyValues.Length > 0 ? entropyValues.Min() : 0f;
            float maxEntropyVal = entropyValues.Length > 0 ? entropyValues.Max() : 4f;

            float displayMinEntropy = (float)Math.Floor(minEntropyVal);
            float displayMaxEntropy = (float)Math.Ceiling(maxEntropyVal) + 1f;

            ChartRenderer.DrawBarChart(
                spriteBatch, _font, bottomLeft,
                "Randomness (Shannon Entropy)",
                "Group",
                "Entropy (Bits)",
                groupLabels, entropyValues, new Color(100, 149, 237),
                fixedMax: displayMaxEntropy, fixedMin: displayMinEntropy);

            float[] correctionValues = _controller.BatchSummaries.Select(b => (float)b.AverageCorrectionsPerRun).ToArray();

            ChartRenderer.DrawBarChart(
                spriteBatch, _font, bottomRight,
                "Average Correction/Run",
                "Group",
                "Count",
                groupLabels, correctionValues, new Color(255, 165, 0),
                fixedMax: 20.0f, fixedMin: 0f);
        }

        private void DrawDataIndex(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            ChartRenderer.DrawRect(spriteBatch, screenBounds, new Color(15, 15, 15, 200));

            int startX = 40;
            int startY = 80;
            int rowHeight = 25;
            int colWidth = 180;

            spriteBatch.DrawString(_font, "DATA INDEX - BATCH SUMMARIES", new Vector2(startX, startY), Color.Yellow);
            startY += 40;

            string[] headers = { "Group", "Survival", "Entropy", "DeltaVar", "Corrections", "NearDeath", "FPI Avg" };
            for (int i = 0; i < headers.Length; i++)
            {
                spriteBatch.DrawString(_font, headers[i], new Vector2(startX + (i * colWidth), startY), Color.LightGray);
            }

            startY += rowHeight;
            ChartRenderer.DrawRect(spriteBatch, new Rectangle(startX, startY, screenBounds.Width - (startX * 2), 2), new Color(70, 70, 70));
            startY += 10;

            foreach (var batch in _controller.BatchSummaries)
            {
                string groupName = batch.GroupName.Replace("Group ", "").Split(' ')[0];

                spriteBatch.DrawString(_font, groupName, new Vector2(startX, startY), Color.White);
                spriteBatch.DrawString(_font, batch.SurvivalRate.ToString("P1"), new Vector2(startX + (1 * colWidth), startY), Color.White);
                spriteBatch.DrawString(_font, batch.MeanEntropy.ToString("F3"), new Vector2(startX + (2 * colWidth), startY), Color.White);
                spriteBatch.DrawString(_font, batch.MeanDeltaVariance.ToString("F3"), new Vector2(startX + (3 * colWidth), startY), Color.White);
                spriteBatch.DrawString(_font, batch.AverageCorrectionsPerRun.ToString("F1"), new Vector2(startX + (4 * colWidth), startY), Color.White);
                spriteBatch.DrawString(_font, batch.NearDeathRate.ToString("P1"), new Vector2(startX + (5 * colWidth), startY), Color.White);
                spriteBatch.DrawString(_font, batch.MeanFPI_Avg.ToString("F3"), new Vector2(startX + (6 * colWidth), startY), Color.White);

                startY += rowHeight;
            }
        }

        private void DrawSeedMenu(SpriteBatch spriteBatch, Rectangle screenBounds)
        {
            ChartRenderer.DrawRect(spriteBatch, screenBounds, new Color(0, 0, 0, 200));

            int boxWidth = 400;
            int boxHeight = 200;
            Rectangle box = new Rectangle(
                screenBounds.Center.X - boxWidth / 2,
                screenBounds.Center.Y - boxHeight / 2,
                boxWidth,
                boxHeight
            );

            ChartRenderer.DrawRect(spriteBatch, box, new Color(30, 30, 30));
            ChartRenderer.DrawRect(spriteBatch, new Rectangle(box.X, box.Y, box.Width, 2), Color.White);
            ChartRenderer.DrawRect(spriteBatch, new Rectangle(box.X, box.Bottom - 2, box.Width, 2), Color.White);
            ChartRenderer.DrawRect(spriteBatch, new Rectangle(box.X, box.Y, 2, box.Height), Color.White);
            ChartRenderer.DrawRect(spriteBatch, new Rectangle(box.Right - 2, box.Y, 2, box.Height), Color.White);

            string title = "ENTER MASTER SEED";
            Vector2 titleSize = _font.MeasureString(title);
            spriteBatch.DrawString(_font, title, new Vector2(box.Center.X - titleSize.X / 2, box.Y + 20), Color.Yellow);

            string displaySeed = _seedInputText + "_";
            Vector2 seedSize = _font.MeasureString(displaySeed);
            spriteBatch.DrawString(_font, displaySeed, new Vector2(box.Center.X - seedSize.X / 2, box.Center.Y - 10), Color.White);

            string note = "* Leave blank for randomized seed";
            Vector2 noteSize = _font.MeasureString(note);
            spriteBatch.DrawString(_font, note, new Vector2(box.Center.X - noteSize.X / 2, box.Bottom - 60), Color.Gray);

            string controls = "[ENTER] Save   [ESC] Cancel";
            Vector2 controlsSize = _font.MeasureString(controls);
            spriteBatch.DrawString(_font, controls, new Vector2(box.Center.X - controlsSize.X / 2, box.Bottom - 30), Color.LightSkyBlue);
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