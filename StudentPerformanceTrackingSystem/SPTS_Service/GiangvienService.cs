using SPTS_Repository;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface;
using SPTS_Service.Interface;
using SPTS_Service.ViewModel;

namespace SPTS_Service
{
    public class GiangvienService : IGiangvienService
    {
        private readonly IGiangvienRepository _repo;

        public GiangvienService(IGiangvienRepository repo)
        {
            _repo = repo;
        }

        public async Task<GiangVienProfileVm> GetProfileAsync(int teacherId)
        {
            var dto = await _repo.GetProfileAsync(teacherId);

            return new GiangVienProfileVm
            {
                TeacherId = dto.TeacherId,
                FullName = dto.TeacherName,
                Email = dto.Email,
                Status = dto.Status,
                TeacherCode = dto.TeacherCode,
                Department = dto.Department,
                Degree = dto.Degree,
                Phone = dto.Phone,
                Birthday = dto.Birthday
            };
        }


        public async Task<Giangvien> GetDashboardAsync(int teacherId, int? termId = null)
        {
            var teacherName = await _repo.GetTeacherUserAsync(teacherId);
            var terms = await _repo.GetTermsByTeacherAsync(teacherId);

            // term đang chọn: ưu tiên query param, fallback term mới nhất trong list
            var selectedTermId = termId ?? terms.FirstOrDefault().TermId;

            var totalStudents = await _repo.GetTotalStudentsByTeacherAsync(teacherId); // nếu muốn theo term thì cũng lọc term
            var averageScore = await _repo.GetAverageScoreByTeacherAsync(teacherId, selectedTermId);
            var atRiskStudents = await _repo.GetAtRiskStudentsCountAsync(teacherId);
            var newStudents = await _repo.GetNewStudentsThisMonthAsync(teacherId);
            var activeSections = await _repo.GetActiveSectionsCountAsync(teacherId);
            var newAlertsCount = await _repo.GetNewAlertsCountAsync(teacherId);

            var sections = await _repo.GetSectionsByTeacherAsync(teacherId);
            var sectionsByTerm = sections
                .GroupBy(s => new { s.TermId, s.TermName, s.StartDate })
                .OrderByDescending(g => g.Key.StartDate)
                .Select(termGroup => new TermSectionsViewModel
                {
                    TermId = termGroup.Key.TermId,
                    TermName = termGroup.Key.TermName,
                    TermStartDate = termGroup.Key.StartDate,
                    Sections = termGroup.Select(s => new SectionCardViewModel
                    {
                        SectionId = s.SectionId,
                        CourseCode = s.CourseCode,
                        CourseName = s.CourseName,
                        Room = s.Room,
                        StudentCount = s.StudentCount,
                        CompletionRate = s.CompletionRate,
                        ColorClass = s.ColorClass,
                        Credits = s.Credits,
                        Status = s.Status,
                        Schedule = s.Schedule,
                        TimeSlot = s.TimeSlot,
                        AverageScore = s.AverageScore
                    }).ToList()
                })
                .ToList();

            var alerts = await _repo.GetRecentAlertsByTeacherAsync(teacherId);
            var chart = await _repo.GetGpaChartDataByTeacherAsync(teacherId, selectedTermId); // nếu muốn chart cũng theo term

            return new Giangvien
            {
                TeacherName = teacherName,
                TotalStudents = totalStudents,
                AverageScore = averageScore,
                AtRiskStudents = atRiskStudents,
                NewStudentsThisMonth = newStudents,
                ActiveSectionsCount = activeSections,
                SectionsByTerm = sectionsByTerm,

                AvailableTerms = terms.Select(t => new TermOptionViewModel
                {
                    TermId = t.TermId,
                    TermName = t.TermName,
                    IsSelected = t.TermId == selectedTermId
                }).ToList(),
                SelectedTermId = selectedTermId,

                RecentAlerts = alerts.Select(a => new AlertViewModel
                {
                    StudentName = a.StudentName,
                    AlertType = a.AlertType,
                    Message = a.Message,
                    Severity = a.Severity,
                    IconName = a.IconName,
                    IconColor = a.IconColor,
                    CreatedAt = a.CreatedAt,
                    NewAlertsCount = newAlertsCount
                }).ToList(),

                ChartData = chart.Select(c => new ChartDataViewModel
                {
                    TermId = c.TermId,
                    TermName = c.TermName,
                    AverageGpa = c.AverageGpa
                }).ToList()
            };
        }

        public async Task<ChiTietLopVm> GetSectionDetailAsync(int sectionId, int page = 1, int pageSize = 10, string? search = null)
        {
            var dto = await _repo.GetSectionDetailAsync(sectionId);
            var alertCount = await _repo.GetAlertCountBySectionAsync(sectionId);

            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            // FILTER (search theo MSSV hoặc tên)
            var all = dto.Students.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                all = all.Where(x =>
                    (!string.IsNullOrEmpty(x.StudentCode) && x.StudentCode.Contains(s, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(x.FullName) && x.FullName.Contains(s, StringComparison.OrdinalIgnoreCase)));
            }

            var total = all.Count();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            totalPages = Math.Max(totalPages, 1);
            page = Math.Min(page, totalPages);

            var studentsPaged = all
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new ChiTietLopVm
            {
                SectionId = dto.SectionId,
                CourseCode = dto.CourseCode,
                CourseName = dto.CourseName,
                TermName = dto.TermName,
                Room = dto.Room,
                ScheduleText = dto.ScheduleText,
                SectionStatus = dto.SectionStatus,
                AlertCount = alertCount,

                Students = studentsPaged.Select(s => new StudentGradeRowVm
                {
                    StudentId = s.StudentId,
                    StudentCode = s.StudentCode,
                    FullName = s.FullName,
                    DateOfBirth = s.DateOfBirth,
                    ProcessScore = s.ProcessScore,
                    FinalScore = s.FinalScore,
                    TotalScore = s.TotalScore
                }).ToList()
            };

            // Stats header nên tính trên TOÀN BỘ lớp (dto.Students), không phải trang hiện tại
            vm.StudentCount = dto.Students.Count;

            var gradedAll = dto.Students.Where(x => x.TotalScore.HasValue).ToList();
            vm.AverageScore = gradedAll.Any()
                ? Math.Round(gradedAll.Average(x => x.TotalScore!.Value), 1)
                : (decimal?)null;

            vm.PassRatePercent = gradedAll.Any()
                ? Math.Round((decimal)gradedAll.Count(x => x.TotalScore >= 5m) * 100 / gradedAll.Count, 0)
                : 0;

            // Pagination fields (cần thêm property vào ChiTietLopVm ở bước 3)
            vm.CurrentPage = page;
            vm.TotalPages = totalPages;
            vm.PageSize = pageSize;
            vm.TotalStudents = total;
            vm.Search = search;

            return vm;
        }

        public async Task SaveGradesAsync(int sectionId, List<StudentGradeRowVm> students)
        {
            var rule = await _repo.GetActiveGradeRuleBySectionAsync(sectionId);
            if (rule == null)
                throw new Exception("Môn học này chưa cấu hình tỉ trọng điểm (GradeRule).");

            var termId = await _repo.GetTermIdBySectionAsync(sectionId);
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
                    gpaPoint = await _repo.GetGpaPointByTotalAsync(total.Value);

                await _repo.UpsertGradeAsync(sectionId, s.StudentId, s.ProcessScore, s.FinalScore, total, gpaPoint);
                await _repo.SyncAlertsForGradeAsync(sectionId, s.StudentId, s.ProcessScore, s.FinalScore, total);

                touched.Add(s.StudentId);
            }

            // ✅ Update TermGpas (A: có TotalScore là tính)
            foreach (var studentId in touched)
                await _repo.RecalculateAndUpsertTermGpaAsync(studentId, termId);
        }

        public async Task<ThongBaoPageVm> GetThongBaoPageAsync(int teacherId, int sectionId, int page = 1, int pageSize = 10)
        {
            // 1. Lấy danh sách lớp
            var sections = await _repo.GetSectionsForNotificationAsync(teacherId);

            // 2. Lấy thông tin lớp hiện tại
            var currentSection = await _repo.GetSectionDetailAsync(sectionId);

            // 3. Lấy sinh viên + alert status
            var allStudents = await _repo.GetStudentsWithAlertStatusAsync(sectionId);

            // 4. Phân trang
            var totalPages = (int)Math.Ceiling(allStudents.Count / (double)pageSize);
            var pagedStudents = allStudents
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new ThongBaoPageVm
            {
                SectionId = sectionId,
                CourseCode = currentSection.CourseCode,
                CourseName = currentSection.CourseName,
                TermName = currentSection.TermName,
                StudentCount = allStudents.Count,

                AvailableSections = sections.Select(s => new SectionOptionVm
                {
                    SectionId = s.SectionId,
                    CourseCode = s.CourseCode,
                    CourseName = s.CourseName,
                    StudentCount = s.StudentCount,
                    IsSelected = s.SectionId == sectionId
                }).ToList(),

                Students = pagedStudents.Select(s => new StudentRowVm
                {
                    StudentId = s.StudentId,
                    StudentCode = s.StudentCode,
                    FullName = s.FullName,
                    StatusLabel = GetStatusLabel(s.AlertType, s.Severity),
                    StatusBadgeClass = GetStatusBadgeClass(s.Severity),
                    StatusIcon = GetStatusIcon(s.AlertType),
                    StatusDetail = GetStatusDetail(s.AlertType, s.ActualValue),
                    StatusColorClass = GetStatusColor(s.Severity)
                }).ToList(),

                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };
        }

        // Helper methods
        private static string GetStatusLabel(string? alertType, string? severity)
        {
            if (string.IsNullOrEmpty(alertType)) return "Bình thường";

            return alertType switch
            {
                "LOW_TOTAL" or "LOW_FINAL" or "LOW_GPA" => "Điểm kém",
                "ABSENT" => "Vắng nhiều",
                "MISSING_ASSIGNMENT" => "Thiếu bài tập",
                _ => "Cảnh báo"
            };
        }

        private static string GetStatusBadgeClass(string? severity)
        {
            return severity switch
            {
                "HIGH" => "bg-red-50 dark:bg-red-900/20 text-red-600 dark:text-red-400 border-red-100 dark:border-red-900/30",
                "MEDIUM" => "bg-orange-50 dark:bg-orange-900/20 text-orange-600 dark:text-orange-400 border-orange-100 dark:border-orange-900/30",
                "LOW" => "bg-yellow-50 dark:bg-yellow-900/20 text-yellow-600 dark:text-yellow-400 border-yellow-100 dark: border-yellow-900/30",
                _ => "bg-slate-100 dark:bg-slate-800 text-slate-600 dark: text-slate-400 border-slate-200 dark:border-slate-700"
            };
        }

        private static string GetStatusIcon(string? alertType)
        {
            return alertType switch
            {
                "LOW_TOTAL" or "LOW_FINAL" or "LOW_GPA" => "trending_down",
                "ABSENT" => "event_busy",
                "MISSING_ASSIGNMENT" => "assignment_late",
                _ => "remove"
            };
        }

        private static string GetStatusDetail(string? alertType, decimal? actualValue)
        {
            if (string.IsNullOrEmpty(alertType)) return "";

            return alertType switch
            {
                "LOW_TOTAL" or "LOW_FINAL" or "LOW_GPA" => $"TB:  {actualValue:F1}/10",
                "ABSENT" => $"Vắng:  {actualValue} buổi",
                _ => ""
            };
        }

        private static string GetStatusColor(string? severity)
        {
            return severity switch
            {
                "HIGH" => "bg-red-500",
                "MEDIUM" => "bg-orange-500",
                "LOW" => "bg-yellow-500",
                _ => "bg-emerald-500"
            };
        }

        public async Task<List<SectionOptionDto>> GetSectionsForNotificationAsync(int teacherId)
        {
            return await _repo.GetSectionsForNotificationAsync(teacherId);
        }

        public async Task<List<StudentNotificationDto>> GetStudentsWithAlertStatusAsync(int sectionId)
        {
            return await _repo.GetStudentsWithAlertStatusAsync(sectionId);
        }

        public Task<int> SendToSectionAsync(int sectionId, string title, string content)
             => _repo.SendToSectionAsync(sectionId, title, content);

        public Task SendToStudentAsync(int sectionId, int studentId, string title, string content)
            => _repo.SendToStudentAsync(sectionId, studentId, title, content);

        
    }

}
