using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PolygonFiller
{
    public class ColorsForPolygonFill
    {
        public static Vector3 ConstantVectorToLight { get; set; }
        public static Vector3 ConstantNormalVector { get; set; }
        public static Vector3 ConstantDisruptionVector { get; set; }

        public Point Offset { get; set; }
        public ObjectColorOption ObjectColorOption { get; set; }
        public VectorToLightOption VectorToLightOption { get; set; }
        public NormalVectorOption NormalVectorOption { get; set; }
        public DisruptionVectorOption DisruptionVectorOption { get; set; }

        public Color ObjectColor { get; set; }
        private Color[,] colorsFromObjectTexture;

        private Vector3[,] normalMapVectors;
        private Vector3[,] heightMapVectors;

        public Color GetColor(int x, int y)
        {
            if (ObjectColorOption == ObjectColorOption.Constant)
            {
                return ObjectColor;
            }
            else
            {
                int rx = x < 0 ? 0 : x;
                rx %= colorsFromObjectTexture.GetLength(0);
                int ry = y < 0 ? 0 : y;
                ry %= colorsFromObjectTexture.GetLength(1);
                return colorsFromObjectTexture[rx, ry];
            }
        }

        public void ChangeObjectTexture(Bitmap objectTexture)
        {
            colorsFromObjectTexture = new Color[objectTexture.Width, objectTexture.Height];
            for (int i = 0; i < objectTexture.Width; i++)
            {
                for (int j = 0; j < objectTexture.Height; j++)
                {
                    colorsFromObjectTexture[i, j] = objectTexture.GetPixel(i, j);
                }
            }
        }

        public void ChangeNormalMap(Bitmap normalMap)
        {
            normalMapVectors = new Vector3[normalMap.Width, normalMap.Height];

            float normalizationFactor = 127;
            for (int i = 0; i < normalMap.Width; i++)
            {
                for (int j = 0; j < normalMap.Height; j++)
                {
                    Color color = normalMap.GetPixel(i, j);

                    float x = color.R / normalizationFactor;
                    float y = color.G / normalizationFactor;
                    float z = color.B / normalizationFactor;

                    if (z == 0)
                        normalMapVectors[i, j] = new Vector3(x, y, 0);
                    else
                        normalMapVectors[i, j] = new Vector3(x / z, y / z, 1);
                }
            }
        }

        public void ChangeHeightMap(Bitmap heightMap)
        {
            heightMapVectors = new Vector3[heightMap.Width, heightMap.Height];
            float factor = 0.01f;

            for (int i = 0; i < heightMap.Width; i++)
            {
                for (int j = 0; j < heightMap.Height; j++)
                {
                    Color color = heightMap.GetPixel(i, j);
                    Vector3 normalVector = GetNormalVector(i, j);

                    if (normalVector.Z == 0)
                    {
                        heightMapVectors[i, j] = new Vector3();
                    }
                    else
                    {
                        Color colorNextX = heightMap.GetPixel(i == heightMap.Width - 1 ? 0 : i, j);
                        Color colorNextY = heightMap.GetPixel(i, j == heightMap.Height - 1 ? 0 : j);
                        Vector3 t = new Vector3(1, 0, -normalVector.X);
                        Vector3 b = new Vector3(0, 1, -normalVector.Y);
                        Vector3 d = t * new Vector3(colorNextX.R, colorNextX.G, colorNextX.B) + b * new Vector3(colorNextY.R, colorNextY.G, colorNextY.B);
                        heightMapVectors[i, j] = factor * d;
                    }
                }
            }
        }

        private Vector3 GetNormalVector(int x,int y)
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
