using System;
using System.Collections.Generic;

namespace SPTS_Repository.Entities;

public partial class TermGpa
{
    public int TermId { get; set; }

    public int StudentId { get; set; }

    public decimal? GpaValue { get; set; }

    public int? CreditsAttempted { get; set; }

    public int? CreditsEarned { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual Term Term { get; set; } = null!;
}
