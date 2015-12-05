﻿using Migree.Core.Definitions;
using Migree.Core.Interfaces.Models;
using System;

namespace Migree.Core.Models
{
    public class User : StorageModel, IUser
    {
        public static string GetPartitionKey(UserType userType)
        {
            return userType.ToString().ToLower();
        }

        /// <summary>
        /// Default, used by Azure
        /// </summary>
        public User() { }

        /// <summary>
        /// Create new user constructor
        /// </summary>
        /// <param name="userType"></param>
        public User(UserType userType)
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = GetPartitionKey(userType);
        }

        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserType UserType { get; set; }
    }
}
