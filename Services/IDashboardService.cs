using ALOud.DTOs.Admin;

namespace ALOud.Services;

// Dashboard service: aggregates statistics and KPIs for admin dashboard.
public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}
