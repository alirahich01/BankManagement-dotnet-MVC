using BankProjectJuin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;


namespace BankProjectJuin.Controllers
{
    public class HomeController : Controller

    {
        private readonly ILogger<HomeController> _logger;



        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;

        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Main(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                decimal balance = GetBalance(username);
                ViewBag.Balance = balance;
                ViewBag.Username = username; // Pass the username to the view
            }
            else
            {
                ViewBag.Balance = 0;
                ViewBag.Username = string.Empty;
            }

            return View();
        }


        private decimal GetBalance(string username)
        {

            decimal balance = 0;

            using (SqlConnection connection = new SqlConnection(@"Data Source=MOUAD;Initial Catalog=BankMangement;Integrated Security=True"))
            {
                connection.Open();

                string query = "SELECT s.Solde " +
                       "FROM Soldes s " +
                       "INNER JOIN Utilisateurs u ON u.Id = s.UtilisateurId " +
                       "WHERE u.Nom = @Nom";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Nom", username);

                    object result = command.ExecuteScalar();

                    if (result != null && decimal.TryParse(result.ToString(), out decimal parsedBalance))
                    {
                        balance = parsedBalance;
                    }
                }
            }

            return balance;
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}