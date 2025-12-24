using System;
using System.Collections.Generic;

namespace SPTS_Repository.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Advisor? Advisor { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<SectionStudent> SectionStudents { get; set; } = new List<SectionStudent>();

    public virtual Student? Student { get; set; }

    public virtual Teacher? Teacher { get; set; }
}
