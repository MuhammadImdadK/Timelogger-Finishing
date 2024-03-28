using Common.ViewModels;
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
        IRepository _repository;
        public UserService(IRepository repository)
        {
            _repository = repository;
        }
        public List<User> GetUsers(int skip = 0, int take = 0)
        {
            List<User> response = _repository.GetQueryableWithOutTracking<User>()
                .OrderByDescending(x => x.Modified)
                //.Skip(skip)
                //.Take(take)
                .ToList();
            response.ForEach(itm => itm.Role = Constants.Roles.FirstOrDefault(role => itm.RoleID == role.Id));
            return response;
        }

        public User? GetUserById(int Id)
        {
            User? response = _repository.GetQueryableWithOutTracking<User>()
                .Where(x => x.Id.Equals(Id))
                .FirstOrDefault();
            return response;
        }

        public bool AddUser(User user)
        {
            _repository.InsertModel(user);
            return _repository.Save() > 0;
        }

        public bool EditUser(User? user)
        {
            try
            {
                _repository.UpdateRange(new List<User>() { user });
                _repository.Save();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<DropdownModel> GetRolesForDropdown()
        {
            return _repository.GetQueryableWithOutTracking<Role>().Where(x => x.IsActive.Equals(true))
                  .Select(x => new DropdownModel
                  {
                      Id = x.Id,
                      Name = x.Name,
                  }).ToList();
        }

        public void DeleteUser(User? user)
        {
            if (user == null)
            {
                return;
            }

            _repository.RemoveRange<User>(new List<User> { user });
        }

        public List<User> SearchFor(string searchTerm)
        {
            searchTerm = searchTerm?.ToLower() ?? string.Empty;
            return GetUsers().Where(itm => itm.Username.ToLower().Contains(searchTerm) ||
                itm.FirstName.ToLower().Contains(searchTerm) ||
                (itm.LastName?.ToLower().Contains(searchTerm) ?? false) ||
                itm.Email.ToLower().Contains(searchTerm) ||
                itm.EmployeeNumber.ToLower().Contains(searchTerm) ||
                (itm.Designation?.ToLower().Contains(searchTerm) ?? false)).ToList();
        }
    }
}
