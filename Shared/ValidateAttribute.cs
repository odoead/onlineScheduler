using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Shared
{
    public class ValidateAttribute<T> : Attribute, IAsyncActionFilter
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IValidator<T>> _validators;

        public ValidateAttribute(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {

            // Validate each parameter that has a validator
            foreach (var arg in context.ActionArguments)
            {
                if (arg.Value == null) continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(arg.Value.GetType());
                var validator = _serviceProvider.GetService(validatorType) as IValidator;

                if (validator != null)
                {
                    var validationContext = new ValidationContext<object>(arg.Value);
                    var validationResult = await validator.ValidateAsync(validationContext);

                    if (!validationResult.IsValid)
                    {
                        throw new ValidationException(validationResult.Errors);
                    }
                }
            }

            await next();
        }
    }
}
