using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RequirementsScheduler.WebApiHost.BLL.Model;
using RequirementsScheduler.WebApiHost.Core.Service;

namespace RequirementsScheduler2.Identity
{
    public class TokenProviderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenProviderOptions _options;

        public TokenProviderMiddleware(
            RequestDelegate next,
            IOptions<TokenProviderOptions> options,
            IUserService service)
        {
            _next = next;
            _options = options.Value;

            UserService = service;
        }

        private IUserService UserService { get; }

        public Task Invoke(HttpContext context)
        {
            // If the request path doesn't match, skip
            if (!context.Request.Path.Equals(_options.Path, StringComparison.Ordinal)) return _next(context);

            // Request must be POST with Content-Type: application/x-www-form-urlencoded
            if (!context.Request.Method.Equals("POST")
                || !context.Request.HasFormContentType)
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync(JsonConvert.SerializeObject(new {Message = "Bad request."}));
            }

            return GenerateToken(context);
        }

        private async Task GenerateToken(HttpContext context)
        {
            var username = context.Request.Form["username"];
            var password = context.Request.Form["password"];

            var user = UserService.GetByUserName(username);
            var identity = await GetIdentity(user, password);
            if (identity == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                    {Message = "Invalid username or password."}));
                return;
            }

            var now = DateTimeOffset.UtcNow;

            // Specifically add the jti (random nonce), iat (issued timestamp), and sub (subject/user) claims.
            // You can add other claims here, if you want:
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                //new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
                new Claim("role", user.IsAdmin ? "Admin" : "User"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Create the JWT and write it to a string
            var jwt = new JwtSecurityToken(
                _options.Issuer,
                _options.Audience,
                claims,
                now.DateTime,
                now.DateTime.Add(_options.Expiration),
                _options.SigningCredentials);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                expires_in = (int) _options.Expiration.TotalSeconds
            };

            // Serialize and return the response
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response,
                new JsonSerializerSettings {Formatting = Formatting.Indented}));
        }

        private Task<ClaimsIdentity> GetIdentity(User user, string password)
        {
            if (user != null && user.Password == password)
                return Task.FromResult(
                    new ClaimsIdentity(
                        new GenericIdentity(user.Username, "Token"),
                        new Claim[] { }));

            // Credentials are invalid, or account doesn't exist
            return Task.FromResult<ClaimsIdentity>(null);
        }
    }
}