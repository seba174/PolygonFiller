﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PolygonFiller
{
    public class Polygon : IPolygon
    {
        public List<Vertice> Vertices { get; private set; }
        public List<Edge> Edges { get; private set; }
        public IEnumerable<IClickable> Clickables => Vertices.Concat(Edges.Cast<IClickable>());

        public Polygon(IEnumerable<Vertice> vertices, IEnumerable<Edge> edges)
        {
            Vertices = vertices.ToList();
            Edges = edges.ToList();
        }

        public bool HandleClickableMove(IClickable clickable, Point offset)
        {
            if (clickable is Vertice vertice)
            {
                return MoveVertice(vertice, offset);
            }
            return false;
        }

        public bool HandlePolygonMove(Point offset)
        {
            foreach (var vertice in Vertices)
            {
                vertice.Position = new Point(vertice.Position.X + offset.X, vertice.Position.Y + offset.Y);
            }
            return true;
        }

        private bool MoveVertice(Vertice original, Point offset)
        {
            original.Position = new Point(original.Position.X + offset.X, original.Position.Y + offset.Y);
            return true;
        }
    }
}
