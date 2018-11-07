using System.Collections.Generic;
using System.Drawing;

namespace PolygonFiller
{
    public static class PolygonCreator
    {
        public static Polygon CreatePolygon(List<Point> positionsOfVertices)
        {
            List<Vertice> vertices = new List<Vertice>();
            List<Edge> edges = new List<Edge>();

            foreach (Point position in positionsOfVertices)
            {
                vertices.Add(new Vertice
                {
                    Position = position
                });
            }

            for (int i = 0; i < positionsOfVertices.Count; i++)
            {
                int leftVerticeIndex = i;
                int rightVerticeIndex = (i + 1) % positionsOfVertices.Count;

                Edge edge = new Edge();
                edge.Endpoints[0] = vertices[leftVerticeIndex];
                edge.Endpoints[1] = vertices[rightVerticeIndex];

                vertices[leftVerticeIndex].Edges[1] = edge;
                vertices[rightVerticeIndex].Edges[0] = edge;
                edges.Add(edge);
            }

            return new Polygon(vertices, edges);
        }

        public static Polygon GetTriangle()
        {
            List<Point> verticesPositions = new List<Point> { new Point(200, 300), new Point(400, 300), new Point(300, 150) };
            return CreatePolygon(verticesPositions);
        }
    }
}
