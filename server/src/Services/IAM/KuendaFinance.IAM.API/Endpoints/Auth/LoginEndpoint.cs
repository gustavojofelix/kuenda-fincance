using FastEndpoints;
using KuendaFinance.IAM.Application.Commands.Login;
using KuendaFinance.IAM.Application.DTOs;
using MediatR;

namespace KuendaFinance.IAM.API.Endpoints.Auth;

public class LoginEndpoint : Endpoint<LoginCommand, AuthResultDto>
{
    private readonly IMediator _mediator;

    public LoginEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginCommand req, CancellationToken ct)
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
