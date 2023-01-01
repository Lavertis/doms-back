using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace DoctorsOffice.Domain.Entities.UserTypes;

public class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public List<RefreshToken> RefreshTokens { get; set; } = new();

    [InverseProperty(nameof(ChatMessage.Sender))]
    public List<ChatMessage> SendMessages { get; set; } = new();

    [InverseProperty(nameof(ChatMessage.Receiver))]
    public List<ChatMessage> ReceivedMessages { get; set; } = new();
}