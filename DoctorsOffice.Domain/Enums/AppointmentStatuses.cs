namespace DoctorsOffice.Domain.Enums;

public readonly record struct AppointmentInfo(Guid Id, string Name);

public readonly record struct AppointmentStatuses
{
    public static readonly AppointmentInfo Pending = new(Guid.Parse("b7a08d2e-116d-42e3-9ec5-1aa0636d116c"), nameof(Pending).ToUpper());

    public static readonly AppointmentInfo Accepted = new(Guid.Parse("8445a2f4-97cd-45c9-921f-f649f85cc0be"), nameof(Accepted).ToUpper());

    public static readonly AppointmentInfo Rejected = new(Guid.Parse("1cf993e4-73f2-497f-ad38-bccb4b4d0eee"), nameof(Rejected).ToUpper());

    public static readonly AppointmentInfo Cancelled = new(Guid.Parse("ccbb0db5-1661-4f9b-9482-67280ebdb6b5"), nameof(Cancelled).ToUpper());

    public static readonly AppointmentInfo Completed = new(Guid.Parse("5de8a7ba-fb65-464f-9583-181d20d44b1b"), nameof(Completed).ToUpper());

    public static readonly List<AppointmentInfo> All = new() { Pending, Accepted, Rejected, Cancelled, Completed };

    public static readonly Dictionary<string, List<string>> AllowedTransitions = new()
    {
        { Pending.Name, new List<string> { Pending.Name, Accepted.Name, Rejected.Name } },
        { Accepted.Name, new List<string> { Accepted.Name, Completed.Name, Cancelled.Name } },
        { Rejected.Name, new List<string> { Rejected.Name } },
        { Cancelled.Name, new List<string> { Cancelled.Name } },
        { Completed.Name, new List<string> { Completed.Name } }
    };
}