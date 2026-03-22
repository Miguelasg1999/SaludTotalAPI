using System;
using Microsoft.EntityFrameworkCore;
using SaludTotalAPI.Data;
using SaludTotalAPI.Repository.IRepository;

namespace SaludTotalAPI.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _db;
    private readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext db)
    {
        _db = db;
        _dbSet = _db.Set<T>();
    }

    public async Task<bool> Add(T entity)
    {
        await _dbSet.AddAsync(entity);
        return await _db.SaveChangesAsync() > 0 ? true : false;
    }

    public async Task<bool> Delete(T entity)
    {
        _dbSet.Remove(entity);
        return await _db.SaveChangesAsync() > 0 ? true : false;
    }

    public async Task<IEnumerable<T>> GetAll()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public async Task<T?> GetById(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<bool> Update(T entity)
    {
        _dbSet.Update(entity);
        return await _db.SaveChangesAsync() > 0 ? true : false;
    }

}
