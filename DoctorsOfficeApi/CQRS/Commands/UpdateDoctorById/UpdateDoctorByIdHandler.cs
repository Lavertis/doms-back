using DoctorsOfficeApi.Data;
using DoctorsOfficeApi.Models.Responses;
using DoctorsOfficeApi.Services.DoctorService;
using DoctorsOfficeApi.Services.UserService;
using MediatR;

namespace DoctorsOfficeApi.CQRS.Commands.UpdateDoctorById;

public class UpdateDoctorByIdHandler : IRequestHandler<UpdateDoctorByIdCommand, DoctorResponse>
{
    private readonly AppDbContext _context;
    private readonly IDoctorService _doctorService;
    private readonly IUserService _userService;

    public UpdateDoctorByIdHandler(AppDbContext context, IDoctorService doctorService, IUserService userService)
    {
        _context = context;
        _doctorService = doctorService;
        _userService = userService;
    }

    public async Task<DoctorResponse> Handle(UpdateDoctorByIdCommand request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(request.Id);

        if (request.UserName is not null)
        {
            doctor.AppUser.UserName = request.UserName;
            doctor.AppUser.NormalizedUserName = request.UserName.ToUpper();
        }

        if (request.Email is not null)
        {
            doctor.AppUser.Email = request.Email;
            doctor.AppUser.NormalizedEmail = request.Email.ToUpper();
        }

        doctor.AppUser.PhoneNumber = request.PhoneNumber ?? doctor.PhoneNumber;

        if (!string.IsNullOrEmpty(request.Password))
        {
            _userService.SetUserPassword(doctor.AppUser, request.Password!);
        }

        var doctorEntity = _context.Doctors.Update(doctor).Entity;
        await _context.SaveChangesAsync(cancellationToken);

        return new DoctorResponse(doctorEntity);
    }
}