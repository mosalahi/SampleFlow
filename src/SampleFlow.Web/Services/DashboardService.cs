using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SampleFlow.Infrastructure.Data;
using SampleFlow.Web.Models;

namespace SampleFlow.Web.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> BuildAsync(
        DateOnly? from, DateOnly? to, int? centerId, int? sampleTypeId,
        CancellationToken cancellationToken = default);
}

/// <summary>يبني نموذج لوحة المشرف (KPIs + جدول تجميعي + رسوم) لفترة وفلاتر معيّنة.</summary>
public sealed class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _db;

    public DashboardService(ApplicationDbContext db) => _db = db;

    public async Task<DashboardViewModel> BuildAsync(
        DateOnly? from, DateOnly? to, int? centerId, int? sampleTypeId,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var fromDate = from ?? today;
        var toDate = to ?? today;
        if (toDate < fromDate)
        {
            (fromDate, toDate) = (toDate, fromDate);
        }

        var activeCenters = await _db.Centers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        var activeTypes = await _db.SampleTypes
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync(cancellationToken);

        var rowCenters = centerId.HasValue
            ? activeCenters.Where(c => c.Id == centerId.Value).ToList()
            : activeCenters;

        var columns = sampleTypeId.HasValue
            ? activeTypes.Where(s => s.Id == sampleTypeId.Value).ToList()
            : activeTypes;

        var entries = await _db.DailyEntries
            .Where(e => e.EntryDate >= fromDate && e.EntryDate <= toDate)
            .Where(e => !centerId.HasValue || e.CenterId == centerId.Value)
            .Include(e => e.Details)
            .ToListAsync(cancellationToken);

        var byCenter = entries
            .GroupBy(e => e.CenterId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var vm = new DashboardViewModel
        {
            Filter = new DashboardFilter
            {
                From = fromDate,
                To = toDate,
                CenterId = centerId,
                SampleTypeId = sampleTypeId,
            },
            Columns = columns
                .Select(c => new DashboardColumn { Id = c.Id, Code = c.Code, Name = c.Name })
                .ToList(),
            CenterOptions = activeCenters
                .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == centerId))
                .ToList(),
            SampleTypeOptions = activeTypes
                .Select(s => new SelectListItem(s.Name, s.Id.ToString(), s.Id == sampleTypeId))
                .ToList(),
        };

        foreach (var center in rowCenters)
        {
            var centerEntries = byCenter.TryGetValue(center.Id, out var list) ? list : new();

            var row = new DashboardRow
            {
                CenterId = center.Id,
                CenterName = center.Name,
                Entered = centerEntries.Count > 0,
                Visitors = centerEntries.Sum(e => e.Visitors),
            };

            foreach (var col in columns)
            {
                row.Counts[col.Id] = centerEntries
                    .SelectMany(e => e.Details)
                    .Where(d => d.SampleTypeId == col.Id)
                    .Sum(d => d.Count);
            }

            row.Total = row.Counts.Values.Sum();
            vm.Rows.Add(row);
        }

        foreach (var col in columns)
        {
            vm.ColumnTotals[col.Id] = vm.Rows.Sum(r => r.Counts[col.Id]);
        }

        vm.TotalSamples = vm.Rows.Sum(r => r.Total);
        vm.TotalVisitors = vm.Rows.Sum(r => r.Visitors);
        vm.CentersEntered = vm.Rows.Count(r => r.Entered);
        vm.TotalActiveCenters = rowCenters.Count;

        var top = vm.Rows.Where(r => r.Total > 0).OrderByDescending(r => r.Total).FirstOrDefault();
        vm.TopCenterName = top?.CenterName;
        vm.TopCenterSamples = top?.Total ?? 0;

        var maxCenterTotal = vm.Rows.Select(r => r.Total).DefaultIfEmpty(0).Max();
        vm.SamplesByCenter = vm.Rows
            .Where(r => r.Total > 0)
            .OrderByDescending(r => r.Total)
            .Take(6)
            .Select(r => new DashboardBar
            {
                Label = r.CenterName,
                Value = r.Total,
                Percent = maxCenterTotal > 0 ? (int)Math.Round(r.Total * 100.0 / maxCenterTotal) : 0,
            })
            .ToList();

        var typeSums = columns
            .Select(c => new { c.Name, Sum = vm.ColumnTotals[c.Id] })
            .Where(x => x.Sum > 0)
            .OrderByDescending(x => x.Sum)
            .Take(6)
            .ToList();
        var maxTypeSum = typeSums.Select(x => x.Sum).DefaultIfEmpty(0).Max();
        vm.TypeDistribution = typeSums
            .Select(x => new DashboardBar
            {
                Label = x.Name,
                Value = x.Sum,
                Percent = maxTypeSum > 0 ? (int)Math.Round(x.Sum * 100.0 / maxTypeSum) : 0,
            })
            .ToList();

        return vm;
    }
}
