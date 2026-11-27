using System;

namespace FlameBlame.Audio.Simulation
{
    public class ShuffleBag
    {
        private readonly int[] _order;
        private int _next;

        public int Count => _order.Length;

        public ShuffleBag(int count)
        {
            _order = new int[count];
            for (var index = 0; index < count; index++)
                _order[index] = index;

            _next = count;
        }

        public int Draw(Random random)
        {
            if (_next == _order.Length)
                Refill(random);

            return _order[_next++];
        }

        private void Refill(Random random)
        {
            var previous = _order[_order.Length - 1];
            for (var index = _order.Length - 1; index > 0; index--)
                Swap(index, random.Next(index + 1));

            if (_order.Length > 1 && _order[0] == previous)
                Swap(0, random.Next(1, _order.Length));

            _next = 0;
        }

        private void Swap(int first, int second)
        {
            var held = _order[first];
            _order[first] = _order[second];
            _order[second] = held;
        }
    }
}
