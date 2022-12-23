namespace DoctorsOffice.Domain.DTO.Requests;

public class CreateTimetableRequest
{
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
}

public class CreateTimetableRequestList : List<CreateTimetableRequest>
{
}