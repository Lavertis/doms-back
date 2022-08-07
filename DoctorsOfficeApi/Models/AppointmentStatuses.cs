namespace DoctorsOfficeApi.Models;

public readonly record struct AppointmentStatuses
{
    public const string Pending = "Pending";
    public const string Accepted = "Accepted";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
    public const string Completed = "Completed";

    public static readonly Dictionary<string, List<string>> AllowedTransitions = new()
    {
        {Pending, new List<string> {Accepted, Rejected,}},
        {Accepted, new List<string> {Completed, Cancelled}},
        {Rejected, new List<string>()},
        {Cancelled, new List<string>()},
        {Completed, new List<string>()},
    };
}