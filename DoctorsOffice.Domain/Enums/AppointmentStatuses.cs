namespace DoctorsOffice.Domain.Enums;

public readonly record struct AppointmentStatuses
{
    public const string Pending = nameof(Pending);
    public const string Accepted = nameof(Accepted);
    public const string Rejected = nameof(Rejected);
    public const string Cancelled = nameof(Cancelled);
    public const string Completed = nameof(Completed);

    public static readonly Dictionary<string, List<string>> AllowedTransitions = new()
    {
        {Pending, new List<string> {Accepted, Rejected,}},
        {Accepted, new List<string> {Completed, Cancelled}},
        {Rejected, new List<string>()},
        {Cancelled, new List<string>()},
        {Completed, new List<string>()},
    };
}