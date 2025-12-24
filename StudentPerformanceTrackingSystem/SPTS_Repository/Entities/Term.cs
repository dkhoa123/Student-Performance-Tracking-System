using System;
using System.Collections.Generic;

namespace SPTS_Repository.Entities;

public partial class Term
{
    public int TermId { get; set; }

    public string TermName { get; set; } = null!;

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();

    public virtual ICollection<TermGpa> TermGpas { get; set; } = new List<TermGpa>();
}
