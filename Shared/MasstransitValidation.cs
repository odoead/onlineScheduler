using FluentValidation;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Shared
{
    //https://stackoverflow.com/questions/70143738/masstransit-middleware-getting-instance-of-iserviceprovider
    public class MasstransitValidation<T> : IFilter<ConsumeContext<T>> where T : class
    {
        private readonly IValidator<T> _validator;

        public MasstransitValidation(IValidator<T> validator)
        {
            _validator = validator;
        }

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            if (_validator != null)
            {
                var validationResult = await _validator.ValidateAsync(context.Message);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }
            }

            await next.Send(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("validation");
        }

    }

    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// register validators for objects in DI. 
        /// builder.services.AddValidatorsAndFilters(typeof(T1),typeof(T2))
        /// </summary>
        /// <param name="services"></param>
        /// <param name="markerTypes">types for which validation is added </param>
        /// <returns></returns>
        public static IServiceCollection AddValidatorsAndFilters(this IServiceCollection services, params Type[] markerTypes)
        {
            foreach (var markerType in markerTypes)
            {
                var assembly = markerType.Assembly;

                // Find all validator types in the assembly
                var validatorTypes = assembly.GetTypes()
                    .Where(t => t.GetInterfaces()
                        .Any(i => i.IsGenericType &&
                             i.GetGenericTypeDefinition() == typeof(IValidator<>)));

                // Register filters for each validator
                foreach (var validatorType in validatorTypes)
                {
                    var entityType = validatorType.GetInterfaces()
                        .First(i => i.IsGenericType &&
                               i.GetGenericTypeDefinition() == typeof(IValidator<>))
                        .GetGenericArguments()[0];

                    var filterType = typeof(MasstransitValidation<>).MakeGenericType(entityType);
                    services.AddScoped(typeof(IFilter<ConsumeContext>).MakeGenericType(entityType), filterType);
                }
            }

            return services;
        }
    }

}
