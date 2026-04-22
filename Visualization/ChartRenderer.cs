using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Visualization
{
    public static class ChartRenderer
    {
        private static Texture2D _pixel;

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public static void DrawRect(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            spriteBatch.Draw(_pixel, rect, color);
        }

        public static void DrawBarChart(
            SpriteBatch spriteBatch,
            SpriteFont font,
            Rectangle bounds,
            string title,
            string xAxisLabel,
            string yAxisLabel,
            string[] labels,
            float[] values,
            Color barColor,
            float? fixedMax = null,
            float? fixedMin = null)
        {
            DrawRect(spriteBatch, bounds, new Color(25, 25, 25, 128));
            DrawRect(spriteBatch, new Rectangle(bounds.X, bounds.Y, bounds.Width, 1), new Color(50, 50, 50));
            DrawRect(spriteBatch, new Rectangle(bounds.X, bounds.Bottom - 1, bounds.Width, 1), new Color(50, 50, 50));
            DrawRect(spriteBatch, new Rectangle(bounds.X, bounds.Y, 1, bounds.Height), new Color(50, 50, 50));
            DrawRect(spriteBatch, new Rectangle(bounds.Right - 1, bounds.Y, 1, bounds.Height), new Color(50, 50, 50));

            Vector2 titleSize = font.MeasureString(title);
            spriteBatch.DrawString(font, title, new Vector2(bounds.X + (bounds.Width - titleSize.X) / 2, bounds.Y + 10), Color.White);

            int marginLeft = 80;
            int marginBottom = 50;
            int marginTop = 40;
            int marginRight = 20;

            int chartBottom = bounds.Bottom - marginBottom;
            int chartTop = bounds.Y + marginTop;
            int chartHeight = chartBottom - chartTop;
            int chartLeft = bounds.X + marginLeft;
            int chartWidth = bounds.Width - marginLeft - marginRight;

            float maxVal = fixedMax ?? (values.Length > 0 ? values.Max() : 1f);
            float minVal = fixedMin ?? 0f;
            if (maxVal <= minVal) maxVal = minVal + 1f;
            float range = maxVal - minVal;

            DrawRect(spriteBatch, new Rectangle(chartLeft, chartTop, 2, chartHeight), Color.Gray);
            DrawRect(spriteBatch, new Rectangle(chartLeft, chartBottom, chartWidth, 2), Color.Gray);

            DrawYAxisLabel(spriteBatch, font, minVal.ToString("0.##"), chartLeft, chartBottom, true);
            DrawYAxisLabel(spriteBatch, font, (minVal + range * 0.5f).ToString("0.##"), chartLeft, chartTop + chartHeight / 2, true);
            DrawYAxisLabel(spriteBatch, font, maxVal.ToString("0.##"), chartLeft, chartTop, true);

            Vector2 xLabelSize = font.MeasureString(xAxisLabel);
            spriteBatch.DrawString(font, xAxisLabel,
                new Vector2(chartLeft + (chartWidth - xLabelSize.X) / 2, bounds.Bottom - 25),
                Color.LightGray);

            Vector2 yLabelSize = font.MeasureString(yAxisLabel);
            spriteBatch.DrawString(font, yAxisLabel,
                new Vector2(bounds.X + 15, chartTop + (chartHeight + yLabelSize.X) / 2),
                Color.LightGray,
                -MathHelper.PiOver2,
                Vector2.Zero,
                1.0f,
                SpriteEffects.None,
                0f);

            if (values.Length == 0) return;

            int barWidth = (chartWidth / values.Length) - 10;
            if (barWidth < 5) barWidth = 5;

            for (int i = 0; i < values.Length; i++)
            {
                float val = MathHelper.Clamp(values[i], minVal, maxVal);
                float normalizedHeight = (val - minVal) / range;
                int barHeight = (int)(normalizedHeight * chartHeight);

                Rectangle barRect = new Rectangle(
                    chartLeft + (i * (barWidth + 10)) + 10,
                    chartBottom - barHeight,
                    barWidth,
                    barHeight
                );

                DrawRect(spriteBatch, barRect, barColor);

                string label = labels[i].Length > 6 ? labels[i].Substring(0, 6) : labels[i];
                Vector2 labelSize = font.MeasureString(label);
                spriteBatch.DrawString(font, label,
                    new Vector2(barRect.Center.X - labelSize.X / 2, chartBottom + 5),
                    Color.White * 0.8f);

                string valStr = values[i].ToString("0.##");
                Vector2 valSize = font.MeasureString(valStr);
                spriteBatch.DrawString(font, valStr,
                    new Vector2(barRect.Center.X - valSize.X / 2, barRect.Y - 20),
                    Color.Yellow * 0.9f);
            }
        }

        public static void DrawLineChart(
            SpriteBatch spriteBatch,
            SpriteFont font,
            Rectangle bounds,
            string title,
            string xAxisLabel,
            string yAxisLabel,
            List<float[]> seriesList,
            List<Color> colors,
            List<string> seriesNames)
        {
            DrawRect(spriteBatch, bounds, new Color(25, 25, 25, 128));
            DrawRect(spriteBatch, new Rectangle(bounds.X, bounds.Y, bounds.Width, 1), new Color(50, 50, 50));
            DrawRect(spriteBatch, new Rectangle(bounds.X, bounds.Bottom - 1, bounds.Width, 1), new Color(50, 50, 50));
            DrawRect(spriteBatch, new Rectangle(bounds.X, bounds.Y, 1, bounds.Height), new Color(50, 50, 50));
            DrawRect(spriteBatch, new Rectangle(bounds.Right - 1, bounds.Y, 1, bounds.Height), new Color(50, 50, 50));

            int marginLeft = 100;
            int marginBottom = 50;
            int marginTop = 50;
            int marginRight = 90;

            int chartBottom = bounds.Bottom - marginBottom;
            int chartTop = bounds.Y + marginTop;
            int chartHeight = chartBottom - chartTop;
            int chartLeft = bounds.X + marginLeft;
            int chartWidth = bounds.Width - marginLeft - marginRight;

            Vector2 titleSize = font.MeasureString(title);
            spriteBatch.DrawString(font, title, new Vector2(chartLeft + (chartWidth - titleSize.X) / 2, bounds.Y + 10), Color.White);

            int legendX = chartLeft + chartWidth + 15;
            int legendY = bounds.Y + 30;
            for (int i = 0; i < seriesNames.Count; i++)
            {
                Color c = colors[i % colors.Count];
                DrawRect(spriteBatch, new Rectangle(legendX, legendY + (i * 20), 12, 12), c);
                spriteBatch.DrawString(font, seriesNames[i], new Vector2(legendX + 20, legendY + (i * 20) - 2), Color.White * 0.9f);
            }

            DrawRect(spriteBatch, new Rectangle(chartLeft, chartTop, 2, chartHeight), Color.Gray);
            DrawRect(spriteBatch, new Rectangle(chartLeft, chartBottom, chartWidth, 2), Color.Gray);

            float[] gridPoints = { 0.0f, 0.25f, 0.5f, 0.75f, 1.0f };
            foreach (float p in gridPoints)
            {
                int yPos = chartBottom - (int)(p * chartHeight);

                DrawRect(spriteBatch, new Rectangle(chartLeft, yPos, chartWidth, 1), Color.Gray * 0.2f);

                DrawYAxisLabel(spriteBatch, font, p.ToString("0.00"), chartLeft, yPos, true);
            }

            Vector2 xLabelSize = font.MeasureString(xAxisLabel);
            spriteBatch.DrawString(font, xAxisLabel,
                new Vector2(chartLeft + (chartWidth - xLabelSize.X) / 2, bounds.Bottom - 25),
                Color.LightGray);

            Vector2 yLabelSize = font.MeasureString(yAxisLabel);
            spriteBatch.DrawString(font, yAxisLabel,
                new Vector2(bounds.X + 15, chartTop + (chartHeight + yLabelSize.X) / 2),
                Color.LightGray,
                -MathHelper.PiOver2,
                Vector2.Zero,
                1.0f,
                SpriteEffects.None,
                0f);

            if (seriesList.Count == 0) return;

            int points = seriesList[0].Length;
            float xStep = (float)chartWidth / (points - 1);

            Vector2 zeroSize = font.MeasureString("0");
            spriteBatch.DrawString(font, "0", new Vector2(chartLeft - zeroSize.X / 2, chartBottom + 5), Color.Gray);

            string maxStr = (points - 1).ToString();
            Vector2 maxSize = font.MeasureString(maxStr);
            spriteBatch.DrawString(font, maxStr, new Vector2(chartLeft + chartWidth - maxSize.X / 2, chartBottom + 5), Color.Gray);

            for (int s = 0; s < seriesList.Count; s++)
            {
                float[] data = seriesList[s];
                Color c = colors[s % colors.Count];

                for (int i = 0; i < data.Length - 1; i++)
                {
                    Vector2 start = new Vector2(chartLeft + (i * xStep), chartBottom - (data[i] * chartHeight));
                    Vector2 end = new Vector2(chartLeft + ((i + 1) * xStep), chartBottom - (data[i + 1] * chartHeight));
                    DrawLine(spriteBatch, start, end, c, 2);
                }
            }
        }

        private static void DrawYAxisLabel(SpriteBatch spriteBatch, SpriteFont font, string text, int axisX, int yPos, bool drawGridLine)
        {
            Vector2 size = font.MeasureString(text);
            spriteBatch.DrawString(font, text, new Vector2(axisX - size.X - 10, yPos - size.Y / 2), Color.Gray);
        }

        private static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            spriteBatch.Draw(_pixel, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness), null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }
    }
}