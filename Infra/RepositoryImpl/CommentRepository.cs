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
    public class CommentRepository : IGenericRepository<Comment>
    {
        private readonly BoooooooooksDBContext _context;

        public CommentRepository(BoooooooooksDBContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Comment entity)
        {
            await _context.Comments.AddAsync(entity);
        }

        public void Delete(Comment entity)
        {
            _context.Comments.Remove(entity);  
        }

        public async Task<IQueryable<Comment>> GetAllAsync()
        {
            return _context.Comments.AsQueryable();
        }

        public async Task<Comment?> GetByIdAsync(Guid id)
        {
            return await _context.Comments.Where(c => c.Gid == id).FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Update(Comment entity)
        {
            _context.Comments.Update(entity);
        }
    }
}
