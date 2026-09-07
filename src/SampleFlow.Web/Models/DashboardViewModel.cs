using Microsoft.AspNetCore.Mvc.Rendering;

namespace SampleFlow.Web.Models;

public sealed class DashboardViewModel
{
    public DashboardFilter Filter { get; set; } = new();
    public List<SelectListItem> CenterOptions { get; set; } = new();
    public List<SelectListItem> SampleTypeOptions { get; set; } = new();

    // KPIs
    public int TotalSamples { get; set; }
    public int TotalVisitors { get; set; }
    public int CentersEntered { get; set; }
    public int TotalActiveCenters { get; set; }
    public string? TopCenterName { get; set; }
    public int TopCenterSamples { get; set; }

    // جدول تجميعي
    public List<DashboardColumn> Columns { get; set; } = new();
    public List<DashboardRow> Rows { get; set; } = new();
    public Dictionary<int, int> ColumnTotals { get; set; } = new();

    // رسوم
    public List<DashboardBar> SamplesByCenter { get; set; } = new();
    public List<DashboardBar> TypeDistribution { get; set; } = new();
}

public sealed class DashboardFilter
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public int? CenterId { get; set; }
    public int? SampleTypeId { get; set; }
}

public sealed class DashboardColumn
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class DashboardRow
{
    public int CenterId { get; set; }
    public string CenterName { get; set; } = string.Empty;
    public bool Entered { get; set; }
    public int Visitors { get; set; }
    public Dictionary<int, int> Counts { get; set; } = new();
    public int Total { get; set; }
}

public sealed class DashboardBar
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public int Percent { get; set; }
}
