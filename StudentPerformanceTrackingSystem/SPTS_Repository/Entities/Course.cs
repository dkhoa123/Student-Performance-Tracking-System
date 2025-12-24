using System;
using System.Collections.Generic;

namespace SPTS_Repository.Entities;

public partial class Course
{
    public int CourseId { get; set; }

    public string CourseCode { get; set; } = null!;

    public string CourseName { get; set; } = null!;

    public int Credits { get; set; }

    public virtual ICollection<GradeRule> GradeRules { get; set; } = new List<GradeRule>();

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
}
