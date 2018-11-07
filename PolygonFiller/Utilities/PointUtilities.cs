using System;
using System.Drawing;

namespace PolygonFiller
{
    public static class PointUtilities
    {
        public static double GetDistanceBetweenPoints(Point point1, Point point2)
        {
            return Math.Sqrt(SquaredDistanceBetweenPoints(point1, point2));
        }

        public static Point GetPointOnLineWithSpecificDistanceFromStart(Point lineSegmentStart, Point lineSegmentEnd, double distance)
        {
            double lineSegmentLength = GetDistanceBetweenPoints(lineSegmentStart, lineSegmentEnd);
            double ratio = distance / lineSegmentLength;
            int x = (int)((1 - ratio) * lineSegmentStart.X + ratio * lineSegmentEnd.X);
            int y = (int)((1 - ratio) * lineSegmentStart.Y + ratio * lineSegmentEnd.Y);
            return new Point(x, y);
        }

        public static double GetDistanceFromEdge(Edge edge, Point point)
        {
            Point v1 = edge.Endpoints[0].Position;
            Point v2 = edge.Endpoints[1].Position;
            double dx = v2.X - v1.X;
            double dy = v2.Y - v1.Y;
            double dotProduct = (point.X - v1.X) * dx + (point.Y - v1.Y) * dy;
            double lenghtSquared = dx * dx + dy * dy;

            if (lenghtSquared == 0)
            {
                return GetDistanceBetweenPoints(v1, point);
            }

            double t = dotProduct / lenghtSquared;
            if (t < 0)
            {
                dx = v1.X;
                dy = v1.Y;
            }
            else if (t > 1)
            {
                dx = v2.X;
                dy = v2.Y;
            }
            else
            {
                dx = v1.X + t * dx;
                dy = v1.Y + t * dy;
            }
            dx = point.X - dx;
            dy = point.Y - dy;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static int SquaredDistanceBetweenPoints(Point point1, Point point2)
        {
            int centerDx = point1.X - point2.X;
            int centerDy = point1.Y - point2.Y;
            return centerDx * centerDx + centerDy * centerDy;
        }
    }
}
