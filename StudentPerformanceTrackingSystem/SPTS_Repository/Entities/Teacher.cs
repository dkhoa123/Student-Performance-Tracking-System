using System;
using System.Collections.Generic;

namespace SPTS_Repository.Entities;

public partial class Teacher
{
    public int TeacherId { get; set; }

    public string TeacherCode { get; set; } = null!;

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();

    public virtual User TeacherNavigation { get; set; } = null!;
}
