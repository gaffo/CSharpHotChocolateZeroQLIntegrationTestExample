// This file generated for ZeroQL.
// <auto-generated/>
// <checksum>cd7dfeebe439d836a7fec06d46de27a1</checksum>
#pragma warning disable 8618
#nullable enable
namespace GQLServiceIntegrationTests.Client
{
    using System;
    using System.Linq;
    using System.Text.Json.Serialization;
    using ZeroQL;

    public class IntegrationTestClient : global::ZeroQL.GraphQLClient<Query, Mutation>
    {
        public IntegrationTestClient(global::System.Net.Http.HttpClient client, global::ZeroQL.Pipelines.IGraphQLQueryPipeline? queryPipeline = null) : base(client, queryPipeline)
        {
        }
    }

    [System.CodeDom.Compiler.GeneratedCode ( "ZeroQL" ,  "3.1.0.0" )]
    public class Mutation : global::ZeroQL.Internal.IMutation
    {
        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never), JsonPropertyName("Add")]
        public int __Add { get; set; }

        [ZeroQL.GraphQLFieldSelector]
        public int Add(int count)
        {
            return __Add;
        }
    }

    [System.CodeDom.Compiler.GeneratedCode ( "ZeroQL" ,  "3.1.0.0" )]
    public class Person
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    [System.CodeDom.Compiler.GeneratedCode ( "ZeroQL" ,  "3.1.0.0" )]
    public class Query : global::ZeroQL.Internal.IQuery
    {
        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never), JsonPropertyName("Person")]
        public Person __Person { get; set; }

        [ZeroQL.GraphQLFieldSelector]
        public T Person<T>(Func<Person, T> selector)
        {
            return selector(__Person);
        }
    }

    public static class JsonConvertersInitializers
    {
        [global::System.Runtime.CompilerServices.ModuleInitializer]
        public static void Init()
        {
        }
    }
}
#pragma warning restore 8618