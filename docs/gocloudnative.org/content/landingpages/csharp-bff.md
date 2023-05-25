# Build a C# BFF

To build a Back-end for Front-end with C#, you can choose three approaches:

## Build an ASPNETCORE Reverse Proxy with YARP. 
Use YARP to relay traffic to downstream API's. You can use Minimal-APIs to implement custom endpoints to invoke requests to multiple downstream APIs and aggregate the results.

## Build a GraphQL Back-end For Front-end
GraphQL allows end-users to specify which downstream resources they wish to query. You can use [Chilli Cream](https://chillicream.com/docs/bananacakepop/v2) to implement GraphQL in your aspnetcore project.

## Implement all endpoints manually
Another way to build a BFF with C#, is to simply create a new web-project, and use controllers to implement endpoints you need. The code in these controllers will invoke downstream APIs and aggregate the results.
