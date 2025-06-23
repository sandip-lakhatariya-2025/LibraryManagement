using LibraryManagement.DataAccess.Data;
using LibraryManagement.DataAccess.Repository.IRepository;
using LibraryManagement.Models.Models;

namespace LibraryManagement.DataAccess.Repository;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }
}
