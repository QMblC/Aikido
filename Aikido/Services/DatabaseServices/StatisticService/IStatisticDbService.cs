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
        Task<int> GetMonthlyAttendances(int year, int month, StatAttendanceFilter filter);
        Task<double> GetAverageMonthlyAttendancePercent(int year, int month, StatAttendanceFilter filter);

        #endregion
    }
}
