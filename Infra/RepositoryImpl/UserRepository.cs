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
    public class UserRepository : IGenericRepository<User>
    {
        private readonly BoooooooooksDBContext _context;

        public UserRepository(BoooooooooksDBContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User entity)
        {
            await _context.Users.AddAsync(entity);
        }

        public void Delete(User entity)
        {
            _context.Users.Remove(entity);
        }

        public async Task<IQueryable<User>> GetAllAsync()
        {
            return _context.Users.AsQueryable();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.Where(u => u.Gid == id).FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Update(User entity)
        {
            _context.Users.Update(entity);
        }
    }
}
