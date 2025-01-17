﻿using Common.ViewModels;
using Model.ModelSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IUserService
    {
        public List<User> GetUsers(int skip=0, int take=0);
        public User? GetUserById(int Id);
        public bool AddUser(User user);
        public List<DropdownModel> GetRolesForDropdown();
        bool EditUser(User? modifyingUser);
        void DeleteUser(User? id);
        public List<User> SearchFor(string searchTerm);
    }
}
