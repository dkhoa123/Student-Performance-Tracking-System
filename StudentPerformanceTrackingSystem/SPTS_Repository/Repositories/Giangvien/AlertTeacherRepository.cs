using Microsoft.EntityFrameworkCore;
using SPTS_Repository.DTOs.Giangvien;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface.Giangvien;
using SPTS_Shared.Constants; //  Dùng constants
using SPTS_Shared.Helpers;   //  Dùng helpers
using SPTS_Shared.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPTS_Repository.Repositories.Giangvien
{
    public class AlertTeacherRepository : IAlertTeacherRepository
    {
        private readonly SptsContext _context;

        public AlertTeacherRepository(SptsContext context)
        {
            _context = context;
        }

        public async Task<int> GetAtRiskStudentsCountAsync(int teacherId)
        {
            return await _context.Alerts
                .Where(a => a.Section.TeacherId == teacherId &&
                            a.Status != AlertStatus.Closed &&
                            (a.Severity == Severity.High || a.Severity == Severity.Medium))
                .Select(a => a.StudentId)
                .Distinct()
                .CountAsync();
        }

        public async Task<int> GetNewAlertsCountAsync(int teacherId)
        {
            return await _context.Alerts
                .Where(a => a.Section.TeacherId == teacherId &&
                            (a.Status == AlertStatus.New || a.Status == AlertStatus.Sent))
                .CountAsync();
        }

        public async Task<List<AlertViewModelDto>> GetRecentAlertsByTeacherAsync(int teacherId, int top = 3)
        {
            return await _context.Alerts
                .Include(a => a.Student)
                .Include(a => a.Section)
                .Where(a => a.Section.TeacherId == teacherId &&
                            (a.Status == AlertStatus.New || a.Status == AlertStatus.Sent))
                .OrderByDescending(a => a.Severity)
                .ThenByDescending(a => a.CreatedAt)
                .Take(top)
                .Join(
                    _context.Users,
                    alert => alert.StudentId,
                    user => user.UserId,
                    (alert, user) => new { alert, user }
                )
                .Select(x => new AlertViewModelDto(
                    x.user.FullName,
                    x.alert.AlertType,
                    x.alert.Reason ?? AlertDisplayHelper.GetDefaultMessage(x.alert.AlertType, x.alert.ActualValue),
                    x.alert.Severity,
                    AlertDisplayHelper.GetIconName(x.alert.AlertType),
                    AlertDisplayHelper.GetIconColor(x.alert.Severity),
                    x.alert.CreatedAt
                ))
                .ToListAsync();
        }


    }
}