namespace Utils.IRepository
{
    public interface IBaseRepository<TEntity>
    {
        public virtual Task<int>? Add(TEntity entity) { return default; }
        public virtual Task<bool>? Update(TEntity entity) { return default; }
        public virtual Task<TEntity>? GetByVar(dynamic arg) { return default; }
        public virtual Task<List<TEntity>?>? GetAll(Dictionary<string,string> args) { return default; }
        public virtual void DeleteById(int id) { }
    }
}
