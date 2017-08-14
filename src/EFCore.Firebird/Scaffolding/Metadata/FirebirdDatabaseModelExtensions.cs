using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public static class FirebirdDatabaseModelExtensions
    {
        public static FirebirdColumnModelAnnotations Firebird(/* [NotNull] */ this ColumnModel column)
            => new FirebirdColumnModelAnnotations(column);

        public static FirebirdIndexModelAnnotations Firebird(/* [NotNull] */ this IndexModel index)
            => new FirebirdIndexModelAnnotations(index);
    }
}
