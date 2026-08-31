using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Service.Interface;

namespace Hall_rent.Service;

public class FavorResolver : IFavorResolver
{
    private readonly IFavorRepository _favorRepository;

    public FavorResolver(IFavorRepository favorRepository)
    {
        _favorRepository = favorRepository;
    }

    public async Task<List<FavorEntity>> ResolveOrThrowAsync(IEnumerable<Guid>? ids)
    {
        var distinctIds = (ids ?? []).Distinct().ToList();
        if (distinctIds.Count == 0) return [];

        var favors = await _favorRepository.GetByIdsAsync(distinctIds);

        if (favors.Count != distinctIds.Count)
        {
            var missing = distinctIds.Except(favors.Select(f => f.Id));
            throw new NotFoundException($"Favors not found: {string.Join(", ", missing)}");
        }

        return favors;
    }
}