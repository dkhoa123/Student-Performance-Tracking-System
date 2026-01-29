using SPTS_Repository.Entities;
using System.Threading.Tasks;

namespace SPTS_Service.Interface.Domain
{
    public interface INotificationDomainService
    {
        Task SendAlertNotificationAsync(int studentId, Alert alert);
        Task<int> SendToSectionAsync(int sectionId, string title, string content);
        Task SendToStudentAsync(int studentId, string title, string content);
    }
}