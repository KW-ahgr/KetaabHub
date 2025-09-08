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
    public class BookRepository : IGenericRepository<Book>
    {
        private readonly BoooooooooksDBContext _context;
        public BookRepository(BoooooooooksDBContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Book entity)
        {
            await _context.Books.AddAsync(entity);
        }

        public void Delete(Book entity)
        {
            _context.Books.Remove(entity);
        }

        public async Task<IQueryable<Book>> GetAllAsync()
        {
            return _context.Books
                .Include(b => b.Category)
                .Include(b => b.Comments)
                .Include(b => b.Pics)
                .AsQueryable();
        }

        public async Task<Book?> GetByIdAsync(Guid id)
        {
            return await _context.Books.Where(b => b.Gid == id).FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Update(Book entity)
        {
            _context.Books.Update(entity);
        }
    }
}
