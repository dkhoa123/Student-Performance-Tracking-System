using System;
using System.Collections.Generic;

namespace SPTS_Repository.Entities;

public partial class GradeRule
{
    public int RuleId { get; set; }

    public int CourseId { get; set; }

    public decimal ProcessWeight { get; set; }

    public decimal FinalWeight { get; set; }

    public int RoundingScale { get; set; }

    public bool Active { get; set; }

    public virtual Course Course { get; set; } = null!;
}
