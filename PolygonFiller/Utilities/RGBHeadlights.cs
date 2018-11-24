using System.Drawing;
using System.Numerics;

namespace PolygonFiller
{
    public class RBGHeadlights
    {
        public int HeadlightsHeight { get; set; }
        public int CosinePower { get; set; }
        public Vector3 RedHeadlightPos { get; private set; }
        public Vector3 GreenHeadlightPos { get; private set; }
        public Vector3 BlueHeadlightPos { get; private set; }

        public Vector3[,] GetRGBHeadlightsMatrix(Size screenSize)
        {
            RedHeadlightPos = new Vector3(screenSize.Width / 2, 0, HeadlightsHeight);
            GreenHeadlightPos = new Vector3(0, screenSize.Height, HeadlightsHeight);
            BlueHeadlightPos = new Vector3(screenSize.Width, screenSize.Height, HeadlightsHeight);

            Vector3 rToMid = Vector3.Normalize(new Vector3(RedHeadlightPos.X - screenSize.Width / 2, RedHeadlightPos.Y - screenSize.Height / 2, RedHeadlightPos.Z));
            Vector3 gToMid = Vector3.Normalize(new Vector3(GreenHeadlightPos.X - screenSize.Width / 2, GreenHeadlightPos.Y - screenSize.Height / 2, GreenHeadlightPos.Z));
            Vector3 bToMid = Vector3.Normalize(new Vector3(BlueHeadlightPos.X - screenSize.Width / 2, BlueHeadlightPos.Y - screenSize.Height / 2, BlueHeadlightPos.Z));

            Vector3[,] result = new Vector3[screenSize.Width, screenSize.Height];
            for (int i = 0; i < screenSize.Width; i++)
            {
                for (int j = 0; j < screenSize.Height; j++)
                {
                    Vector3 curToR = Vector3.Normalize(new Vector3(RedHeadlightPos.X - i, RedHeadlightPos.Y - j, RedHeadlightPos.Z));
                    float rCos = rToMid.X * curToR.X + rToMid.Y * curToR.Y + rToMid.Z * curToR.Z;
                    rCos = System.Math.Abs((float)System.Math.Pow(rCos, CosinePower));

                    Vector3 curToG = Vector3.Normalize(new Vector3(GreenHeadlightPos.X - i, GreenHeadlightPos.Y - j, GreenHeadlightPos.Z));
                    float gCos = gToMid.X * curToG.X + gToMid.Y * curToG.Y + gToMid.Z * curToG.Z;
                    gCos = System.Math.Abs((float)System.Math.Pow(gCos, CosinePower));

                    Vector3 curToB = Vector3.Normalize(new Vector3(BlueHeadlightPos.X - i, BlueHeadlightPos.Y - j, BlueHeadlightPos.Z));
                    float bCos = bToMid.X * curToB.X + bToMid.Y * curToB.Y + bToMid.Z * curToB.Z;
                    bCos = System.Math.Abs((float)System.Math.Pow(bCos, CosinePower));

                    result[i, j] = new Vector3(rCos, gCos, bCos);
                }
            }
            return result;
        }
    }
}