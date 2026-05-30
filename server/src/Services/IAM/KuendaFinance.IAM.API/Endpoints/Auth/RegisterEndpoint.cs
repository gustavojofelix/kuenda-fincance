using FastEndpoints;
using KuendaFinance.IAM.Application.Commands.RegisterUser;
using KuendaFinance.IAM.Application.DTOs;
using MediatR;

namespace KuendaFinance.IAM.API.Endpoints.Auth;

public class RegisterEndpoint : Endpoint<RegisterUserCommand, AuthResultDto>
{
    private readonly IMediator _mediator;

    public RegisterEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/auth/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterUserCommand req, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(req, ct);
            HttpContext.Response.StatusCode = 200;
            await HttpContext.Response.WriteAsJsonAsync(result, ct);
        }
        catch (Exception ex)
        {
            AddError(ex.Message);
            ThrowIfAnyErrors();
        }
    }
}
