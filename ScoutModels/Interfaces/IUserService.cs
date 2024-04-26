using System.Collections.Generic;
using ScoutDomains.Analysis;

namespace ScoutModels.Interfaces
{
    public interface IUserService
    {
        List<UserDomain> GetUserList();
    }
}