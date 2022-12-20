using HotChocolate.Execution;
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Path = System.IO.Path;

namespace GQLService;

/*
 * This class encapsulates a service for a GQL stack, with some helper functions.
 *
 * This allows us to use the service both in a real GQL server as well as in integration tests.
 */
public class GQLService
{
    private readonly WebApplication _app;

    public GQLService(
        Action<HostBuilderContext, IServiceCollection> configureDelegate)
    {
        // Create a web application builder for aspnetcore
        var builder = WebApplication.CreateBuilder(new string[]{}) ??
                      throw new ArgumentNullException("WebApplication.CreateBuilder(args)");
        
        // Setup overall DI
        builder.Host.ConfigureServices(ConfigureServices(configureDelegate));

        // Create the web application
        _app = builder.Build();
        
        // Setup Hotchocolate features
        _app.MapGraphQL();
        _app.MapGraphQLSchema();
    }

    // This function is used to configure all of the services needed for a service stack
    // It's re-used with the integration tests and the 
    public static Action<HostBuilderContext, IServiceCollection> ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
    {
        return (ctx, services) =>
        {
            // Let our caller configure themselves for base DI
            configureDelegate(ctx, services); 
            
            // Create a hotchocolate gql server
            var gqlServer = services.AddGraphQLServer();
            
            // configure DI services for Hotchocolate here... 
            
            // Configure Services for Hot Chocolate Here,
            
            // Since there are no differences we not 
            // allowing our callers to do their own, but you could easily allow another action function in
            // to allow callers to do custom stuff... for instance if you have testing apis that
            // do not exist in production.
            
            // These are things that will get injected into your query or mutation functions
            // Note you often need to put stuff in both places.
                
            // Register count repo as a service
            gqlServer.RegisterService<ICountRepository>();
            // Register the person repo as well
            gqlServer.RegisterService<IPersonRepository>();
            
            // This is code first, so register our GQL types
            AddGQLTypes(gqlServer);
        };
    }


    // Expose starting the app
    public void Run() => _app.Run();

    // Function to write the schema to file, this is useful in multiple places
    // It'd be better to do this on build but I haven't figured out how to do that yet.
    public void OutputSchema()
    {
        var resolver = _app.Services.GetService<IRequestExecutorResolver>();
        if (resolver == null)
        {
            return;
        }

        var exeutor = resolver.GetRequestExecutorAsync().Result;
        
        // Output the schema to a string
        var schema = exeutor.Schema.ToString();

        // Find the solution directory
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !dir.GetFiles("*.sln").Any())
        {
            dir = dir.Parent;
        }
        var path = Path.Combine(dir.Parent.Parent.FullName, "schema.graphql");
        
        // Write the schema to the solution directory
        File.WriteAllText(path, schema);
    }

    // Code first register our query and mutation types
    public static void AddGQLTypes(IRequestExecutorBuilder gql)
    {
        gql.AddQueryType<Query>();
        gql.AddMutationType<Mutation>();
    }
}

public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class Query
{
    // A really simple hard coded query that just returns a hard coded value
    public Person Person([Service] IPersonRepository personRepository) => personRepository.Person;
}

// Interface for a person repo
public interface IPersonRepository
{
    Person Person { get; }
}

// A simple implementation of a person repository, this one doesn't
// need any constructor values so it can just be created
public class PersonRepsitory : IPersonRepository
{
    public Person Person
    {
        get
        {
            return new Person { FirstName = "Mike", LastName = "Gaffney" };
        }
    }
}

// A simple mutation for examples
public class Mutation
{
    // A mutation that uses a statically scoped repository
    public int Add(int count, [Service] ICountRepository countRepository)
    {
        return countRepository.Add(count);
    }
}

// Interface describing a count repository
public interface ICountRepository
{
    int Add(int count);
}

// A simple in memory repository to show a singleton with a set constructor value.
public class InMemoryCountRepository : ICountRepository
{
    private int _start;

    public InMemoryCountRepository()
    {
        _start = 0;
    }

    public InMemoryCountRepository(int start)
    {
        _start = start;
    }
    
    public int Add(int count)
    {
        _start += count;
        return _start;
    }
}