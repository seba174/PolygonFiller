using System.Collections.Generic;
using System.Drawing;

namespace PolygonFiller
{
    public interface IPolygon
    {
        IEnumerable<IClickable> Clickables { get; }

        List<Vertice> Vertices { get; }

        List<Edge> Edges { get; }

        bool HandleClickableMove(IClickable clickableElement, Point offset);

        bool HandlePolygonMove(Point offset);

        bool IsPolygonMovingPermitted(Point offset, Rectangle area);

        bool IsClickableMovingPermitted(IClickable clickable, Point offset, Rectangle area);
    }
}
