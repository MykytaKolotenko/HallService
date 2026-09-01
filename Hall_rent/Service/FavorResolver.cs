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

    // Resolves the list of service IDs in the entity and guarantees that ALL provided IDs exist.
    // It is enough to compare the number of found rows with the number of requested unique IDs;
    // the missing ID(s) are determined with Except only in the exception branch, which is rarely executed.
    // Used both when creating/updating a hall (which services it offers) and when booking
    // (which services the client selected).
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