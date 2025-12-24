using System;
using System.Collections.Generic;

namespace SPTS_Repository.Entities;

public partial class Alert
{
    public int AlertId { get; set; }

    public int StudentId { get; set; }

    public int? TermId { get; set; }

    public int? SectionId { get; set; }

    public string AlertType { get; set; } = null!;

    public string Severity { get; set; } = null!;

    public decimal? ThresholdValue { get; set; }

    public decimal? ActualValue { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();

    public virtual Section? Section { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual Term? Term { get; set; }
}
