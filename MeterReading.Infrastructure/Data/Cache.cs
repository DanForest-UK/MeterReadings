using LanguageExt;
using MeterReading.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Generic = System.Collections.Generic;
using static LanguageExt.Prelude;

namespace MeterReading.Infrastructure.Data
{
    public class Cache
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Thread safe atomic account cache
        /// </summary>
        readonly Atom<Generic.HashSet<AccountId>> accountIds = 
            Atom(new Generic.HashSet<AccountId>());

        public Cache(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            // Initialize the Atom with an empty HashSet
        }

        public Generic.HashSet<AccountId> GetAccountIds() =>
            accountIds.Value;

        /// <summary>
        /// Rebuild account ID cache when accounts modified - async version
        /// </summary>
        public async Task OnAccountsModifiedAsync()
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<MeterReadingContext>();

                var accounts = await context.Accounts
                    .Select(account => account.AccountId)
                    .ToListAsync();

                accountIds.Swap(_ => accounts.ToHashSet());
            }
            catch (Exception ex)
            {
                // Log error if you have logging configured
                Console.WriteLine($"Failed to refresh cache: {ex.Message}");
            }
        }     
    }
}