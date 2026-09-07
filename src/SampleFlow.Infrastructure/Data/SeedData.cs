using Microsoft.EntityFrameworkCore;
using SampleFlow.Domain.Authorization;
using SampleFlow.Domain.Entities;
using SampleFlow.Infrastructure.Identity;

namespace SampleFlow.Infrastructure.Data;

/// <summary>
/// بذر البيانات المرجعية الثابتة عبر <c>HasData</c> فتُدرَج ضمن الـ Migration:
/// المراكز، أنواع التحاليل، الصلاحيات، الأدوار، وربط الأدوار بالصلاحيات.
/// (مستخدم الأدمن يُبذَر وقت التشغيل عبر <see cref="Seeding.IdentityDataSeeder"/>
/// لأن تجزئة كلمة المرور تحتاج UserManager.)
/// </summary>
public static class SeedData
{
    // طابع زمني ثابت للبذر — يجب أن يكون حرفياً ثابتاً حتى لا تتغير الـ Migrations.
    private static readonly DateTime SeedTimestamp =
        new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // معرّفات أدوار ثابتة (GUID كنص) لثبات ربط RolePermissions.
    public const string CenterRoleId = "b1a7c0de-0000-0000-0000-0000000000c1";
    public const string SupervisorRoleId = "b1a7c0de-0000-0000-0000-0000000000c2";
    public const string AdminRoleId = "b1a7c0de-0000-0000-0000-0000000000c3";

    public static void Apply(ModelBuilder builder)
    {
        SeedCenters(builder);
        SeedSampleTypes(builder);
        SeedPermissions(builder);
        SeedRoles(builder);
        SeedRolePermissions(builder);
    }

    private static void SeedCenters(ModelBuilder builder)
    {
        string[] names =
        {
            "الأشرفية", "الصالحية", "القادسية", "المطار",
            "السليمانية", "الزهرة", "الروضة", "لولوة النعيم",
            "غرب عنيزة", "الروابي", "الروغاني", "جنوب غرب عنيزة",
            "وسط عنيزة", "الصناعية", "الملك خالد", "جنوب الملك خالد",
        };

        var centers = new List<Center>(names.Length);
        for (var i = 0; i < names.Length; i++)
        {
            centers.Add(new Center
            {
                Id = i + 1,
                Name = names[i],
                IsActive = true,
                CreatedAt = SeedTimestamp,
            });
        }

        builder.Entity<Center>().HasData(centers);
    }

    private static void SeedSampleTypes(ModelBuilder builder)
    {
        // (Code == Name مبدئياً؛ الاسم المعروض قابل للتعديل من لوحة الأدمن.)
        string[] codes =
        {
            "CBC", "Chemistry", "HBA1C", "Hormone", "Stool",
            "FIT", "Urine", "ABO", "Serology",
        };

        var types = new List<SampleType>(codes.Length);
        for (var i = 0; i < codes.Length; i++)
        {
            types.Add(new SampleType
            {
                Id = i + 1,
                Code = codes[i],
                Name = codes[i],
                DisplayOrder = i + 1,
                IsActive = true,
            });
        }

        builder.Entity<SampleType>().HasData(types);
    }

    private static void SeedPermissions(ModelBuilder builder)
    {
        const string entries = "Entries";
        const string reports = "Reports";
        const string admin = "Administration";

        builder.Entity<Permission>().HasData(
            new Permission { Id = 1, Key = PermissionKeys.EntriesCreate, Name = "إضافة إدخال يومي", Category = entries },
            new Permission { Id = 2, Key = PermissionKeys.EntriesEditOwn, Name = "تعديل إدخال المركز", Category = entries },
            new Permission { Id = 3, Key = PermissionKeys.EntriesEditAny, Name = "تعديل أي إدخال", Category = entries },
            new Permission { Id = 4, Key = PermissionKeys.EntriesViewOwn, Name = "عرض إدخالات المركز", Category = entries },
            new Permission { Id = 5, Key = PermissionKeys.EntriesViewAny, Name = "عرض جميع الإدخالات", Category = entries },
            new Permission { Id = 6, Key = PermissionKeys.ReportsView, Name = "عرض التقارير", Category = reports },
            new Permission { Id = 7, Key = PermissionKeys.ReportsExport, Name = "تصدير التقارير", Category = reports },
            new Permission { Id = 8, Key = PermissionKeys.CentersManage, Name = "إدارة المراكز", Category = admin },
            new Permission { Id = 9, Key = PermissionKeys.SampleTypesManage, Name = "إدارة أنواع التحاليل", Category = admin },
            new Permission { Id = 10, Key = PermissionKeys.UsersManage, Name = "إدارة المستخدمين والصلاحيات", Category = admin },
            new Permission { Id = 11, Key = PermissionKeys.AuditView, Name = "عرض سجل التتبّع", Category = admin });
    }

    private static void SeedRoles(ModelBuilder builder)
    {
        builder.Entity<ApplicationRole>().HasData(
            new ApplicationRole
            {
                Id = CenterRoleId,
                Name = Roles.Center,
                NormalizedName = Roles.Center.ToUpperInvariant(),
                ConcurrencyStamp = CenterRoleId,
            },
            new ApplicationRole
            {
                Id = SupervisorRoleId,
                Name = Roles.Supervisor,
                NormalizedName = Roles.Supervisor.ToUpperInvariant(),
                ConcurrencyStamp = SupervisorRoleId,
            },
            new ApplicationRole
            {
                Id = AdminRoleId,
                Name = Roles.Admin,
                NormalizedName = Roles.Admin.ToUpperInvariant(),
                ConcurrencyStamp = AdminRoleId,
            });
    }

    private static void SeedRolePermissions(ModelBuilder builder)
    {
        // المركز: إضافة + عرض إدخالاته فقط (بدون تعديل).
        int[] centerPermissions = { 1, 4 };

        // المشرف: عرض/تعديل كل الإدخالات + التقارير والتصدير.
        int[] supervisorPermissions = { 3, 5, 6, 7 };

        // الأدمن: كل الصلاحيات.
        int[] adminPermissions = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

        var rows = new List<RolePermission>();
        foreach (var p in centerPermissions)
            rows.Add(new RolePermission { RoleId = CenterRoleId, PermissionId = p });
        foreach (var p in supervisorPermissions)
            rows.Add(new RolePermission { RoleId = SupervisorRoleId, PermissionId = p });
        foreach (var p in adminPermissions)
            rows.Add(new RolePermission { RoleId = AdminRoleId, PermissionId = p });

        builder.Entity<RolePermission>().HasData(rows);
    }
}
