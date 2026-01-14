using System;
using System.Collections.Generic;

namespace SPTS_Repository.Entities;

public partial class Student
{
    public int StudentId { get; set; }

    public string StudentCode { get; set; } = null!;

    public string? Major { get; set; }

    public int? CohortYear { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public int? DepartmentId { get; set; }

    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual ICollection<SectionStudent> SectionStudents { get; set; } = new List<SectionStudent>();

    public virtual User StudentNavigation { get; set; } = null!;

    public virtual ICollection<TermGpa> TermGpas { get; set; } = new List<TermGpa>();
}
