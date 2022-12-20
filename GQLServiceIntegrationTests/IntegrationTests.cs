using System.Net;
using GQLService;
using GQLServiceIntegrationTests.Client;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Person = GQLService.Person;

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

        // Now we have a service... we need to get ahold of the request executor for it, and then glue that up to a http client
        
        // First make a DI container directly
        var service = new ServiceCollection();
        // Now make a host builder context with no properties
        var context = new HostBuilderContext(new Dictionary<object, object>());
        // Now wire up the service

        GQLService.GQLService.ConfigureServices((context, services) =>
        {
            // Configure your dependency injection framework here
            // generally stuff you need to DI wire with constructors go here

            // In this integration test version we're actually going to put the configured
            // instance of the count repo in so that we can manipulate it
            service.AddSingleton<ICountRepository, InMemoryCountRepository>(s => countRepo);

            // We also need to create the personrepo, same deal, but this time
            // since there's really no state here we'll just let it be request scoped
            service.AddScoped<IPersonRepository, PersonRepsitory>();
        })(context, service);
        
        // Now we need to get to the request provider
        var provider = service.BuildServiceProvider();
        var handler = new GQLInMemoryMessageHandler(provider);

        // Create a http client to use for the tests with this handler
        var htc = new HttpClient(handler);
        htc.BaseAddress = new Uri("http://integration_test/graphql"); // we have to set a base address
        return htc;
    }

    public IntegrationTestClient Client(int count)
    {
        var htc = TestHttpClient(count);
        return new IntegrationTestClient(htc);
    }

    [Test]
    public async Task Query_Person()
    {
        var expected = new PersonRepsitory().Person;
        var client = Client(0);

        var result = await client.Query(static q => q.Person(s => new Person
        {
            FirstName = s.FirstName,
            LastName = s.LastName,
        }));
        
        Assert.NotNull(result);
        Assert.Null(result.Errors); // check errors
        Assert.NotNull(result.Data);
        
        var actual = result.Data;
        Assert.That(actual.FirstName, Is.EqualTo(expected.FirstName));
        Assert.That(actual.LastName, Is.EqualTo(expected.LastName));
    }

    [Test]
    public void Mutation_Count_Default()
    {
        var client = Client(0);
        var result = client.Mutation(m => m.Add(3)).Result;
        
        Assert.NotNull(result);
        Assert.Null(result.Errors); // check errors
        Assert.NotNull(result.Data);
        
        var actual = result.Data;
        Assert.That(actual, Is.EqualTo(3));
        
        // Ok do it again to show it's actually server scoped not request scoped
        result = client.Mutation(m => m.Add(3)).Result;
        
        Assert.NotNull(result);
        Assert.Null(result.Errors); // check errors
        Assert.NotNull(result.Data);
        
        actual = result.Data;
        Assert.That(actual, Is.EqualTo(6));
    }

    [Test]
    public void Mutation_Count_WithHigherDefault()
    {
        var client = Client(10);
        var result = client.Mutation(m => m.Add(3)).Result;
        
        Assert.NotNull(result);
        Assert.Null(result.Errors); // check errors
        Assert.NotNull(result.Data);
        
        var actual = result.Data;
        Assert.That(actual, Is.EqualTo(10 + 3));
    }
}

// This class does the actual work of sending http requests, by putting it in an HTTP client
// we keep ourselves from hitting the wire and actually just call the SendAsync function below
// this lets us in-memory glue the http request to the GQL stack directly. For great good.
class GQLInMemoryMessageHandler: HttpMessageHandler
{
    private readonly ServiceProvider _serviceProvider;

    public GQLInMemoryMessageHandler(ServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
    {
        // We need to take apart the raw http request and turn it into a GQL typed request... 
        var queryString = new StreamReader(httpRequest.Content.ReadAsStream()).ReadToEnd();
        // Parse it back into json
        var j = JObject.Parse(queryString);
        // Get the query out
        var query = j["query"].ToString();

        // Get the variables out as a dict
        var variables = new Dictionary<string, string>();
        if (j.ContainsKey("variables"))
        {
            variables = j["variables"].ToObject<Dictionary<string, string>>();
        }

        // We now have mapped the http request to in memory objects
        // which we can now use to directly invoke HotCoffee and our integration stack
        
        // first create a request builder
        var requestBuilder = QueryRequestBuilder.New().SetQuery(query);
        
        // set the varibles on the request builder from the GQL http request
        foreach (var (key, value) in variables)
        {
            requestBuilder.AddVariableValue(key, value);
        }
        
        // Give the request builder our stack context
        requestBuilder.SetServices(_serviceProvider);

        // Create an actual request
        var request = requestBuilder.Create();
        
        // Execute it
        var response = await _serviceProvider.ExecuteRequestAsync(request);
        
        // Get the body back
        var responseJson = response.ToJson();
        
        // Map it back to a http response
        return new HttpResponseMessage
        {
            Content = new StringContent(responseJson),
            StatusCode = HttpStatusCode.OK,
        };
    }
}
