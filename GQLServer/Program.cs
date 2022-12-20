// This class executes the GQL server from the server library

using GQLService;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    static void Main(string[] args)
    {
        var app = new GQLService.GQLService(
            (ctx, service) =>
            {
                // Configure your dependency injection framework here
                // generally stuff you need to DI wire with constructors go here
                
                // We need to make a singleton for the InMemoryCountRepoistory, in this case
                // we are just going to use the default ctor... we need to tell DI what interface and what
                // concreate class to use
                service.AddSingleton<ICountRepository, InMemoryCountRepository>();
                
                // We also need to create the personrepo, same deal, but this time
                // since there's really no state here we'll just let it be request scoped
                service.AddScoped<IPersonRepository, PersonRepsitory>();
            });
        
        // Hack to output the GQL Schema to a file for us to use in unit tests
        // This also has the benefit of making sure the schema is good, tho this would be better
        // as a build step as well
        app.OutputSchema();
        
        app.Run();
    }
}