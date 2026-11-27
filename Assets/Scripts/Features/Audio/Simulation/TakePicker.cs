using System;

namespace FlameBlame.Audio.Simulation
{
    public static class TakePicker
    {
        public const int NoTake = -1;

        public static int Pick(int count, int lastTake, Random random)
        {
            if (count == 1)
                return 0;

            if (lastTake == NoTake)
                return random.Next(count);

            var take = random.Next(count - 1);
            return take >= lastTake ? take + 1 : take;
        }
    }
}
