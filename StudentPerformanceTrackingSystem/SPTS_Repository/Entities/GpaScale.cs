using System;
using System.Collections.Generic;

namespace SPTS_Repository.Entities;

public partial class GpaScale
{
    public int ScaleId { get; set; }

    public decimal MinScore { get; set; }

    public decimal MaxScore { get; set; }

    public decimal GpaPoint { get; set; }

    public string? Letter { get; set; }
}
