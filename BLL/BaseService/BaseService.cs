using DAL.BaseRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BLL.BaseService
{
    /// <summary>
    /// 服务基类
    /// </summary>
    public abstract class BaseService<T> : IBaseService<T> where T : class
    {
        protected IBaseRepository<T> CurrentRepository { get; set; }

        public BaseService(IBaseRepository<T> currentRepository)
        {
            CurrentRepository = currentRepository;
        }

        public T Add(T entity)
        {
            return CurrentRepository.Add(entity);
        }

        public bool Update(T entity)
        {
            return CurrentRepository.Update(entity);
        }

        public bool Delete(T entity)
        {
            return CurrentRepository.Delete(entity);
        }

        public int Count(Expression<Func<T, bool>> predicate)
        {
            return CurrentRepository.Count(predicate);
        }

        public bool Exist(Expression<Func<T, bool>> anyLambda)
        {
            return CurrentRepository.Exist(anyLambda);
        }

        public T Find(Expression<Func<T, bool>> whereLambda)
        {
            return CurrentRepository.Find(whereLambda);
        }

        public IQueryable<T> FindList(Expression<Func<T, bool>> whereLamdba, string orderName, bool isAsc)
        {
            return CurrentRepository.FindList(whereLamdba, orderName, isAsc);
        }

        public IQueryable<T> FindPageList(int pageIndex, int pageSize, out int totalRecord, Expression<Func<T, bool>> whereLamdba, string orderName, bool isAsc)
        {
            return FindPageList(pageIndex, pageSize, out totalRecord, whereLamdba, orderName, isAsc);
        }
    }
}
