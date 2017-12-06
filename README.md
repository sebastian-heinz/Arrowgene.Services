Arrowgene.Services
===
Arrowgene.Services provides interfaces and implementations for data transfer and handling.

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

In case of a TCP Server:

If the default implementation is used, there are two tasks required:
- Defining model classes for holding the data
- Defining handleing classes to process the model classes

The library provides the following default functionality for server and client:
- Consumable events (Client Connected, Client Disconnected, Received Data)
- Message routing ('message' -> 'handler method')
- Handle nagle algorithm

If required it is possible to supply a different implemenation
for every aspect of the library, instead of the default functionality.

## Pipeline

This is an overview of the default pipeline that describes how data is transformed, send, received and recovered.

Minimal Tcp/Client server pipeline where the library only handles the transportation.
It is not necessary to use the library on both ends, 
it can be used for the server side only or as a client to connect to a remote host.
```
                                                               @@@@@  
                                                           @@@@@   @@@@@  
                ┌───────────────────────────────┐     @@@@@   @@@@@  @@@@@  
     ==|data|==>@ [IClient/IServer] ==|data|==> @══════╗@@@              @@@@@  
                └───────────────────────────────┘   @@@║@            @@@@@  @@@@@  
                                                 @@@@  ║  Internet       @@@@@  
                ┌───────────────────────────────┐  @@@@║@                   @@@@@  
     <==|data|==@ [IClient/IServer] <==|data|== @══════╝@@  @@@@@  @@@@@  @@@@@  
                └───────────────────────────────┘     @@@@@  @@@@@   @@@@@  
                                                          @@@@@    @@@@@  
```

Define a protocol to handle the serialization:
```
                                                                                     @@@@@  
                                                                                 @@@@@   @@@@@  
                ┌───────────────────────────────────────────────────────┐     @@@@@   @@@@@  @@@@@  
   ==|object|==>@ [IProtocol] ==|Data|==> [IClient/IServer] ==|Data|==> @══════╗@@@              @@@@@  
                └───────────────────────────────────────────────────────┘   @@@║@            @@@@@  @@@@@  
                                                                         @@@@  ║  Internet       @@@@@  
                ┌───────────────────────────────────────────────────────┐  @@@@║@                   @@@@@  
   <==|object|==@ [IProtocol] <==|Data|== [IClient/IServer] <==|Data|== @══════╝@@  @@@@@  @@@@@  @@@@@  
                └───────────────────────────────────────────────────────┘     @@@@@  @@@@@   @@@@@  
                                                                                @@@@@    @@@@@  
```

Use the provided message protocol that allows to define message handler:
```
                                                                                                               @@@@@  
                                                                                                           @@@@@   @@@@@  
                                    ┌─────────────────────────────────────────────────────────────┐     @@@@@   @@@@@  @@@@@  
                      ==|Message|==>@ [MessageProtocol] ==|Data|==> [IClient/IServer] ==|Data|==> @══════╗@@@              @@@@@  
                                    └─────────────────────────────────────────────────────────────┘   @@@║@            @@@@@  @@@@@  
                                                                                                   @@@@  ║  Internet       @@@@@  
    ┌─────────────────────────────────────────────────────────────────────────────────────────────┐  @@@@║@                   @@@@@  
    │ [MessageHandler] <==|Message|== [MessageProtocol] <==|Data|== [IClient/IServer] <==|Data|== @══════╝@@  @@@@@  @@@@@  @@@@@  
    └─────────────────────────────────────────────────────────────────────────────────────────────┘     @@@@@  @@@@@   @@@@@  
                                                                                                           @@@@@    @@@@@  
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

### [Networking](./Arrowgene.Services/Networking)    
Sever and client implementations to handle network traffic.

### [Protocols](./Arrowgene.Services/Protocols)    
Reading and writing data.
 
- [Messages](./Arrowgene.Services/Protocols/Messages)    
Provides methods to serialize and deserialize messages and 
calls handler methods for registered handlers.
The server and client need to use the same assembly,
so it is recommended to create your model classes in a shared library (.dll).
Model classes need to be marked with [Serializable].

## Links

- NuGet (https://www.nuget.org/packages/Arrowgene.Services/)
- CLI Tools Help (https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x)
- NetCore 2.0 (https://www.microsoft.com/net/download/windows)