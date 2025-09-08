using Domin;
using Infra.Persist;

using static Api.Security.Hsh;

namespace Api.Security
{
    public interface IUserService
    {
        void DeleteUser(string id);
        User CreateUser(string surename, string username, string password, string role);
        User FindUserById(string id);
        User FindUserByArgs(string username, string password);
    }

    public class UserService : IUserService
    {

        private readonly BoooooooooksDBContext _context;

        public UserService(BoooooooooksDBContext context)
        {
            _context = context;
        }

        public User CreateUser(string surename, string username, string password, string role)
        {
            (string Hash, string Salt) saltpass = HashPassword(password);
            User user = new()
            {
                SureName = surename,
                UserName = username,
                HashPassword = saltpass.Hash,
                Slt = saltpass.Salt,
                Role = role
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }
        public void DeleteUser(string id)
        {
            _context.Users.Remove(FindUserById(id));
        }
        public User FindUserById(string id)
        {
            var g = Guid.Parse(id);
            return _context.Users.Where(u => u.Gid == g).First();
        }

        public User FindUserByArgs(string username, string password) =>
                _context.Users.Where(u => u.UserName == username)
                    .AsEnumerable()
                    .Where(u => VerifyPassword(password, u.HashPassword, u.Slt))
                    .First();

    }
}
