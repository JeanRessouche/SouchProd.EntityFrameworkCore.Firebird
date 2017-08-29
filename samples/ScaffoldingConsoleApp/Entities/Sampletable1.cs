using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScaffoldingConsoleApp.Entities
{
    [Table("SAMPLETABLE1")]
    public partial class Sampletable1
    {
        [Column("ID")]
        public int Id { get; set; }
        [Column("INTFIELD")]
        public int? Intfield { get; set; }
        [Column("STRFIELD")]
        [StringLength(255)]
        public string Strfield { get; set; }
    }
}
