using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Claims;

namespace BankProjectJuin.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _connectionString;

        public AccountController(IConfiguration configuration)
        {
            _connectionString = @"Data Source=MOUAD;Initial Catalog=BankMangement;Integrated Security=True";

        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            (bool isValidCredentials, string name) = VerifyCredentials(username, password);

            if (isValidCredentials)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, name)
                };

                var identity = new ClaimsIdentity(claims, "ApplicationCookie");
                var principal = new ClaimsPrincipal(identity);

                HttpContext.SignInAsync("Cookies", principal); // This line sets the authentication cookie

                /*return RedirectToAction("Main", "Home");*/
                return RedirectToAction("Main", "Home", new { username = name });


            }
            else
            {
                ViewBag.ErrorMessage = "Nom d'utilisateur ou mot de passe incorrect.";
                return View();
            }
        }

        private (bool, string) VerifyCredentials(string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*), Nom FROM dbo.Utilisateurs WHERE Nom = @Username AND MotDePasse = @Password GROUP BY Nom";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int count = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            return (count > 0, name);
                        }
                    }
                }
            }

            return (false, null);
        }

    }
}