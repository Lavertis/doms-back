using System.ComponentModel.DataAnnotations.Schema;
using DoctorsOffice.Domain.Entities.UserTypes;

namespace DoctorsOffice.Domain.Entities;

[Table("ChatMessages")]
public class ChatMessage : BaseEntity
{
    public string Message { get; set; } = "";

    public Guid SenderId { get; set; }
    public AppUser Sender { get; set; } = null!;

    public Guid ReceiverId { get; set; }
    public AppUser Receiver { get; set; } = null!;
}