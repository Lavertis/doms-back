﻿using DoctorsOffice.Domain.Repositories;
using DoctorsOfficeApi.CQRS.Commands.DeleteDoctorById;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Commands.DeleteDoctorById;

public class DeleteDoctorByIdHandler : IRequestHandler<DeleteDoctorByIdCommand, Unit>
{
    private readonly IDoctorRepository _doctorRepository;

    public DeleteDoctorByIdHandler(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    public async Task<Unit> Handle(DeleteDoctorByIdCommand request, CancellationToken cancellationToken)
    {
        await _doctorRepository.DeleteByIdAsync(request.DoctorId);
        return Unit.Value;
    }
}