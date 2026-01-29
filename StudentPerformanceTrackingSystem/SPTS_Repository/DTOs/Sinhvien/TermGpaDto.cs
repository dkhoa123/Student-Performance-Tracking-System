using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Sinhvien
{
    public record TermGpaDto(decimal? GpaValue, int? CreditsAttempted, int? CreditsEarned);
}
