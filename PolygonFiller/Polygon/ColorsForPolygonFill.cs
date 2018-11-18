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

        private Color[,] colorsFromObjectTexture;
        private Vector3[,] normalMapVectors;
        private Vector3[,] heightMapVectors;

        public int GetColor(int x, int y)
        {
            int rx = x < 0 ? 0 : x;
            int ry = y < 0 ? 0 : y;

            float r = LightColor.X;
            float g = LightColor.Y;
            float b = LightColor.Z;
            if (ObjectColorOption == ObjectColorOption.Constant)
            {
                r *= ObjectColor.R;
                g *= ObjectColor.G;
                b *= ObjectColor.B;
            }
            else
            {
                int textureX = rx % colorsFromObjectTexture.GetLength(0);
                int textureY = ry % colorsFromObjectTexture.GetLength(1);

                r *= colorsFromObjectTexture[textureX, textureY].R;
                g *= colorsFromObjectTexture[textureX, textureY].G;
                b *= colorsFromObjectTexture[textureX, textureY].B;
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

            float cos;
            if (VectorToLightOption == VectorToLightOption.Constant)
            {
                cos = N.X * ConstantVectorToLight.X + N.Y * ConstantVectorToLight.Y + N.Z * ConstantVectorToLight.Z;
            }
            else
            {
                Vector3 vectorToLight = Vector3.Normalize(new Vector3(LightSourcePosition.X - x, LightSourcePosition.Y - y, LightSourcePosition.Z));
                cos = N.X * vectorToLight.X + N.Y * vectorToLight.Y + N.Z * vectorToLight.Z;
            }

            cos = cos < 0 ? 0 : cos;

            r *= cos;
            g *= cos;
            b *= cos;

            return 255 << 24 | ((int)r > 255 ? 255 : (int)r) << 16 | ((int)g > 255 ? 255 : (int)g) << 8 | ((int)b > 255 ? 255 : (int)b);
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
        }

        private Vector3 GetNormalVector(int x, int y)
        {
            return NormalVectorOption == NormalVectorOption.Constant
                ? ConstantNormalVector
                : normalMapVectors[x % normalMapVectors.GetLength(0), y % normalMapVectors.GetLength(1)];
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
}
