using Hall_rent.Entity;

namespace Hall_rent.Repository.Interfaces;

public interface IFavorRepository
{
    Task AddAsync(FavorEntity favor);
    Task<FavorEntity?> GetByIdAsync(Guid id);
    Task<List<FavorEntity>> GetAllAsync();
    Task<List<FavorEntity>> GetByIdsAsync(List<Guid> ids);
    void Remove(FavorEntity favor);
}