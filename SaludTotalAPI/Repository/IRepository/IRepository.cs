using System;

namespace SaludTotalAPI.Repository.IRepository;

public interface IRepository<T> where T: class
{
    Task<IEnumerable<T>> GetAll();
    Task<T?> GetById(int id);
    Task<bool> Add(T entity);
    Task<bool> Update(T entity);
    Task<bool> Delete(T entity);
}
