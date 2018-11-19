using System.Drawing;
using System.Numerics;

namespace PolygonFiller
{
    public static class ExtensionsMethods
    {
        public static void MultiplyByColor(this Vector3 vector, Color color)
        {
            vector.X *= color.R;
            vector.Y *= color.G;
            vector.Z *= color.B;
        }
    }
}
