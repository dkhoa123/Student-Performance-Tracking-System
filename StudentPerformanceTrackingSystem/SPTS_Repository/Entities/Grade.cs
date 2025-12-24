using System;
using System.Collections.Generic;

namespace SPTS_Repository.Entities;

public partial class Grade
{
    public int GradeId { get; set; }

    public int SectionId { get; set; }

    public int StudentId { get; set; }

    public decimal? ProcessScore { get; set; }

    public decimal? FinalScore { get; set; }

    public decimal? TotalScore { get; set; }

    public decimal? GpaPoint { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Section Section { get; set; } = null!;

    public virtual SectionStudent SectionStudent { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;

    public virtual Teacher? UpdatedByNavigation { get; set; }
}
