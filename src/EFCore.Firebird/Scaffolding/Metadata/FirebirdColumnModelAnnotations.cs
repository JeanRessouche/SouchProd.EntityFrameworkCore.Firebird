using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using FirebirdSql.Data.FirebirdClient;


namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    public class FirebirdDatabaseColumnAnnotations
    {
        private readonly DatabaseColumn _column;

        public FirebirdDatabaseColumnAnnotations(/* [NotNull] */ DatabaseColumn column)
        {
            // Check.NotNull(column, nameof(column));

            _column = column;
        }

        public bool IsSerial
        {
            get
            {
                var value = _column[FirebirdDatabaseModelAnnotationNames.IsSerial];
                return value is bool && (bool)value;
            }
            //[param: CanBeNull]
            set { _column[FirebirdDatabaseModelAnnotationNames.IsSerial] = value; }
        }
    }
}
