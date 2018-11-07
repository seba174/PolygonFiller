using System.Drawing;

namespace PolygonFiller
{
    public class Edge : IClickable
    {
        public static int ClickDistance { get; set; }

        public Vertice[] Endpoints { get; private set; }

        public Edge() => Endpoints = new Vertice[2];

        public Vertice GetSecondEndpoint(Vertice endpoint)
        {
            return endpoint == Endpoints[0] ? Endpoints[1] : Endpoints[0];
        }

        bool IClickable.IsClicked(Point position)
        {
            return PointUtilities.GetDistanceFromEdge(this, position) < ClickDistance;
        }
    }
}
