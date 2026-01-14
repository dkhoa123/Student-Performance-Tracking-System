using System;
using System.Collections.Generic;

namespace SPTS_Repository.Entities;

public partial class AcademicYear
{
    public int AcademicYearId { get; set; }

    public string YearName { get; set; } = null!;

    public int StartYear { get; set; }

    public int EndYear { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public bool IsCurrent { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Term> Terms { get; set; } = new List<Term>();
}
