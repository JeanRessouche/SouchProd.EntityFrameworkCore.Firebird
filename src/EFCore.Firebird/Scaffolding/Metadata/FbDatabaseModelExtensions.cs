using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public static class FbDatabaseModelExtensions
    {
        public static FbDatabaseColumnAnnotations Firebird(/* [NotNull] */ this DatabaseColumn column)
            => new FbDatabaseColumnAnnotations(column);

        public static FbDatabaseIndexAnnotations Firebird(/* [NotNull] */ this DatabaseIndex index)
            => new FbDatabaseIndexAnnotations(index);
    }
}
