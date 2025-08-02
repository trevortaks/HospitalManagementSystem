using MediatR;

namespace HospitalManagementSystem.Application.Common.Interfaces;

public interface IRequestHandler<TRequest, TResponse> : MediatR.IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
}

public interface IRequestHandler<TRequest> : MediatR.IRequestHandler<TRequest>
    where TRequest : IRequest
{
}