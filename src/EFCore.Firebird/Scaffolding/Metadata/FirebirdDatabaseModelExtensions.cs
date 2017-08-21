using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public static class FirebirdDatabaseModelExtensions
    {
        public static FirebirdDatabaseColumnAnnotations Firebird(/* [NotNull] */ this DatabaseColumn column)
            => new FirebirdDatabaseColumnAnnotations(column);

        public static FirebirdDatabaseIndexAnnotations Firebird(/* [NotNull] */ this DatabaseIndex index)
            => new FirebirdDatabaseIndexAnnotations(index);
    }
}
