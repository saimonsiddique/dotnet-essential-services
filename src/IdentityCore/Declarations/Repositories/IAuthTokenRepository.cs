﻿using IdentityCore.Declarations.Commands;
using IdentityCore.Models.Responses;

namespace IdentityCore.Declarations.Repositories
{
    public interface IAuthTokenRepository
    {
        Task<ServiceResponse> Authenticate(AuthenticateCommand command);

        Task<bool> BeAnExistingRefreshToken(string refreshToken);

        Task<ServiceResponse> ValidateToken(ValidateTokenCommand command);
    }
}