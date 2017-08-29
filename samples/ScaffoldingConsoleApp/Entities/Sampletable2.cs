using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScaffoldingConsoleApp.Entities
{
    [Table("SAMPLETABLE2")]
    public partial class Sampletable2
    {
        [Column("ID")]
        public int Id { get; set; }
        [Column("SAMPLETABLE1ID")]
        public int Sampletable1id { get; set; }
        [Column("FIELDSMALLINT")]
        public short? Fieldsmallint { get; set; }
        [Column("FIELDMEMO", TypeName = "BLOB SUB_TYPE 1")]
        public string Fieldmemo { get; set; }
        [Column("TYPETIMESTAMP")]
        public DateTime? Typetimestamp { get; set; }
        [Column("FIELDBLOB", TypeName = "BLOB SUB_TYPE 0")]
        public byte[] Fieldblob { get; set; }
        [Column("FIELDBIGINT")]
        public long? Fieldbigint { get; set; }
        [Column("FIELDNUM", TypeName = "DECIMAL")]
        public decimal? Fieldnum { get; set; }
        [Column("FIELDDATE", TypeName = "DATE")]
        public DateTime? Fielddate { get; set; }
        [Column("FIELDTIME")]
        public TimeSpan? Fieldtime { get; set; }
        [Column("FIELDGUID", TypeName = "CHAR(36)")]
        public string Fieldguid { get; set; }
    }
}
