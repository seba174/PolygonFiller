
namespace PolygonFiller
{
    public static class TimeTicker
    {
        public static int Tick { get; set; } = 5;
        public static int GetNextTimeTickToAnimation(int current)
        {
            return (current % (int.MaxValue - Tick)) + Tick;
        }
    }
}
