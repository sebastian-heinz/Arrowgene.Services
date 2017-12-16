Arrowgene.Services
===
Arrowgene.Services provides solutions for and around networking.

## Table of contents
- [Requirements](#requirements)
- [Building](#building)
- [Purpose](#purpose)
- [Pipeline](#pipeline)
- [Project](#project)
  - [Buffers](#buffers)
  - [Logging](#logging)
  - [Messages](#messages)
  - [Network](#network)
  - [Tcp/Consumer](#tcpconsumer)
- [Links](#links)

## Requirements
- NetStandard 2.0

## Building
```
dotnet restore
dotnet build
```

## Purpose

The purpose of this library is to provide transportation capabilities for data and 
automatic handling for the transferred data. The main focus is on the Tcp protocoll.
Additionally the library helps with assembly or manipulation of data.

- ITcpServer (listen, accept connections, read/send data)
- ITcpClient (connect, read/send data)
- IConsumer - Provide Consumable Events (OnClientConnected(), OnReceivedData(), OnClientDisconnected())
- IBuffer - Ease working with byte[] by providing reading/writing functions (ReadInt(), ReadSingle(), WriteInt(int value) ...etc)

These are the main Interfaces and building blocks.
A default implementation for each part is provided, which will allow a quick start, 
so that the focus can be on the business logic instead of writing boiler plate code.

## Pipeline

This is an overview of the default pipeline that describes how data is send, received and handled.
It is not necessary to use the library on both ends, 
it can be used for the server side only or as a client to connect to a remote host.
```
                                                                          @@@@@  
                                                                      @@@@@   @@@@@  
                            ┌───────────────────────────────┐      @@@@@   @@@@@  @@@@@  
                 ==|data|==>@ [IClient/IServer] ==|data|==> @══════╗@@@              @@@@@  
                            └───────────────────────────────┘   @@@║@            @@@@@  @@@@@  
                                                             @@@@  ║  Internet       @@@@@  
  ┌─────────────┐           ┌───────────────────────────────┐  @@@@║@                   @@@@@  
  │ [IConsumer] @<==|data|==@ [IClient/IServer] <==|data|== @══════╝@@  @@@@@  @@@@@  @@@@@  
  └─────────────┘           └───────────────────────────────┘     @@@@@  @@@@@   @@@@@  
                                                                      @@@@@    @@@@@  
                                                                          @@@@@  
```

## Project

### [Buffers](./Arrowgene.Services/Buffers)
Methods to read from a byte array.

### [Logging](./Arrowgene.Services/Logging)    
Provides logging with different log levels.

### [Networking](./Arrowgene.Services/Networking)    
Sever and client implementations to handle network traffic.

### [Tcp/Consumer](./Arrowgene.Services/Networking/Tcp/Consumer)    
Reading and writing data.
 
- [Messages](./Arrowgene.Services/Networking/Tcp/Consumer/Messages)    
Provides methods to serialize and deserialize messages and 
calls handler methods for registered handlers.
The server and client need to use the same assembly,
so it is recommended to create your model classes in a shared library (.dll).
Model classes need to be marked with [Serializable].

## Links

- NuGet (https://www.nuget.org/packages/Arrowgene.Services/)
- CLI Tools Help (https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x)
- NetCore 2.0 (https://www.microsoft.com/net/download/windows)