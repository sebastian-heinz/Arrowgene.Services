namespace Arrowgene.Services.Middleware
{
    public delegate void MiddlewareDelegate<T, TReq, TRes>(T user, TReq request, TRes response);
}
