using Common.ViewModels;
using Microsoft.Extensions.Logging;
using Model.Interface;
using Model.ModelSql;
using Service.Interface;
using Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Service
{
    public class UserService : IUserService
    {
        IRepository repository;
        private readonly ILogger<UserService> logger;

        public UserService(IRepository repository, ILogger<UserService> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }
        public List<User> GetUsers(int skip = 0, int take = 0)
        {
            List<User> response = repository.GetQueryableWithOutTracking<User>()
                .OrderByDescending(x => x.Modified)
                //.Skip(skip)
                //.Take(take)
                .ToList();
            response.ForEach(itm => itm.Role = Constants.Roles.FirstOrDefault(role => itm.RoleID == role.Id));
            return response;
        }

        public User? GetUserById(int Id)
        {
            User? response = repository.GetQueryableWithOutTracking<User>()
                .Where(x => x.Id.Equals(Id))
                .FirstOrDefault();
            return response;
        }

        public bool AddUser(User user)
        {
            try
            {
                repository.InsertModel(user);
                return repository.Save() > 0;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to create user: {message} {exception}", ex.Message, ex);
                return false;
            }
        }

        public bool EditUser(User? user)
        {
            try
            {
                repository.UpdateRange(new List<User>() { user });
                repository.Save();
                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogError("Failed to edit user: {message} {exception}", ex.Message, ex);
                return false;
            }
        }

        public List<DropdownModel> GetRolesForDropdown()
        {
            return repository.GetQueryableWithOutTracking<Role>().Where(x => x.IsActive.Equals(true))
                  .Select(x => new DropdownModel
                  {
                      Id = x.Id,
                      Name = x.Name,
                  }).ToList();
        }

        public void DeleteUser(User? user)
        {
            try
            {
                if (user == null)
                {
                    return;
                }

                repository.RemoveRange<User>(new List<User> { user });
            }
            catch (Exception ex)
            {
                this.logger.LogError("Failed to delete user: {message} {exception}", ex.Message, ex);
            }
        }

        public List<User> SearchFor(string searchTerm)
        {
            searchTerm = searchTerm?.ToLower() ?? string.Empty;
            return GetUsers().Where(itm => itm.Username.ToLower().Contains(searchTerm) ||
                itm.FirstName.ToLower().Contains(searchTerm) ||
                (itm.LastName?.ToLower().Contains(searchTerm) ?? false) ||
                itm.Email.ToLower().Contains(searchTerm) ||
                itm.EmployeeNumber.ToLower().Contains(searchTerm)
            ).ToList();
        }
    }
}
