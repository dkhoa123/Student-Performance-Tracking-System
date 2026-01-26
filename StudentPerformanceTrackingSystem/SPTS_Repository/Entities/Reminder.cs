using System;
using System.Collections.Generic;

namespace SPTS_Repository.Entities;

public partial class Reminder
{
    public int ReminderId { get; set; }

    public int AlertId { get; set; }

    public int AdvisorId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public virtual Alert Alert { get; set; } = null!;
}
