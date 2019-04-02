namespace SourceCode.Chasm
{
#pragma warning disable CA1028 // Enum Storage should be Int32
    public enum NodeKind : byte
#pragma warning restore CA1028 // Enum Storage should be Int32
    {
        Blob = 0, // Default

        Tree = 1
    }
}
