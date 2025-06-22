using ExternalProvider.Abstract;
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
                var users = await _externalUserService.GetAllUsersAsync();

                foreach (var user in users)
                {
                    Console.WriteLine($"{user.Id}: {user.FirstName} {user.LastName} - {user.Email}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching users.");
            }
        }
    }
}
