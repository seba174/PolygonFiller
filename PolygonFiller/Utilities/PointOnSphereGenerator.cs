using System;
using System.Numerics;

namespace PolygonFiller
{
    public class LightSourceGenerator
    {
        private int actualAngle;
        private int actualHeight;

        public int AngleChange { get; set; }
        public int StartingHeight { get; set; }
        public int MinHeight { get; set; }
        public int MaxHeight { get; set; }
        public int HeightStepChange { get; set; }
        public int Radius { get; set; }
        public Vector2 Origin { get; set; }

        public LightSourceGenerator() => Reset();

        public Vector3 GetNextLightSourcePosition()
        {
            int x = (int)(Origin.X + Math.Cos(actualAngle * Math.PI / 180) * Radius);
            int y = (int)(Origin.Y + Math.Sin(actualAngle * Math.PI / 180) * Radius);

            actualAngle = (actualAngle + AngleChange) % 360;
            actualHeight += HeightStepChange;
            if (actualHeight > MaxHeight)
            {
                HeightStepChange = -Math.Abs(HeightStepChange);
            }
            else if (actualHeight < MinHeight)
            {
                HeightStepChange = Math.Abs(HeightStepChange);
            }

            return new Vector3(x, y, actualHeight);
        }

        public void Reset()
        {
            actualAngle = 0;
            actualHeight = StartingHeight;
        }
    }
}