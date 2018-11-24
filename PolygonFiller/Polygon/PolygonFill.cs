using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PolygonFiller
{
    public static class PolygonFill
    {
        public static DirectBitmap GetPolygonFillDirectBitmap(IPolygon polygon, ColorsForPolygonFill colorsForPolygonFill)
        {
            int xMin = polygon.Vertices.Min(v => v.Position.X);
            int yMin = polygon.Vertices.Min(v => v.Position.Y);

            DirectBitmap directBitmap = new DirectBitmap(polygon.Vertices.Max(v => v.Position.X) - xMin + 1, polygon.Vertices.Max(v => v.Position.Y) - yMin + 1)
            {
                BitmapLocationForDrawing = new Point(xMin, yMin)
            };

            List<EdgeDetails>[] et = new List<EdgeDetails>[directBitmap.Height - 1];
            PrepareEdgeDetails(et, polygon.Edges, xMin, yMin);

            List<EdgeDetails> aet = new List<EdgeDetails>();
            EdgeDetailsComparer bucketComparer = new EdgeDetailsComparer();

            for (int y = 0; y < et.Length; y++)
            {
                if (et[y] != null)
                {
                    aet.AddRange(et[y]);
                }

                aet.Sort(bucketComparer);

                for (int i = 0; i < aet.Count - 1; i++)
                {
                    int loopEnd = (int)aet[i + 1].XofSecondVertice;
                    for (int j = (int)aet[i].XofSecondVertice; j < loopEnd; j++)
                    {
                        directBitmap.SetPixel(j, y, colorsForPolygonFill.GetColor(j + xMin, y + yMin));
                    }
                }

                aet.RemoveAll(b => b.YMax == y + yMin);

                foreach (var bucket in aet)
                {
                    bucket.XofSecondVertice += bucket.Step;
                }
            }

            return directBitmap;
        }

        private static void PrepareEdgeDetails(List<EdgeDetails>[] edgeDetails, List<Edge> edges, int xMin, int yMin)
        {
            foreach (var edge in edges)
            {
                int dx = edge.Endpoints[0].Position.X - edge.Endpoints[1].Position.X;
                int dy = edge.Endpoints[0].Position.Y - edge.Endpoints[1].Position.Y;
                if (dy == 0)
                {
                    continue;
                }

                Vertice pointWithMaxY = edge.Endpoints.Where(v => v.Position.Y == edge.Endpoints.Max(vertice => vertice.Position.Y)).First();
                EdgeDetails bucket = new EdgeDetails
                {
                    YMax = pointWithMaxY.Position.Y,
                    XofSecondVertice = edge.GetSecondEndpoint(pointWithMaxY).Position.X - xMin,
                    Step = (double)dx / dy
                };

                int indexInEtTable = edge.GetSecondEndpoint(pointWithMaxY).Position.Y - yMin;
                if (edgeDetails[indexInEtTable] == null)
                {
                    edgeDetails[indexInEtTable] = new List<EdgeDetails> { bucket };
                }
                else
                {
                    edgeDetails[indexInEtTable].Add(bucket);
                }
            }
        }
    }
}
