using AuthServer.Core.Dtos;
using FluentValidation;

namespace AuthServer.API.Validations
{
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(x=> x.Email).NotEmpty().WithMessage("{PropertyName} is required").EmailAddress().WithMessage("{PropertyName} format is wrong");
            RuleFor(x => x.Password).NotEmpty().WithMessage("{PropertyName} is required");
            RuleFor(x => x.UserName).NotEmpty().WithMessage("{PropertyName} is required");
        }
    }
}
