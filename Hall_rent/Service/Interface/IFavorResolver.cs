using Hall_rent.Entity;

namespace Hall_rent.Service.Interface;

public interface IFavorResolver
{
    Task<List<FavorEntity>> ResolveOrThrowAsync(IEnumerable<Guid>? ids);
}