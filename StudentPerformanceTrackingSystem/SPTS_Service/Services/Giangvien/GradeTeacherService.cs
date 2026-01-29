using SPTS_Repository.Interface.Giangvien;
using SPTS_Service.Interface.Giangvien;
using SPTS_Service.ViewModel.GiangvienVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.Services.Giangvien
{
    public class GradeTeacherService : IGradeTeacherService
    {
        private readonly IGradeTeacherRepository _gradeRepo;
        private readonly ITermTeacherRepository _termRepo;
        private readonly IAlertTeacherRepository _alertRepo;

        public GradeTeacherService(
            IGradeTeacherRepository gradeRepo,
            ITermTeacherRepository termRepo,
            IAlertTeacherRepository alertRepo)
        {
            _gradeRepo = gradeRepo;
            _termRepo = termRepo;
            _alertRepo = alertRepo;
        }
        public async Task SaveGradesAsync(int sectionId, List<StudentGradeRowVm> students)
        {
            var rule = await _gradeRepo.GetActiveGradeRuleBySectionAsync(sectionId);
            if (rule == null)
                throw new Exception("Môn học này chưa cấu hình tỉ trọng điểm (GradeRule).");

            var termId = await _termRepo.GetTermIdBySectionAsync(sectionId);
            var touched = new HashSet<int>();

            foreach (var s in students)
            {
                var hasP = s.ProcessScore.HasValue;
                var hasF = s.FinalScore.HasValue;

                if (!hasP && !hasF) continue;

                decimal sumWeight = 0m;
                decimal sum = 0m;

                if (hasP) { sum += s.ProcessScore!.Value * rule.ProcessWeight; sumWeight += rule.ProcessWeight; }
                if (hasF) { sum += s.FinalScore!.Value * rule.FinalWeight; sumWeight += rule.FinalWeight; }

                decimal? total = null;
                if (sumWeight > 0)
                    total = Math.Round(sum / sumWeight, rule.RoundingScale);

                decimal? gpaPoint = null;
                if (total.HasValue)
                    gpaPoint = await _gradeRepo.GetGpaPointByTotalAsync(total.Value);

                await _gradeRepo.UpsertGradeAsync(sectionId, s.StudentId, s.ProcessScore, s.FinalScore, total, gpaPoint);
                await _alertRepo.SyncAlertsForGradeAsync(sectionId, s.StudentId, s.ProcessScore, s.FinalScore, total);

                touched.Add(s.StudentId);
            }

            // ✅ Update TermGpas (A: có TotalScore là tính)
            foreach (var studentId in touched)
                await _gradeRepo.RecalculateAndUpsertTermGpaAsync(studentId, termId);
        }
    }
}
