using LingoAITutor.Host.Entities;
using System.Drawing;

namespace LingoAITutor.Host.Utilities
{
    public class WordPositionOnMapCalc
    {
        public static void CaculatePositionsOnTheMap(List<Word> words)
        {
            var random = new Random(999);

            var rectangle = new RectangleF(0, 0, 1920, 1080);
            var randomPoints = new List<PointF>();
            for (int i = 0; i < words.Count(); i++)
            {
                randomPoints.Add(GenerateValidPoint(randomPoints, rectangle, random));
            }

            var center = new PointF(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);

            List<PointF> orderedByDistance = randomPoints.OrderBy(p => (p.X - center.X) * (p.X - center.X) + (p.Y - center.Y) * (p.Y - center.Y)).ToList();

            for (int i = 0; i < words.Count(); i++)
            {
                words[i].XOnMap = (int)Math.Round(orderedByDistance[i].X);
                words[i].YOnMap = (int)Math.Round(orderedByDistance[i].Y);
            }
        }


        private static PointF GenerateValidPoint(List<PointF> existingPoints, RectangleF rectangle, Random random)
        {
            PointF newPoint;
            do
            {
                float x = rectangle.Left + (float)random.NextDouble() * rectangle.Width;
                float y = rectangle.Top + (float)random.NextDouble() * rectangle.Height;
                newPoint = new PointF(x, y);
            }
            while (IsTooCloseToExistingPoints(newPoint, existingPoints));

            return newPoint;
        }

        private static bool IsTooCloseToExistingPoints(PointF point, List<PointF> existingPoints)
        {
            return existingPoints.Any(existingPoint => Distance(point, existingPoint) < 3);
        }

        private static float Distance(PointF a, PointF b)
        {
            return (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

    }
}
