namespace SourceCode.Chasm.Tests.TestObjects
{
    public static class TreeNodeTestObject
    {
        public static readonly TreeNode Random = new TreeNode(
            RandomHelper.String,
            TreeIdTestObject.Random);
    }
}
