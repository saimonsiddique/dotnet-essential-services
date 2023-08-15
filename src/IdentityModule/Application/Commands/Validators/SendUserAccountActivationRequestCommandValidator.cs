﻿using BaseModule.Infrastructure.Extensions;
using FluentValidation;
using IdentityModule.Domain.Repositories.Interfaces;

namespace IdentityModule.Application.Commands.Validators
{
    public class SendUserAccountActivationRequestCommandValidator : AbstractValidator<SendUserAccountActivationRequestCommand>
    {
        private readonly IUserRepository _userRepository;

        public SendUserAccountActivationRequestCommandValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;

            RuleFor(x => x.Email).NotNull().NotEmpty();
            RuleFor(x => x).MustAsync(BeAnExistingUser).WithMessage("User doesn't exist.").When(x => !x.Email.IsNullOrBlank());
        }

        private Task<bool> BeAnExistingUser(SendUserAccountActivationRequestCommand command, CancellationToken token)
        {
            return _userRepository.BeAnExistingUserEmail(command.Email);
        }
    }
}