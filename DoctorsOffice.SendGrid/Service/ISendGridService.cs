using DoctorsOffice.SendGrid.DTO;
using SendGrid;

namespace DoctorsOffice.SendGrid.Service;

public interface ISendGridService
{
    Task<Response> SendSingleEmailAsync(SingleEmailDto singleEmailDto);
}