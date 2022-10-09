﻿using DoctorsOffice.Domain.DTO.Responses;
using DoctorsOffice.Domain.Filters;
using DoctorsOffice.Domain.Utils;
using DoctorsOffice.Domain.Wrappers;
using MediatR;

namespace DoctorsOffice.Application.CQRS.Queries.Prescriptions.GetPrescriptionsByAppointmentId;

public class GetPrescriptionsByAppointmentIdQuery : IRequest<HttpResult<PagedResponse<PrescriptionResponse>>>
{
    public Guid DoctorId { get; set; }
    public Guid AppointmentId { get; set; }
    public PaginationFilter PaginationFilter { get; set; } = null!;
}