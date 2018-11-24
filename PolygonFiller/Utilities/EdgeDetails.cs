using System.Collections.Generic;

namespace PolygonFiller
{
    public class EdgeDetails
    {
        public int YMax { get; set; }
        public double XofSecondVertice { get; set; }
        public double Step { get; set; }
    }

    public class EdgeDetailsComparer : IComparer<EdgeDetails>
    {
        public int Compare(EdgeDetails x, EdgeDetails y)
        {
            return x.XofSecondVertice.CompareTo(y.XofSecondVertice);
        }
    }
}
