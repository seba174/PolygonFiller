using System.Drawing;
using System.Numerics;

namespace PolygonFiller
{
    public class ColorsForPolygonFill
    {
        public static Vector3 ConstantVectorToLight { get; set; }
        public static Vector3 ConstantNormalVector { get; set; }
        public static Vector3 ConstantDisruptionVector { get; set; }

        public ObjectColorOption ObjectColorOption { get; set; }
        public VectorToLightOption VectorToLightOption { get; set; }
        public NormalVectorOption NormalVectorOption { get; set; }
        public DisruptionVectorOption DisruptionVectorOption { get; set; }

        public Vector3 LightColor { get; set; }
        public Color ObjectColor { get; set; }
        public Vector3 LightSourcePosition { get; set; }

        public Size DrawingAreaSize { get; set; }

        public RBGHeadlights RBGHeadlights { get; set; }
        public bool RGBHeadlightsEnabled { get; set; }

        private Color[,] colorsFromObjectTexture;
        private Vector3[,] normalMapVectors;
        private Vector3[,] heightMapVectors;
        private Vector3[,] rgbHeadlights;

        private int[,] CachedColors;

        public int GetColor(int x, int y)
        {
            int rx = x < 0 ? 0 : x;
            int ry = y < 0 ? 0 : y;
            return CachedColors[rx % DrawingAreaSize.Width, ry % DrawingAreaSize.Height];
        }

        public void SetObjectTexture(Bitmap objectTexture)
        {
            using (var fastBitmap = objectTexture.GetLockedFastBitmap())
            {
                colorsFromObjectTexture = new Color[fastBitmap.Width, fastBitmap.Height];
                for (int i = 0; i < fastBitmap.Width; i++)
                {
                    for (int j = 0; j < fastBitmap.Height; j++)
                    {
                        colorsFromObjectTexture[i, j] = fastBitmap.GetPixel(i, j);
                    }
                }
            }
            if (ObjectColorOption == ObjectColorOption.FromTexture)
            {
                UpdateCache();
            }
        }

        public void SetNormalMap(Bitmap normalMap)
        {
            using (var fastBitmap = normalMap.GetLockedFastBitmap())
            {
                normalMapVectors = new Vector3[fastBitmap.Width, fastBitmap.Height];

                float normalizationFactor = 127;
                for (int i = 0; i < fastBitmap.Width; i++)
                {
                    for (int j = 0; j < fastBitmap.Height; j++)
                    {
                        Color color = fastBitmap.GetPixel(i, j);

                        float x = color.R / normalizationFactor - 1;
                        float y = color.G / normalizationFactor - 1;
                        float z = color.B / normalizationFactor;

                        if (z == 0)
                        {
                            normalMapVectors[i, j] = new Vector3(x, y, 0);
                        }
                        else
                        {
                            normalMapVectors[i, j] = new Vector3(x / z, y / z, 1);
                        }
                    }
                }
            }
            if (NormalVectorOption == NormalVectorOption.FromNormalMap)
            {
                UpdateCache();
            }
        }

        public void SetHeightMap(Bitmap heightMap)
        {
            using (var fastBitmap = heightMap.GetLockedFastBitmap())
            {
                heightMapVectors = new Vector3[fastBitmap.Width, fastBitmap.Height];
                float factor = 0.01f;

                for (int i = 0; i < fastBitmap.Width; i++)
                {
                    for (int j = 0; j < fastBitmap.Height; j++)
                    {
                        Color color = fastBitmap.GetPixel(i, j);
                        Vector3 normalVector = GetNormalVector(i, j);

                        if (normalVector.Z == 0)
                        {
                            heightMapVectors[i, j] = new Vector3();
                        }
                        else
                        {
                            Color colorNextX = fastBitmap.GetPixel((i + 1) % fastBitmap.Width, j);
                            Color colorNextY = fastBitmap.GetPixel(i, (j + 1) % fastBitmap.Height);

                            Vector3 t = new Vector3(1, 0, -normalVector.X);
                            Vector3 b = new Vector3(0, 1, -normalVector.Y);
                            int dhx = colorNextX.R - color.R;
                            int dhy = colorNextY.R - color.R;

                            heightMapVectors[i, j] = factor * (t * dhx + b * dhy);
                        }
                    }
                }
            }
            if (DisruptionVectorOption == DisruptionVectorOption.FromHeightMap)
            {
                UpdateCache();
            }
        }

        private Vector3 GetNormalVector(int x, int y)
        {
            return NormalVectorOption == NormalVectorOption.Constant
                ? ConstantNormalVector
                : normalMapVectors[x % normalMapVectors.GetLength(0), y % normalMapVectors.GetLength(1)];
        }

        public void EnableRgbHeadlights()
        {
            rgbHeadlights = RBGHeadlights.GetRGBHeadlightsMatrix(DrawingAreaSize);
            RGBHeadlightsEnabled = true;
        }

        public void DisableRgbHeadlights()
        {
            rgbHeadlights = null;
            RGBHeadlightsEnabled = false;
        }

        public void UpdateCache()
        {
            CachedColors = CreateCache();
        }

        public int[,] CreateCache()
        {
            var cache = new int[DrawingAreaSize.Width, DrawingAreaSize.Height];

            for (int x = 0; x < DrawingAreaSize.Width; x++)
            {
                for (int y = 0; y < DrawingAreaSize.Height; y++)
                {
                    int rx = x < 0 ? 0 : x;
                    int ry = y < 0 ? 0 : y;

                    float r = LightColor.X, g = LightColor.Y, b = LightColor.Z;
                    float headLightR = 0, headLightG = 0, headLightB = 0;

                    if (rgbHeadlights != null && RGBHeadlightsEnabled)
                    {
                        Vector3 colorsFromHeadlights = rgbHeadlights[rx % rgbHeadlights.GetLength(0), ry % rgbHeadlights.GetLength(1)];
                        headLightR = colorsFromHeadlights.X;
                        headLightG = colorsFromHeadlights.Y;
                        headLightB = colorsFromHeadlights.Z;
                    }
                    if (ObjectColorOption == ObjectColorOption.Constant)
                    {
                        r *= ObjectColor.R;
                        g *= ObjectColor.G;
                        b *= ObjectColor.B;
                        headLightR *= ObjectColor.R;
                        headLightG *= ObjectColor.G;
                        headLightB *= ObjectColor.B;
                    }
                    else
                    {
                        int textureX = rx % colorsFromObjectTexture.GetLength(0);
                        int textureY = ry % colorsFromObjectTexture.GetLength(1);

                        r *= colorsFromObjectTexture[textureX, textureY].R;
                        g *= colorsFromObjectTexture[textureX, textureY].G;
                        b *= colorsFromObjectTexture[textureX, textureY].B;
                        headLightR *= colorsFromObjectTexture[textureX, textureY].R;
                        headLightG *= colorsFromObjectTexture[textureX, textureY].G;
                        headLightB *= colorsFromObjectTexture[textureX, textureY].B;
                    }

                    Vector3 N;
                    if (NormalVectorOption == NormalVectorOption.Constant)
                    {
                        N = ConstantNormalVector;
                    }
                    else
                    {
                        N = normalMapVectors[rx % normalMapVectors.GetLength(0), ry % normalMapVectors.GetLength(1)];
                    }

                    if (DisruptionVectorOption == DisruptionVectorOption.None)
                    {
                        N += ConstantDisruptionVector;
                    }
                    else
                    {
                        N += heightMapVectors[rx % heightMapVectors.GetLength(0), ry % heightMapVectors.GetLength(1)];
                    }

                    N = Vector3.Normalize(N);

                    float cos, cosR, cosG, cosB;
                    Vector3 cosHeadlights = new Vector3();
                    if (VectorToLightOption == VectorToLightOption.Constant)
                    {
                        cos = N.X * ConstantVectorToLight.X + N.Y * ConstantVectorToLight.Y + N.Z * ConstantVectorToLight.Z;
                    }
                    else
                    {
                        Vector3 vectorToLight = Vector3.Normalize(new Vector3(LightSourcePosition.X - x, LightSourcePosition.Y - y, LightSourcePosition.Z));
                        cos = N.X * vectorToLight.X + N.Y * vectorToLight.Y + N.Z * vectorToLight.Z;
                    }
                    Vector3 vecToHeadlight = Vector3.Normalize(new Vector3(RBGHeadlights.rPos.X - x, -RBGHeadlights.rPos.Y - y, RBGHeadlights.rPos.Z));
                    cosHeadlights.X = N.X * vecToHeadlight.X + N.Y * vecToHeadlight.Y + N.Z * vecToHeadlight.Z;
                    cosR = N.X * vecToHeadlight.X + N.Y * vecToHeadlight.Y + N.Z * vecToHeadlight.Z;
                    cosR = cosR < 0 ? 0 : cosR;
                    vecToHeadlight = Vector3.Normalize(new Vector3(RBGHeadlights.gPos.X - x, RBGHeadlights.gPos.Y - y, RBGHeadlights.gPos.Z));
                    cosHeadlights.Y = N.X * vecToHeadlight.X + N.Y * vecToHeadlight.Y + N.Z * vecToHeadlight.Z;
                    cosG = N.X * vecToHeadlight.X + N.Y * vecToHeadlight.Y + N.Z * vecToHeadlight.Z;
                    cosG = cosG < 0 ? 0 : cosG;
                    vecToHeadlight = Vector3.Normalize(new Vector3(RBGHeadlights.bPos.X - x, RBGHeadlights.bPos.Y - y, RBGHeadlights.bPos.Z));
                    cosHeadlights.Z = N.X * vecToHeadlight.X + N.Y * vecToHeadlight.Y + N.Z * vecToHeadlight.Z;
                    cosB = N.X * vecToHeadlight.X + N.Y * vecToHeadlight.Y + N.Z * vecToHeadlight.Z;
                    cosB = cosB < 0 ? 0 : cosB;

                    cos = cos < 0 ? 0 : cos;

                    r *= cos;
                    g *= cos;
                    b *= cos;
                    headLightR *= cosR;
                    headLightG *= cosG;
                    headLightB *= cosB;

                    cache[x, y] =
                        255 << 24
                        | ((int)(r + headLightR) > 255 ? 255 : (int)(r + headLightR)) << 16
                        | ((int)(g + headLightG) > 255 ? 255 : (int)(g + headLightG)) << 8
                        | ((int)(b + headLightB) > 255 ? 255 : (int)(b + headLightB));
                }
            }
            return cache;
        }
    }

    public enum ObjectColorOption
    {
        Constant,
        FromTexture
    }

    public enum VectorToLightOption
    {
        Constant,
        Variable
    }

    public enum NormalVectorOption
    {
        Constant,
        FromNormalMap
    }

    public enum DisruptionVectorOption
    {
        None,
        FromHeightMap
    }

    public class RBGHeadlights
    {
        public int HeadlightsHeight { get; set; }
        public int CosinePower { get; set; }
        public Vector3 rPos { get; private set; }
        public Vector3 gPos { get; private set; }
        public Vector3 bPos { get; private set; }

        public Vector3[,] GetRGBHeadlightsMatrix(Size screenSize)
        {
            rPos = new Vector3(screenSize.Width / 2, 0, HeadlightsHeight);
            gPos = new Vector3(0, screenSize.Height, HeadlightsHeight);
            bPos = new Vector3(screenSize.Width, screenSize.Height, HeadlightsHeight);

            Vector3 rToMid = Vector3.Normalize(new Vector3(rPos.X - screenSize.Width / 2, rPos.Y - screenSize.Height / 2, rPos.Z));
            Vector3 gToMid = Vector3.Normalize(new Vector3(gPos.X - screenSize.Width / 2, gPos.Y - screenSize.Height / 2, gPos.Z));
            Vector3 bToMid = Vector3.Normalize(new Vector3(bPos.X - screenSize.Width / 2, bPos.Y - screenSize.Height / 2, bPos.Z));

            Vector3[,] result = new Vector3[screenSize.Width, screenSize.Height];
            for (int i = 0; i < screenSize.Width; i++)
            {
                for (int j = 0; j < screenSize.Height; j++)
                {
                    Vector3 curToR = Vector3.Normalize(new Vector3(rPos.X - i, rPos.Y - j, rPos.Z));
                    float rCos = rToMid.X * curToR.X + rToMid.Y * curToR.Y + rToMid.Z * curToR.Z;
                    rCos = System.Math.Abs((float)System.Math.Pow(rCos, CosinePower));

                    Vector3 curToG = Vector3.Normalize(new Vector3(gPos.X - i, gPos.Y - j, gPos.Z));
                    float gCos = gToMid.X * curToG.X + gToMid.Y * curToG.Y + gToMid.Z * curToG.Z;
                    gCos = System.Math.Abs((float)System.Math.Pow(gCos, CosinePower));

                    Vector3 curToB = Vector3.Normalize(new Vector3(bPos.X - i, bPos.Y - j, bPos.Z));
                    float bCos = bToMid.X * curToB.X + bToMid.Y * curToB.Y + bToMid.Z * curToB.Z;
                    bCos = System.Math.Abs((float)System.Math.Pow(bCos, CosinePower));

                    result[i, j] = new Vector3(rCos, gCos, bCos);
                }
            }
            return result;
        }
    }
}
