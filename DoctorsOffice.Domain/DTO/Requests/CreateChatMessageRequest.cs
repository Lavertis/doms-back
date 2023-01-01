namespace DoctorsOffice.Domain.DTO.Requests;

public class CreateChatMessageRequest
{
    public string Message { get; set; } = "";

    public Guid SenderId { get; set; }

    public Guid ReceiverId { get; set; }
}