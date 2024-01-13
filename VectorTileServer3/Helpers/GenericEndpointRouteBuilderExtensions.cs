
namespace VectorTileServer3
{


    using Microsoft.AspNetCore.Builder;

    // using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    // using Microsoft.Extensions.Diagnostics.HealthChecks;
    // using Microsoft.AspNetCore.Routing;
    // using Microsoft.Extensions.DependencyInjection;


    /// <summary>
    /// Provides extension methods for <see cref="IEndpointRouteBuilder"/> to add health checks.
    /// </summary>
    public static class GenericEndpointRouteBuilderExtensions
    {

        private static bool HasOptionsInConstructor(System.Type middlewareType)
        {
            // This code gets the type of the FileSystemListingMiddleware class,
            // gets the constructor info, and then checks if the parameter list
            // contains a parameter of typ

            // Get the first constructor
            foreach (System.Reflection.ConstructorInfo constructorInfo in middlewareType.GetConstructors())
            {
                bool hasOptionsParameter = System.Linq.Enumerable.Any(
                   constructorInfo.GetParameters(),
                   p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition()
                   == typeof(Microsoft.Extensions.Options.IOptions<>)
                ); 

                if (hasOptionsParameter)
                    return true;
            } // Next constructorInfo 

            return false;
        } // End Function HasOptionsInConstructor 


        private static bool HasOptionsInConstructor<T>()
        {
            return HasOptionsInConstructor(typeof(T));
        } // End Function HasOptionsInConstructor 


        /// <summary>
        /// Adds en endpointMiddleware to the <see cref="IEndpointRouteBuilder"/> with the specified pattern and options.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the health checks endpoint to.</param>
        /// <param name="pattern">The URL pattern of the health checks endpoint.</param>
        /// <param name="middlewareType">The type of the endpoint middleware.</param>
        /// <param name="options">The options used to configure the endpoint.</param>
        /// <returns>A convention routes for the endpoint middleware.</returns>
        public static IEndpointConventionBuilder MapEndpoint<TOptions>(
              this Microsoft.AspNetCore.Routing.IEndpointRouteBuilder endpoints 
            , string pattern
            , System.Type middlewareType
            , TOptions options)
            where TOptions : class
        {
            //if (ReferenceEquals(typeof(T), typeof(SQLMiddleware)))
            //{
            //    if (endpoints.ServiceProvider.GetService(typeof(ConnectionFactory)) == null)
            //    {

            //        throw new System.InvalidOperationException("Dependency-check failed. Unable to find service " +
            //            nameof(ConnectionFactory) + " in " +
            //            nameof(GenericEndpointRouteBuilderExtensions.MapEndpointCore)
            //            + ", ConfigureServices(...)"
            //        );
            //    }
            //}

            object[] args = HasOptionsInConstructor(middlewareType) ? 
                new object[]{ Microsoft.Extensions.Options.Options.Create<TOptions>(options) } : new object[0];

            Microsoft.AspNetCore.Http.RequestDelegate pipeline = endpoints.CreateApplicationBuilder()
               .UseMiddleware(middlewareType, args)
               .Build();

            return endpoints.Map(pattern, pipeline).WithDisplayName(middlewareType.AssemblyQualifiedName!);
        } // End Function MapEndpoint 


        /// <summary>
        /// AddsAdds en endpointMiddleware to the <see cref="IEndpointRouteBuilder"/> with the specified pattern.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the health checks endpoint to.</param>
        /// <param name="pattern">The URL pattern of the health checks endpoint.</param>
        /// <returns>A convention routes for the endpoint middleware.</returns>
        public static IEndpointConventionBuilder MapEndpoint<T>(
           this Microsoft.AspNetCore.Routing.IEndpointRouteBuilder endpoints,
           string pattern)
        {
            if (endpoints == null)
            {
                throw new System.ArgumentNullException(nameof(endpoints));
            }

            return MapEndpoint(endpoints, pattern, typeof(T), (object)null);
        } // End Function MapEndpoint 


        /// <summary>
        /// Adds en endpointMiddleware to the <see cref="IEndpointRouteBuilder"/> with the specified pattern and options.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the health checks endpoint to.</param>
        /// <param name="pattern">The URL pattern of the health checks endpoint.</param>
        /// <param name="options">The options used to configure the endpoint.</param>
        /// <returns>A convention routes for the endpoint middleware.</returns>
        public static IEndpointConventionBuilder MapEndpoint<T, TOptions>(
           this Microsoft.AspNetCore.Routing.IEndpointRouteBuilder endpoints,
           string pattern,
           TOptions options)
            where TOptions : class
        {
            if (endpoints == null)
            {
                throw new System.ArgumentNullException(nameof(endpoints));
            }

            if (options == null)
            {
                throw new System.ArgumentNullException(nameof(options));
            }

            return MapEndpoint(endpoints, pattern, typeof(T), options);
        } // End Function MapEndpoint 


    } // End Class GenericEndpointRouteBuilderExtensions 


} // End Namespace TestPWA 
