using BlazorTable.Addons.Handlers;
using BlazorTable.Components.ClientSide;
using BlazorTable.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorTable
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddBlazorTable(this IServiceCollection services)
        {
            services.AddSingleton(typeof(FilterKnownHandlers<>));
            services.AddSingleton(typeof(FilterHandle<>), typeof(BooleanFilterHandle<>));
            services.AddSingleton(typeof(FilterHandle<>), typeof(EnumFilterHandle<>));
            services.AddSingleton(typeof(FilterHandle<>), typeof(NumberFilterHandle<>));
            services.AddSingleton(typeof(FilterHandle<>), typeof(StringFilterHandle<>));
            services.AddSingleton(typeof(FilterHandle<>), typeof(MultiSelectHandler<>));
            services.AddSingleton(typeof(FilterHandle<>), typeof(CustomSelectHandler<>));
            
            return services.AddLocalization();
        }
    }
}