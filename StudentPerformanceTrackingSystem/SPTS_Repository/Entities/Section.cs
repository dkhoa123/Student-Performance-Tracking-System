using System;
using System.Collections.Generic;

namespace SPTS_Repository.Entities;

public partial class Section
{
    public int SectionId { get; set; }

    public int TermId { get; set; }

    public int CourseId { get; set; }

    public int TeacherId { get; set; }

    public string? SectionCode { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual ICollection<SectionSchedule> SectionSchedules { get; set; } = new List<SectionSchedule>();

    public virtual ICollection<SectionStudent> SectionStudents { get; set; } = new List<SectionStudent>();

    public virtual Teacher Teacher { get; set; } = null!;

    public virtual Term Term { get; set; } = null!;
}
