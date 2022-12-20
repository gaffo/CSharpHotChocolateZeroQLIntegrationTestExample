# C# GQL Integration Testing Example

All, please enjoy my sample project here. I couldn't find a good simple example on the internet
of how to do both a GQL service and a Integration Test of the same GQL Service in memory (no actual http)
with a compile time checked GQL client. So I made this.

This project is using HotChocolate as the GQL service. 
I chose ZeroQL as the query client as I couldn't find good documentation on the other C# clients
or they were not strongly typed.

Finally I'm new to the Dependency Injection (DI) space in C# in the last few years so I over documented
the DI stuff works. Hopefully these 3 items can help.

I will admit that the project is mildy over complicated with some refactoring. The reason for this
is that in my personal project, which this has been pulled from, I've been hit by the service
and the integration testing service getting out of sync with each other. 
So I tried to make the core GQL setup and wiring part consistent between the real server
and the integration testing server.

# Project Structure

Here is a quick outline of the project to help you find what you want.

## GQL Server

This project contains a simple Main method to launch the service. 
This project creates the executable you would be shipping to the world.

In here you would put anything that was involved with creating the real version of the server,
such as an entity framework, logging, etc. All of the messy wiring that goes into a real application.

## GQL Service

This project contains the core logic for your application, eg the meat of the code.
Generally I would put everything that is involved with being GQL in here. Eg stuff
that maps from GQL types to whatever your business logic is. Right now I'm not using
resolvers or any other complicated thing, for the purposes of showing the glue to setup
this project.

## GQLServiceIntegrationTests

This is where your integration tests go. 
I generally define Integration Tests as tests that are not unit tests, eg they hit
more than one object, but they generally don't need to go outside of the application
to real dependencies, those would usually be mocked. 
However the way this project is structured, the only thing that knows how the
service is wired is the setup. You could easily switch on an environment variable or other
flag to decide if you are hitting mocks or real systems. The testing logic should not care at all.

In general I like to do Test Driven Development (TDD) but services like this are pretty complicated
so I also like to write api level tests and let that drive the full implementation. 
This setup allows you to do so.

You can see a few very simple Integration Tests against my simple Query and Mutation in this folder.
