using System;
using System.Collections.Generic;

namespace SPTS_Repository.Entities;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string DepartmentCode { get; set; } = null!;

    public string DepartmentName { get; set; } = null!;

    public string? Description { get; set; }

    public int? HeadTeacherId { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Teacher? HeadTeacher { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
