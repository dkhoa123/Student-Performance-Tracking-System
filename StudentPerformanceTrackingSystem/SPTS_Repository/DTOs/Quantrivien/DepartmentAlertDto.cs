using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Repository.DTOs.Quantrivien
{
    public class DepartmentAlertDto
    {
        public string DepartmentName { get; set; } = "";
        public string DepartmentCode { get; set; } = "";
        public int TotalStudents { get; set; }      // Tổng SV của khoa
        public int AlertCount { get; set; }         // Số SV bị cảnh báo (distinct)
        public decimal AlertRate { get; set; }      // Tỷ lệ % SV bị CB
    }
}
