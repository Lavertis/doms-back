﻿using DoctorsOffice.Domain.DTO.Responses;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.GetDoctorById;

public class GetDoctorByIdQuery : IRequest<DoctorResponse>
{
    public readonly Guid DoctorId;

    public GetDoctorByIdQuery(Guid doctorId)
    {
        DoctorId = doctorId;
    }
}