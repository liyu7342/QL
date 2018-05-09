using QL.Repository;
using QL.Repository.Interfaces;
using QL.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL.Services
{
    public class UserService:AppServiceBase,IUserService
    {
        readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public void Save(User user)
        {
            UnitOfWork.Begin();
            _userRepository.Insert(user);
            UnitOfWork.Commit();

        }
    }
}
