Arrowgene.Services
===
Arrowgene.Services aids in creating a Server with multiple Clients.

## Table of contents
- [Requirements](#requirements)
- [Building](#building)
- [Concept](#concept)
- [Pipeline](#pipeline)
- [Project](#project)
  - [Buffers](#buffers)
  - [Logging](#logging)
  - [Messages](#messages)
  - [Network](#network)
  - [Protocols](#protocols)
- [Links](#links)

## Requirements
- NetStandard 2.0

## Building
```
dotnet restore
dotnet build
```

## Concept

The main idea of this library is to provide a set of interfaces that work well with each other.
A default implementation for each part is provided, which will allow a quick start, 
so that the focus can be on the business logic instead of writing boiler plate code.
If the core logic of a project that utilizes this library is stable, 
one can shift the focus to provide own implementations for parts of the library
that might not perform optimal for custom needs.

If the default implementation is used, there are only two tasks required:
- Defining model classes for holding the data
- Defining handleing classes to process the model classes

The library provides the following default functionality for server and client:
- Consumable events (Client Connected, Client Disconnected, Received Data)
- Message routing (Call mapped method for 'message' -> 'handler method')
- Handle nagle algorithm

If required it is possible to supply a different implemenation 
instead of the default functionality for every aspect of the library.

## Pipeline

This is an overview of the default pipeline that describes how data is transformed, send, received and recovered.


```
  (Extended Client/Server)     (Extended Message)  (Implemented Protocol)                              
             |                         |                      |           
             v                         |                      |                                                                              
   {TcpClient/TcpServer}               |                      |                   
             |                         |                      |                                              
             v                         V                      V                                                 
     [IClient/IServer] ==========> {Message} ==========> [IProtocol] => (Transport Medium) => [IProtocol] => {Message} => [IServer/IClient]

(Concrete Implementation)
{Abstract Class}
[Interface]
```

It is possible to customize each part, and remove parts. 
This allowes to pick only parts of this library that you need.
Most of the parts are interfaces or abstract classes, 
so that custom solutions can be easly plugged in and out.

## Project

### [Buffers](./Arrowgene.Services/Buffers)
Methods to read from a byte array.

### [Logging](./Arrowgene.Services/Logging)    
Provides logging with different log levels.

### [Messages](./Arrowgene.Services/Messages)    
Provides methods to serialize and deserialize messages and 
calls handler methods for registered handlers.

### [Networking](./Arrowgene.Services/Networking)    
Sever and client implementations to handle network traffic.

### [Protocols](./Arrowgene.Services/Protocols)    
Reading and writing data.

## Links

- NuGet (https://www.nuget.org/packages/Arrowgene.Services/)
- CLI Tools Help (https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x)
- NetCore 2.0 (https://www.microsoft.com/net/download/windows)