using System;

namespace Arrowgene.Services.Networking.ServerBridge.Messages
{
    [Serializable]
    public enum ResponseError
    {
        RequestTypeCastingFailed,
        NoHandler,
        NullResponse
    }
}