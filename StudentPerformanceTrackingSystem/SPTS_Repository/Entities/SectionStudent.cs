using System;
using System.Collections.Generic;

namespace SPTS_Repository.Entities;

public partial class SectionStudent
{
    public int SectionId { get; set; }

    public int StudentId { get; set; }

    public int? AddedBy { get; set; }

    public DateTime AddedAt { get; set; }

    public string Source { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual User? AddedByNavigation { get; set; }

    public virtual Grade? Grade { get; set; }

    public virtual Section Section { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
