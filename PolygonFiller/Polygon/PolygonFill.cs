using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace PolygonFiller
{
    public static class PolygonFill
    {
        public static DirectBitmap GetPolygonFillDirectBitmap(IPolygon polygon)
        {
            int xMin = polygon.Vertices.Min(v => v.Position.X);
            int yMin = polygon.Vertices.Min(v => v.Position.Y);

            DirectBitmap directBitmap = new DirectBitmap(polygon.Vertices.Max(v => v.Position.X) - xMin + 1, polygon.Vertices.Max(v => v.Position.Y) - yMin + 1)
            {
                BitmapLocationForDrawing = new Point(xMin, yMin)
            };

            List<EdgeDetails>[] et = new List<EdgeDetails>[directBitmap.Height - 1];

            foreach (var edge in polygon.Edges)
            {
                int dx = edge.Endpoints[0].Position.X - edge.Endpoints[1].Position.X;
                int dy = edge.Endpoints[0].Position.Y - edge.Endpoints[1].Position.Y;
                if (dy == 0)
                    continue;

                Vertice pointWithMaxY = edge.Endpoints.Where(v => v.Position.Y == edge.Endpoints.Max(vertice => vertice.Position.Y)).First();
                EdgeDetails bucket = new EdgeDetails
                {
                    YMax = pointWithMaxY.Position.Y,
                    XofSecondVertice = edge.GetSecondEndpoint(pointWithMaxY).Position.X,
                    Step = (double)dx / dy
                };

                int indexInEtTable = edge.GetSecondEndpoint(pointWithMaxY).Position.Y - yMin;
                if (et[indexInEtTable] == null)
                {
                    et[indexInEtTable] = new List<EdgeDetails> { bucket };
                }
                else
                {
                    et[indexInEtTable].Add(bucket);
                }
            }

            List<EdgeDetails> aet = new List<EdgeDetails>();
            EdgeDetailsComparer bucketComparer = new EdgeDetailsComparer();

            for (int y = 0; y < et.Length; y++)
            {
                if (et[y] != null)
                    aet.AddRange(et[y]);

                aet.Sort(bucketComparer);

                for (int i = 0; i < aet.Count - 1; i++)
                {
                    for (double j = aet[i].XofSecondVertice; j < aet[i + 1].XofSecondVertice; j++)
                    {
                        directBitmap.SetPixel((int)(j - xMin), y, Color.Yellow);
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
    }

    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        public Point BitmapLocationForDrawing { get; set; }

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void SetPixel(int x, int y, Color colour)
        {
            int index = x + (y * Width);
            int col = colour.ToArgb();

            Bits[index] = col;
        }

        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Bits[index];
            Color result = Color.FromArgb(col);

            return result;
        }

        public void Dispose()
        {
            if (Disposed)
                return;

            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }

    public class EdgeDetailsComparer : IComparer<EdgeDetails>
    {
        public int Compare(EdgeDetails x, EdgeDetails y)
        {
            return x.XofSecondVertice.CompareTo(y.XofSecondVertice);
        }
    }

    public class EdgeDetails
    {
        public int YMax { get; set; }
        public double XofSecondVertice { get; set; }
        public double Step { get; set; }
    }
}
