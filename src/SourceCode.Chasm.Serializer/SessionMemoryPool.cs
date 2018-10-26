using System.Buffers;
using System.Collections.Generic;

namespace SourceCode.Chasm.Serializer
{
    public sealed class SessionMemoryPool<T> : MemoryPool<T>
    {
        private readonly IList<IMemoryOwner<T>> _rentals;

        public override int MaxBufferSize => Shared.MaxBufferSize;

        public int Count => _rentals.Count;

        public SessionMemoryPool()
        {
            _rentals = new List<IMemoryOwner<T>>();
        }

        public override IMemoryOwner<T> Rent(int minimumBufferSize = -1)
        {
            IMemoryOwner<T> rented = Shared.Rent(minimumBufferSize);

            _rentals.Add(rented);

            return rented;
        }

        private bool _disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    IList<IMemoryOwner<T>> rentals = _rentals;

                    if (rentals != null)
                        for (int i = 0; i < rentals.Count; i++)
                            rentals[i].Dispose();
                }

                _rentals.Clear();
                _disposed = true;
            }
        }
    }
}
