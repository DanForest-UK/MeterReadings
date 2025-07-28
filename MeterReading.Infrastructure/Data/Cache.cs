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
        readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Thread safe atomic account cache
        /// </summary>
        readonly Atom<Generic.HashSet<AccountId>> accountIds =
            Atom(new Generic.HashSet<AccountId>());

        /// <summary>
        /// Initializes a new instance of the Cache class
        /// </summary>
        public Cache(IServiceProvider serviceProvider) =>
            this.serviceProvider = serviceProvider;

        /// <summary>
        /// Gets the current set of account IDs from cache
        /// </summary>
        public Generic.HashSet<AccountId> GetAccountIds() =>
            accountIds.Value;

        /// <summary>
        /// Rebuilds account ID cache when accounts are modified
        /// </summary>
        public async Task OnAccountsModifiedAsync()
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<MeterReadingContext>();

                var accounts = await (from account in context.Accounts
                                      select account.AccountId).ToListAsync();

                accountIds.Swap(_ => accounts.ToHashSet());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to refresh cache: {ex.Message}");
            }
        }
    }
}
