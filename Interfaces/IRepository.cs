namespace Interfaces

{
    public interface IRepository<T> where T : class
    {
        Task AddAsync(T entity);
        Task DeleteAsync(int id);
        Task<T?> GetByIdAsync(int id);
        Task<List<T>> GetAllAsync();
        Task<bool> IsEmpty();

        static List<T> GenerateSeedData(int numberOfItems, Func<int, T> itemGenerator)
        {
            var items = new List<T>();
            for (int i = 0; i < numberOfItems; i++)
            {
                items.Add(itemGenerator(i));
            }
            return items;
        }
    }
}
