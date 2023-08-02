﻿using IdentityCore.Attributes;
using IdentityCore.Contracts.Declarations.Commands;
using IdentityCore.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityWeb.Controllers
{
    [ApiController]
    [AuthorizationRequired]
    [Route("api/[controller]")]
    public class CommandController : ControllerBase
    {
        #region Fields

        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Ctor

        public CommandController(IMediator mediator, IHttpContextAccessor httpContextAccessor)
        {
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Methods

        [AuthorizationNotRequired]
        [HttpPost("Authenticate")]
        public async Task<ServiceResponse> Authenticate()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext is null)
                return new ServiceResponse().BuildBadRequestResponse("Bad request");

            var command = new AuthenticateCommand()
            {
                Email = httpContext.Request.Form["Email"].ToString(),
                Password = httpContext.Request.Form["Password"].ToString(),
            };

            return await _mediator.Send(command);
        }

        [AuthorizationNotRequired]
        [HttpPost("Validate")]
        public async Task<ServiceResponse> Validate(ValidateTokenCommand command)
        {
            return await _mediator.Send(command);
        }

        [HttpPost("CreateUser")]
        public async Task<ServiceResponse> CreateUser(CreateUserCommand command)
        {
            return await _mediator.Send(command);
        }        

        [HttpPost("AddRole")]
        public async Task<ServiceResponse> AddRole(AddRoleCommand command)
        {
            return await _mediator.Send(command);
        }

        [HttpPost("AddClaimPermission")]
        public async Task<ServiceResponse> AddClaimPermission(AddClaimPermissionCommand command)
        {
            return await _mediator.Send(command);
        }

        #endregion
    }
}