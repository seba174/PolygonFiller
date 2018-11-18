using System.Drawing;

namespace PolygonFiller
{
    public class Vertice : IClickable
    {
        public static int ClickRadius { get; set; }

        public Point Position { get; set; }
        public Edge[] Edges { get; private set; }

        public Vertice() => Edges = new Edge[2];

        public Vertice(Edge edge1, Edge edge2) => Edges = new Edge[] { edge1, edge2 };

        public bool IsClicked(Point position)
        {
            int dx = this.Position.X - position.X;
            int dy = this.Position.Y - position.Y;
            return dx * dx + dy * dy <= ClickRadius * ClickRadius;
        }
    }
}
