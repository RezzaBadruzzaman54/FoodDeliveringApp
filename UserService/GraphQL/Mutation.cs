using FoodDeliveringDomain.Models;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UserService.GraphQL
{
    public class Mutation
    {
        public async Task<UserData> RegisterUserAsync(
          RegisterUser input,
          [Service] FoodDeliveringAppContext context)
        {
            var user = context.Users.Where(u => u.UserName == input.UserName).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                UserName = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password) // encrypt password
            };

            // EF
            var ret = context.Users.Add(newUser);
            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData
            {
                Id = newUser.Id,
                UserName = newUser.UserName,
                Email = newUser.Email,
                FullName = newUser.FullName
            });
        }

        public async Task<UserToken> LoginAsync(
          LoginUser input,
          [Service] IOptions<TokenSettings> tokenSettings, // setting token
          [Service] FoodDeliveringAppContext context) // EF
        {
            var user = context.Users.Where(u => u.UserName == input.Username).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new UserToken(null, null, "Username or password was invalid"));
            }
            bool valid = BCrypt.Net.BCrypt.Verify(input.Password, user.Password);
            if (valid)
            {
                // generate jwt token
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                // jwt payload
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.UserName));

                var userBuyers = context.Buyers.Where(b => b.UserId == user.Id).ToList();
                foreach (var userBuyer in userBuyers)
                {
                    var role = context.Roles.Where(o => o.Id == userBuyer.RoleId).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }
                
                var userCouriers = context.Couriers.Where(c => c.UserId == user.Id).ToList();
                foreach (var userCourier in userCouriers)
                {
                    var role = context.Roles.Where(o => o.Id == userCourier.RoleId).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var userEmployees = context.Employees.Where(c => c.UserId == user.Id).ToList();
                foreach (var userEmployee in userEmployees)
                {
                    var role = context.Roles.Where(o => o.Id == userEmployee.RoleId).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var expired = DateTime.Now.AddHours(3);
                var jwtToken = new JwtSecurityToken(
                    issuer: tokenSettings.Value.Issuer,
                    audience: tokenSettings.Value.Audience,
                    expires: expired,
                    claims: claims, // jwt payload
                    signingCredentials: credentials // signature
                );

                return await Task.FromResult(
                    new UserToken(new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expired.ToString(), null));
                //return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }

            return await Task.FromResult(new UserToken(null, null, Message: "Username or password was invalid"));
        }

        [Authorize]
        public async Task<User> UpdatePasswordUserAsync(
            UserChangePasswordInput input,
           [Service] FoodDeliveringAppContext context)
        {
            var user = context.Users.Where(o => o.Id == input.Id).FirstOrDefault();
            if (user != null)
            {

                user.Password = BCrypt.Net.BCrypt.HashPassword(input.Password);


                context.Users.Update(user);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(user);
        }

        //Manage User By Admin
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<Buyer> AddUserToBuyerAsync(
           AddUserToBuyerInput input,
           [Service] FoodDeliveringAppContext context)
        {
            var user = context.Users.Where(u => u.Id == input.UserId).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new Buyer());
            }
            // EF
            var buyer = new Buyer
            {
                UserId = input.UserId,
                RoleId = input.RoleId,
                Address = input.Address

            };

            var ret = context.Buyers.Add(buyer);
            await context.SaveChangesAsync();

            return ret.Entity;
        }

        [Authorize(Roles = new[] { "ADMIN", "MANAGER" })]
        public async Task<Courier> AddUserToCourierAsync(
          AddUserToCourierInput input,
          [Service] FoodDeliveringAppContext context)
        {
            var user = context.Users.Where(u => u.Id == input.UserId).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new Courier());
            }
            // EF
            var courier = new Courier
            {
                UserId = input.UserId,
                RoleId = input.RoleId,
                NumberOfVehicles = input.NumberOfVehicles

            };

            var ret = context.Couriers.Add(courier);
            await context.SaveChangesAsync();

            return ret.Entity;
        }

        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<Employee> AddUserToEmployeeAsync(
         AddUserToEmployeeInput input,
         [Service] FoodDeliveringAppContext context)
        {
            var user = context.Users.Where(u => u.Id == input.UserId).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new Employee());
            }
            // EF
            var employee = new Employee
            {
                UserId = input.UserId,
                RoleId = input.RoleId,
                EmployeeNumber = input.EmployeeNumber

            };

            var ret = context.Employees.Add(employee);
            await context.SaveChangesAsync();

            return ret.Entity;
        }

        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<User> UpdateUserAsync(
         UserInput input,
         [Service] FoodDeliveringAppContext context)
        {
            var user = context.Users.Where(u => u.Id == input.Id).FirstOrDefault();
            if (user != null)
            {
                user.FullName = input.FullName;
                user.Email = input.Email;
                user.UserName = input.UserName;
                user.Password = BCrypt.Net.BCrypt.HashPassword(input.Password);

                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(user);
        }

        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<User> DeletUsertByIdAsync(
          int id,
          [Service] FoodDeliveringAppContext context)
        {
            var user = context.Users.Where(o => o.Id == id).FirstOrDefault();
            if (user != null)
            {
                context.Users.Remove(user);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(user);
        }

        //manage courier by manager
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<CourierData> UpdateCourierAsync(
         CourierInput input,
         [Service] FoodDeliveringAppContext context)
        {
            var courier = context.Couriers.Include(c=>c.User).Where(c =>c.Id == input.Id).FirstOrDefault();
            if (courier != null)
            {
                courier.UserId = input.UserId;
                courier.RoleId = input.RoleId;
                courier.NumberOfVehicles = input.NumberOfVehicles;
                courier.User.FullName = input.FullName;
                courier.User.Email = input.Email;
                courier.User.UserName = input.UserName;
                courier.User.Password = BCrypt.Net.BCrypt.HashPassword(input.Password);

                context.Couriers.Update(courier);
                await context.SaveChangesAsync();
                context.Users.Update(courier.User);
                await context.SaveChangesAsync();
            }
            //return await Task.FromResult(courier);

            return await Task.FromResult(new CourierData
            {
                Id = courier.Id,
                UserId = courier.UserId,
                RoleId = courier.RoleId,
                NumberOfVehicles = courier.NumberOfVehicles,
                FullName = courier.User.FullName,
                Email = courier.User.Email,
                UserName = courier.User.UserName,
                Password = courier.User.Password
            });
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Courier> DeleteCourierByIdAsync(
         int id,
         [Service] FoodDeliveringAppContext context)
        {
            var courier = context.Couriers.Where(o => o.Id == id).FirstOrDefault();
            if (courier != null)
            {
                context.Couriers.Remove(courier);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(courier);
        }
    }
}
