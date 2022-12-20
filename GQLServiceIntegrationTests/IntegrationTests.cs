using GQLService;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GQLServiceIntegrationTests;

// Here are some simple integration tests that can hit the GQL server in memory
// which will let you do TDD on your api using a real C# client
//
// I went with everything in one file for readability
public class Tests
{
    public HttpClient TestHttpClient(int count)
    {
        // The overall goal here is to setup an integration testable version of our service stack
        // and then get a HttpClient for it which we can either use directly... or we can use
        // via the ZeroQL client for compile checked testing based on the schema.
        
        // I'm doing this as a method instead of a setup for now so I can more clearly pass a variable to the count for testing
        
        // To that first we need to make an integration testing stack... this would likely get expanded out to more complex setup code

        // we're going to use the constructor for the count repo so we can alter the starting position
        // you might do similar to set settings directly on a db context for instance... or a logger
        // but you could also just use DI and the configuration system instead to not have to make any real ctors
        // this is actually a forced example because I couldn't find a good one on the internet for
        // injecting an actual instance
        var countRepo = new InMemoryCountRepository(count);

        var testService = new GQLService.GQLService((ctx, service) =>
        {
            // Configure your dependency injection framework here
            // generally stuff you need to DI wire with constructors go here
                
            // In this integration test version we're actually going to put the configured
            // instance of the count repo in so that we can manipulate it
            service.AddSingleton<ICountRepository, InMemoryCountRepository>(s => countRepo);
                
            // We also need to create the personrepo, same deal, but this time
            // since there's really no state here we'll just let it be request scoped
            service.AddScoped<IPersonRepository, PersonRepsitory>();
        });
        
        // Now we have a service... we need to get ahold of the request executor for it, and then glue that up to a http client
    }
    
    public 

    [Test]
    public void Query_Person()
    {
        
    }

    [Test]
    public void Mutation_Count_Default()
    {
        
    }

    [Test]
    public void Mutation_Count_WithHigherDefault()
    {
        
    }
}

