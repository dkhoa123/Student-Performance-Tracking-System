using SPTS_Repository.Interface.Giangvien;
using SPTS_Repository.Interface.Shared;
using SPTS_Service.Interface.Domain;
using SPTS_Service.Interface.Giangvien;
using SPTS_Service.Services.Domain;
using SPTS_Service.ViewModel.GiangvienVm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPTS_Service.Services.Giangvien
{
    public class GradeTeacherService : IGradeTeacherService
    {
        private readonly IGradeTeacherRepository _gradeRepo;
        private readonly ITermTeacherRepository _termRepo;
        private readonly IAlertSyncService _alertSyncService;           // ✅ Domain Service
        private readonly IGpaCalculationService _gpaCalcService;        // ✅ Domain Service
        private readonly ITermGpaRepository _termGpaRepo;               // ✅ New repository

        public GradeTeacherService(
            IGradeTeacherRepository gradeRepo,
            ITermTeacherRepository termRepo,
            IAlertSyncService alertSyncService,
            IGpaCalculationService gpaCalcService,
            ITermGpaRepository termGpaRepo)
        {
            _gradeRepo = gradeRepo;
            _termRepo = termRepo;
            _alertSyncService = alertSyncService;
            _gpaCalcService = gpaCalcService;
            _termGpaRepo = termGpaRepo;
        }

        public async Task SaveGradesAsync(int sectionId, List<StudentGradeRowVm> students)
        {
            var rule = await _gradeRepo.GetActiveGradeRuleBySectionAsync(sectionId);
            if (rule == null)
                throw new Exception("Môn học này chưa cấu hình tỉ trọng điểm (GradeRule).");

            var termId = await _termRepo.GetTermIdBySectionAsync(sectionId);
            var touched = new HashSet<int>();

            foreach (var student in students)
            {
                if (!student.ProcessScore.HasValue && !student.FinalScore.HasValue)
                    continue;

                // Calculate total score
                decimal sumWeight = 0m;
                decimal sum = 0m;

                if (student.ProcessScore.HasValue)
                {
                    sum += student.ProcessScore.Value * rule.ProcessWeight;
                    sumWeight += rule.ProcessWeight;
                }

                if (student.FinalScore.HasValue)
                {
                    sum += student.FinalScore.Value * rule.FinalWeight;
                    sumWeight += rule.FinalWeight;
                }

                decimal? total = null;
                if (sumWeight > 0)
                    total = Math.Round(sum / sumWeight, rule.RoundingScale);

                decimal? gpaPoint = null;
                if (total.HasValue)
                    gpaPoint = await _gradeRepo.GetGpaPointByTotalAsync(total.Value);

                // Save grade
                await _gradeRepo.UpsertGradeAsync(
                    sectionId,
                    student.StudentId,
                    student.ProcessScore,
                    student.FinalScore,
                    total,
                    gpaPoint);

                // ✅ Sync alerts using Domain Service
                await _alertSyncService.SyncAlertsForGradeAsync(
                    sectionId,
                    student.StudentId,
                    student.ProcessScore,
                    student.FinalScore,
                    total);

                touched.Add(student.StudentId);
            }

            // ✅ Recalculate Term GPA using Domain Service
            foreach (var studentId in touched)
            {
                var result = await _gpaCalcService.CalculateForTermAsync(studentId, termId);
                await _termGpaRepo.UpsertAsync(studentId, termId, result);
            }
        }
    }
}