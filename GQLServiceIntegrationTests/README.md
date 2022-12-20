# Integration Tests

These integration tests take a little bit of setup... we're using zeroql which needs to generate
a stub off of a .graphql file.

## Init tooling

This uses dotnet tool to use [zeroql](https://github.com/byme8/ZeroQL)

This step is already done in this project... so you don't need to do it

`dotnet new tool-manifest `

## Add Zero QL

This step is already done... keep reading but you would need to add zeroql by doing:

`dotnet tool install ZeroQL.CLI`

## Restore

When you come to a new machine you'll need to install the tool using restore

`dotnet tool restore`

And now you should be good to go...

## Updating the generated client

`dotnet zeroql generate --schema ..\schema.graphql --namespace GQLServiceIntegrationTests.Client --client-name IntegrationTestClient --output Generated/GraphQL.g.cs`

## More Fancy

If you read the  [zeroql](https://github.com/byme8/ZeroQL)[zeroql](https://github.com/byme8/ZeroQL) you will
see that you can add this tool as a build tool which means if you do by hand editing your .csproj
file you will get an updated client every time you run the server to output a new schema.

I'd love a pull request on how to do this more fluently every compile.

```
<Target Name="GenerateQLClient" BeforeTargets="BeforeCompile">
    <Exec Command="dotnet zeroql generate --schema ..\schema.graphql --namespace GQLServiceIntegrationTests.Client --client-name IntegrationTestClient --output Generated/GraphQL.g.cs" />
</Target>
```
