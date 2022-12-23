namespace DoctorsOffice.Domain.DTO.Requests;

public class UpdateTimetableBatchRequest
{
    public Guid Id { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
}

public class UpdateTimetableBatchRequestList : List<UpdateTimetableBatchRequest>
{
}