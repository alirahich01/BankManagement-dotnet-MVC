using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Security.Claims;

namespace BankProjectJuin.Controllers
{
    [Authorize] // Restrict access to authenticated users
    public class TransferController : Controller
    {
        private readonly string _connectionString;

        public TransferController(IConfiguration configuration)
        {
            _connectionString = @"Data Source=MOUAD;Initial Catalog=BankMangement;Integrated Security=True";
        }

        public IActionResult Index()
        {
            return RedirectToAction("Transfert");
        }

        public IActionResult Transfert()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Transfer(string toAccount, decimal amount)
        {
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
            {
                bool isSufficientBalance = CheckSufficientBalance(username, amount);

                if (isSufficientBalance)
                {
                    UpdateBalance(username, -amount); // Subtract the transfer amount from the balance
                    UpdateBalance(toAccount, amount); // Add the transfer amount to the recipient's balance

                    // Redirect the user to the Main action with the updated username and balance as query parameters
                    return RedirectToAction("Main", "Home", new { username = username });
                }
                else
                {
                    ViewBag.ErrorMessage = "Insufficient balance.";
                }
            }

            // If any condition fails, redirect back to the Main action with empty username and balance
            return RedirectToAction("Main", "Home", new { username = string.Empty });
        }

        private bool CheckSufficientBalance(string username, decimal amount)
        {
            decimal currentBalance = GetBalance(username);
            return currentBalance >= amount;
        }

        private void UpdateBalance(string username, decimal amount)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "UPDATE dbo.Soldes SET Solde = Solde + @Amount WHERE UtilisateurId = (SELECT Id FROM dbo.Utilisateurs WHERE Nom = @Username)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Amount", amount);
                    command.Parameters.AddWithValue("@Username", username);

                    command.ExecuteNonQuery();
                }
            }
        }

        private decimal GetBalance(string username)
        {
            decimal balance = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT s.Solde " +
                               "FROM Soldes s " +
                               "INNER JOIN Utilisateurs u ON u.Id = s.UtilisateurId " +
                               "WHERE u.Nom = @Username";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    object result = command.ExecuteScalar();

                    if (result != null && decimal.TryParse(result.ToString(), out decimal parsedBalance))
                    {
                        balance = parsedBalance;
                    }
                }
            }

            return balance;
        }
    }
}


