using System;
using System.Collections.Generic;

namespace SPTS_Repository.Entities;

public partial class SectionSchedule
{
    public int ScheduleId { get; set; }

    public int SectionId { get; set; }

    public string DayOfWeek { get; set; } = null!;

    public int StartPeriod { get; set; }

    public int EndPeriod { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public string? Room { get; set; }

    public virtual Section Section { get; set; } = null!;
}
