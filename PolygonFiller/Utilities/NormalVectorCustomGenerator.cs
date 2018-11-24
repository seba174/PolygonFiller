using System;
using System.Drawing;
using System.Numerics;

namespace PolygonFiller
{
    public class NormalVectorCustomGenerator
    {
        public float A { get; set; }
        public float B { get; set; }

        private int time = 0;
        private const int baseMultiplier = 6;

        public Vector3[,] GetCustomNormalVectorsArray(Size screenSize)
        {
            Vector3[,] result = new Vector3[screenSize.Width, screenSize.Height];
            for (int x = 0; x < screenSize.Width; x++)
            {
                for (int y = 0; y < screenSize.Height; y++)
                {
                    // f(x,y) = sin(a*x + t) * cos(b*x + t)

                    float dx = (float)(A * Math.Cos(A * x + time) * Math.Cos(B * y + time));
                    float dy = (float)(-B * Math.Sin(A * x + time) * Math.Sin(B * y + time));
                    result[x, y] = Vector3.Normalize(new Vector3(dx * baseMultiplier, dy * baseMultiplier, 1));
                }
            }

            time = TimeTicker.GetNextTimeTickToAnimation(time);

            return result;
        }
    }
}
