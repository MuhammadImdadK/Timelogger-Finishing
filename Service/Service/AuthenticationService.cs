using Common.Enums;
using Common.Messages;
using Common.ResponseModels;
using Microsoft.Extensions.Logging;
using Model.Interface;
using Model.ModelSql;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Service.Service
{
    public class AuthenticationManager
    {
        private int msl = 24;
        private int maxIterations = 100000;
        private int maxHashSize = 256;
        private readonly Regex passwordCriteria = new(@"/(?=(.*[A-z]{2,}))(?=(.*?[^ \w]{2,}))(?=(.*?\d){2,})+^(.){12,}$/");

        public bool MatchesCriteria(string password)
        {
            return passwordCriteria.Match(password).Success;
        }

        public byte[] GenerateRandomBytes(int length)
        {
            var temp = new byte[length];
            using var cryptoProvide = new RNGCryptoServiceProvider();
            cryptoProvide.GetNonZeroBytes(temp);
            return temp;
        }
        public byte[] GetSalt()
        {
            return GetSalt(msl);
        }
        private static byte[] GetSalt(int maximumSaltLength)
        {
            var salt = new byte[maximumSaltLength];
            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetNonZeroBytes(salt);
            }

            return salt;
        }
        public bool GetTimesafeDifference(byte[] sourceA, byte[] sourceB)
        {
            var diff = sourceA.Length ^ sourceB.Length;
            for (var i = 0; i < sourceA.Length && i < sourceB.Length; i++)
            {
                diff |= sourceA[i] ^ sourceB[i];
            }

            return diff == 0;
        }

        public string GenerateHash(byte[] password, byte[] salt)
        {
            HashAlgorithm algorithm = new SHA256CryptoServiceProvider();
            algorithm.Initialize();
            var combined = new byte[password.Length + salt.Length];
            for (var i = 0; i < password.Length; i++)
            {
                combined[i] = password[i];
            }

            for (var i = 0; i < salt.Length; i++)
            {
                combined[i + password.Length] = salt[i];
            }

            return Convert.ToBase64String(algorithm.ComputeHash(combined));
        }
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IRepository repository;
        private readonly ILogger<AuthenticationService> logger;

        public AuthenticationService(
            IRepository repository,
            ILogger<AuthenticationService> logger
            )
        {
            this.repository = repository;
            this.logger = logger;
        }
        Response IAuthenticationService.Login(string username, string password)
        {
            Response response = null;
            IEnumerable<User> users = repository.GetQueryableWithOutTracking<User>();

            logger.LogError("Users found: {count}", users.Count());

            User user = users.Where(x => x.Username.Equals(username)).FirstOrDefault();

            if (user != null)
            {
                if (user.Salt?.Length == 0)
                {
                    if(user.Password == password)
                    {
                        response = new Response()
                        {
                            Data = user,
                            Message = ResponseMessage.DefaultMessage[ResponseStatus.Success],
                            Status = ResponseStatus.Success
                        };
                    } else
                    {
                        response = new Response()
                        {
                            Message = ResponseMessage.DefaultMessage[ResponseStatus.AuthenticationFailed],
                            Status = ResponseStatus.AuthenticationFailed
                        };
                    }
                }
                else 
                {
                    var authMan = new AuthenticationManager();
                    var pass = Encoding.UTF8.GetBytes(user.Password);
                    var entry = Encoding.UTF8.GetBytes(authMan.GenerateHash(Encoding.UTF8.GetBytes( password), user.Salt));

                    if (authMan.GenerateHash(Encoding.UTF8.GetBytes(password),user.Salt).Equals(user.Password))
                    {
                        response = new Response()
                        {
                            Data = user,
                            Message = ResponseMessage.DefaultMessage[ResponseStatus.Success],
                            Status = ResponseStatus.Success
                        };
                    }
                    else
                    {
                        response = new Response()
                        {
                            Message = ResponseMessage.DefaultMessage[ResponseStatus.AuthenticationFailed],
                            Status = ResponseStatus.AuthenticationFailed
                        };
                    }
                }
            }
            else
            {
                response = new Response()
                {
                    Message = ResponseMessage.DefaultMessage[ResponseStatus.AuthenticationFailed],
                    Status = ResponseStatus.AuthenticationFailed
                };
            }
            return response;
        }
    }
}
