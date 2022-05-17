using FoodDeliveringDomain.Models;
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

                //var userRoles = context.UserRoles.Where(o => o.Id == user.Id).ToList();
                //foreach (var userRole in userRoles)
                //{
                //    var role = context.Roles.Where(o => o.Id == userRole.RoleId).FirstOrDefault();
                //    if (role != null)
                //    {
                //        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                //    }
                //}

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

        //public async Task<User> UpdatePasswordUserAsync( 
        //  RegisterUser  input,
        //   [Service] FoodDeliveringAppContext context)
        //{
        //    var user = context.Users.Where(o => o.UserName == input.UserName).FirstOrDefault();
        //    if (user != null)
        //    {
        //        user.FullName = input.FullName;
        //        user.Email = input.Email;
        //        user.UserName = input.UserName;
        //        user.Password = input.Password;


        //        context.Users.Update(user);
        //        await context.SaveChangesAsync();
        //    }
        //    return await Task.FromResult(user);
        //}
    }
}
