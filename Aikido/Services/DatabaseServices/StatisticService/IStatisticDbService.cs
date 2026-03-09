using Aikido.Entities.Filters;
using DocumentFormat.OpenXml.Bibliography;

namespace Aikido.Services.DatabaseServices.StatisticService
{
    public interface IStatisticDbService
    {
        #region AttendanceStats

        Task<int> GetYearlyAttendances(int year, StatAttendanceFilter filter);
        Task<Dictionary<int, int>> GetYearlyAttendances(int firstYear, int lastYear, StatAttendanceFilter filter);
        Task<int> GetYearlyTrainings(int year, StatAttendanceFilter filter);
        Task<Dictionary<int, int>> GetYearlyTrainings(int firstYear, int lastYear, StatAttendanceFilter filter);
        Task<Dictionary<DateTime, int>> GetYearlyAttendancesByMonthes(int year, StatAttendanceFilter filter);

        #endregion

        Task<Dictionary<DateTime, int>> GetMonthlyPupilAmount(int year, StatAttendanceFilter filter);

        Task<Dictionary<DateTime, int>> GetPupilLeft(int year, StatAttendanceFilter filter);
        Task<Dictionary<DateTime, int>> GetMonthlyDanAmount(int year, StatAttendanceFilter filter);
    }
}
