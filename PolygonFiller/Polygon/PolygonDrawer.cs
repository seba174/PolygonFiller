using System.Drawing;

namespace PolygonFiller
{
    public class PolygonDrawer
    {
        public Color VerticeBorderColor { get; set; }
        public Color VerticeInsideColor { get; set; }
        public Color EdgeColor { get; set; }

        public int VerticeRadius { get; set; }
        public int VerticeBorderThickness { get; set; }
        public int EdgeThickness { get; set; }

        public void DrawPolygon(Graphics graphics, IPolygon polygon)
        {
            using (Pen pen = new Pen(EdgeColor, EdgeThickness))
            {
                foreach (Edge edge in polygon.Edges)
                {
                    graphics.DrawLine(pen, edge.Endpoints[0].Position, edge.Endpoints[1].Position);
                }
            }

            using (Brush b = new SolidBrush(VerticeInsideColor))
            {
                using (Pen pen = new Pen(VerticeBorderColor, VerticeBorderThickness))
                {
                    foreach (Vertice vertice in polygon.Vertices)
                    {
                        Point leftUpperCorner = new Point(vertice.Position.X - VerticeRadius, vertice.Position.Y - VerticeRadius);
                        Rectangle rect = new Rectangle(leftUpperCorner.X, leftUpperCorner.Y, 2 * VerticeRadius, 2 * VerticeRadius);

                        graphics.FillEllipse(b, rect);
                        graphics.DrawEllipse(pen, rect);
                    }
                }
            }
        }

        public void FillPolygon(Graphics graphics, IPolygon polygon, DirectBitmap b)
        {
            if (polygon.Vertices.Count == polygon.Edges.Count && polygon.Vertices.Count > 2)
            {
                graphics.DrawImage(b.Bitmap, b.BitmapLocationForDrawing);
            }
        }
    }
}