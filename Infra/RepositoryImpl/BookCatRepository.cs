using Dom.Repo;
using Domin;
using Infra.Persist;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.RepositoryImpl
{
    public class BookCatRepository : IGenericRepository<BookCat>
    {
        private readonly BoooooooooksDBContext _context;

        public BookCatRepository(BoooooooooksDBContext context)
        {
            _context = context;
        }

        public async Task AddAsync(BookCat entity)
        {
            await _context.AddAsync(entity);
        }

        public void Delete(BookCat entity)
        {
            _context.Remove(entity);
        }

        public async Task<IQueryable<BookCat>> GetAllAsync()
        {
            return _context.Categories.AsQueryable();
        }

        public async Task<BookCat?> GetByIdAsync(Guid id)
        {
            return await _context.Categories.Where(c => c.Gid == id).FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Update(BookCat entity)
        {
            _context.Categories.Update(entity);
        }
    }
}
