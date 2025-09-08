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
    public class PersonRepository : IGenericRepository<Person>
    {
        private readonly BoooooooooksDBContext _context;

        public PersonRepository(BoooooooooksDBContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Person entity)
        {
            await _context.Persons.AddAsync(entity);
        }

        public void Delete(Person entity)
        {
            _context.Remove(entity);
        }

        public async Task<IQueryable<Person>> GetAllAsync()
        {
            return _context.Persons.AsQueryable();
        }

        public async Task<Person?> GetByIdAsync(Guid id)
        {
           return await _context.Persons.Where(p => p.Gid == id).FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Update(Person entity)
        {
            _context.Persons.Update(entity);
        }
    }
}
