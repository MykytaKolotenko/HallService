using Hall_rent.Entity;

namespace Hall_rent.Helpers;

public interface IFavorResolver
{
    Task<List<FavorEntity>> ResolveOrThrowAsync(IEnumerable<Guid>? ids);
}
