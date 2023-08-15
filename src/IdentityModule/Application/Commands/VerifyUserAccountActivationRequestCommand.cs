﻿using BaseModule.Application.DTOs.Responses;
using MediatR;

namespace IdentityModule.Application.Commands
{
    public class VerifyUserAccountActivationRequestCommand : IRequest<ServiceResponse>
    {
        public string Email { get; set; } = string.Empty;

        public string ActivationKey { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}