using System;
using System.Collections.Generic;

namespace SPTS_Repository.Entities;

public partial class Advisor
{
    public int AdvisorId { get; set; }

    public string AdvisorCode { get; set; } = null!;

    public virtual User AdvisorNavigation { get; set; } = null!;

    public virtual ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
}
