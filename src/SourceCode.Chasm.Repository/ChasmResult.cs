namespace SourceCode.Chasm.Repository
{
    public sealed class ChasmResult<T> //: ChasmResult
    {
        public T Data { get; }

        public bool Idempotent { get; }

        public ChasmResult(T data, bool idempotent)
        {
            Data = data;
            Idempotent = idempotent;
        }

        public ChasmResult(bool idempotent)
            : this(default, idempotent)
        { }

        public T ToT() => Data;

        public static implicit operator T(ChasmResult<T> result)
            => result == null ? default : result.Data;
    }

    //public class ChasmResult
    //{
    //    public bool Idempotent { get; }

    //    public ChasmResult(bool idempotent)
    //    {
    //        Idempotent = idempotent;
    //    }
    //}
}
