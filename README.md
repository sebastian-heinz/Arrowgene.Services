Arrowgene.Services
===
Arrowgene.Services aids in creating a Server with multiple Clients.

## Table of contents
- [Requirements](#requirements)
- [Building](#building)
- [Concept](#concept)
- [Project](#project)
  - [Buffers](#buffers)
  - [Logging](#logging)
  - [Network](#network)
- [Links](#links)

## Requirements
- NetStandard 2.0

## Building
```
dotnet restore
dotnet build
```

## Concept

By using this library it is recommended to utilize the provided classes,
to minimize the boilerplate code that usually is required.

If the default implementation is used, there are only two tasks required:
- Defining model classes for holding the data
- Defining handleing classes to process the model classes

The library provides the following default functionality for server and client:
- Consumable events (Client Connected, Client Disconnected, Received Data)
- Message routing (Call mapped method for 'message' -> 'handler method')
- Handle nagle algorithm

If required it is possible to supply a different implemenation instead of the default functionality for every aspect of the library.


### Pipeline




```
     {IProtocol}      {I}
          |               |
          V               V
[Client] =+=> [Protocol] =+=> [Messages] => [Protocol] => [Server]



```

### Messages

Provides methods to serialize and deserialize messages and 
calls handler methods for registered handlers.


## Project

### [Buffers](./Arrowgene.Services/Buffers)
Methods to read from a byte array.

### [Logging](./Arrowgene.Services/Logging)    
Provides logging with different log levels.

### [Network](./Arrowgene.Services/Network)    
Sever and client implementations to handle network traffic.

## Links

- NuGet (https://www.nuget.org/packages/Arrowgene.Services/)
- CLI Tools Help (https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x)
- NetCore 2.0 (https://www.microsoft.com/net/download/windows)