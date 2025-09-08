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
    public class BookPicRepository : IGenericRepository<BookPic>
    {
        private readonly BoooooooooksDBContext _context;

        public BookPicRepository(BoooooooooksDBContext context)
        {
            _context = context;
        }

        public async Task AddAsync(BookPic entity)
        {
            await _context.Pics.AddAsync(entity);
        }

        public void Delete(BookPic entity)
        {
            _context.Pics.Remove(entity);
        }

        public async Task<IQueryable<BookPic>> GetAllAsync()
        {
            return _context.Pics.AsQueryable();
        }

        public async Task<BookPic?> GetByIdAsync(Guid id)
        {
            return await _context.Pics.Where(p => p.Gid == id).FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Update(BookPic entity)
        {
            _context.Pics.Update(entity);
        }
    }
}
