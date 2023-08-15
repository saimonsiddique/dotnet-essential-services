﻿using BaseModule.Application.DTOs.Responses;
using BaseModule.Infrastructure.Extensions;
using IdentityModule.Application.Providers.Interfaces;
using LanguageModule.Application.Commands;
using LanguageModule.Application.Commands.Validators;
using LanguageModule.Domain.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace LanguageModule.Application.Commands.Handlers
{
    public class AddLingoResourcesCommandHandler : IRequestHandler<AddLingoResourcesCommand, ServiceResponse>
    {
        #region Fields

        private readonly ILogger<AddLingoResourcesCommandHandler> _logger;

        private readonly AddLingoResourcesCommandValidator _validator;

        private readonly ILanguageResourcesRepository _lingoResourcesRepository;

        private readonly IAuthenticationContextProvider _authenticationContextProvider;

        #endregion

        #region Ctor

        public AddLingoResourcesCommandHandler(
            ILogger<AddLingoResourcesCommandHandler> logger,
            AddLingoResourcesCommandValidator validator,
            ILanguageResourcesRepository lingoResourcesRepository,
            IAuthenticationContextProvider authenticationContextProvider)
        {
            _logger = logger;
            _validator = validator;
            _lingoResourcesRepository = lingoResourcesRepository;
            _authenticationContextProvider = authenticationContextProvider;
        }

        #endregion

        #region Methods

        public async Task<ServiceResponse> Handle(AddLingoResourcesCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(command, cancellationToken);
                validationResult.EnsureValidResult();

                var authCtx = _authenticationContextProvider.GetAuthenticationContext();
                var lingoResources = AddLingoResourcesCommand.Initialize(command, authCtx);

                return await _lingoResourcesRepository.AddLanguageResources(lingoResources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Response.BuildServiceResponse().BuildErrorResponse(ex.Message, _authenticationContextProvider.GetAuthenticationContext()?.RequestUri);
            }
        }

        #endregion
    }
}