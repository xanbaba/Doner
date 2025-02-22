namespace Doner;

public interface IEndpointMapper
{
    public static abstract void Map(IEndpointRouteBuilder builder);
}