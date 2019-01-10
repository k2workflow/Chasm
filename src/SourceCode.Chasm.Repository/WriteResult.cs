namespace SourceCode.Chasm.Repository
{
    public sealed class WriteResult<T>
    {
        public T Data { get; }

        public bool Created { get; }

        public WriteResult(T data, bool created)
        {
            Data = data;
            Created = created;
        }

        public WriteResult(bool created)
            : this(default, created)
        { }

        public void Deconstruct(out T data, out bool created)
            => (data, created) = (Data, Created);

        public T ToT() => Data;

        public static implicit operator T(WriteResult<T> result)
            => result == null ? default : result.Data;
    }
}
