using ExternalProvider.Abstract;
using ExternalProvider.Models.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalProviderConsoleApp
{
    public class ExternalProviderConsoleAppRunner
    {
        private readonly IExternalUserService _externalUserService;
        private readonly ILogger<ExternalProviderConsoleAppRunner> _logger;

        public ExternalProviderConsoleAppRunner(IExternalUserService externalUserService, ILogger<ExternalProviderConsoleAppRunner> logger)
        {
            _externalUserService = externalUserService;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            try
            {
                while (true) 
                {
                    Console.WriteLine("\n\n");
                    Console.WriteLine("Enter 1 to print record of particular user " +
                    "\n Or Enter 2 to print all user records " +
                    "\n Or 3 to exit : (1/2/3)...");

                    var option = Console.ReadLine();

                    if (option == null)
                        break;

                    switch (option.Trim())
                    {
                        case "1":
                            Console.WriteLine("Please enter user id...");
                            var inputId = Console.ReadLine();
                            
                            if (int.TryParse(inputId, out int userId))
                            {
                                var userDetails = await _externalUserService.GetUserByIdAsync(userId);
                                Console.WriteLine($"{userDetails.Id}: {userDetails.FirstName} {userDetails.LastName} - {userDetails.Email}");
                            }
                            else
                            {
                                Console.WriteLine("Invalid User Id. Please enter a valid User Id.");
                            }                            
                            break;

                        case "2":
                            var users = await _externalUserService.GetAllUsersAsync();

                            foreach (var user in users)
                            {
                                Console.WriteLine($"{user.Id}: {user.FirstName} {user.LastName} - {user.Email}");
                            }
                            break;

                        case "3":
                            return;

                        default:
                            Console.WriteLine("Please enter either 1, 2 or 3 and try again");
                            break;
                    
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching users.");
            }
        }
    }
}
