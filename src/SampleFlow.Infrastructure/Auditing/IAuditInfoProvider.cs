namespace SampleFlow.Infrastructure.Auditing;

/// <summary>
/// يوفّر معلومات الفاعل الحالي لسجل التتبّع. تُنفَّذ في طبقة الويب اعتماداً على
/// <c>HttpContext</c> (المستخدم المسجّل + عنوان IP). عند غياب سياق طلب
/// (أدوات، مهام خلفية، وقت التصميم) تُستخدم <see cref="NullAuditInfoProvider"/>.
/// </summary>
public interface IAuditInfoProvider
{
    string? UserId { get; }
    string? UserName { get; }
    string? IpAddress { get; }
}
