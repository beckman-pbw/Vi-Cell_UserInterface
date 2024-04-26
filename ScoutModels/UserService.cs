using System;
using System.Collections.Generic;
using System.IO;
using Ninject.Extensions.Logging;
using ScoutDataAccessLayer.DAL;
using ScoutDomains.Analysis;
using ScoutModels.Admin;
using ScoutModels.Interfaces;
using ScoutModels.Settings;
using ScoutUtilities.Common;
using ScoutUtilities.UIConfiguration;

namespace ScoutModels
{
    public class UserService : IUserService
    {
        private readonly ILogger _logger;

        public UserService(ILogger logger)
        {
            _logger = logger;
        }

        public List<UserDomain> GetUserList()
        {
            return UserModel.GetUserList();
        }

    }
}