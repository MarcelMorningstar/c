using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SecureApp
{
    public class UserController : Controller
    {
        private readonly string _connectionString = "YourSecureConnectionStringHere";

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest("Invalid input");
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT Role FROM Users WHERE Username = @Username AND PasswordHash = HASHBYTES('SHA2_256', @Password)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);
                    conn.Open();
                    var role = cmd.ExecuteScalar()?.ToString();

                    if (role == null)
                    {
                        return Unauthorized("Invalid credentials");
                    }

                    var claims = new List<Claim> { new Claim(ClaimTypes.Name, username), new Claim(ClaimTypes.Role, role) };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    
                    return Ok("Login successful");
                }
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult SecureAdminEndpoint()
        {
            return Ok("Welcome, Admin!");
        }
    }

    public class SecurityTests
    {
        public void TestSQLInjectionPrevention()
        {
            string maliciousInput = "' OR '1'='1";
            UserController controller = new UserController();
            var result = controller.Login(maliciousInput, "password");
            Console.WriteLine(result.ToString()); // Should return Unauthorized
        }
    }
}
