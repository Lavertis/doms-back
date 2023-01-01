namespace DoctorsOffice.Domain.DTO.Responses;

public class ChatMessageResponse
{
    public Guid Id { get; set; }

    public string Message { get; set; } = "";

    public Guid SenderId { get; set; }

    public Guid ReceiverId { get; set; }

    public DateTime CreatedAt { get; set; }
}