using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class FirebirdTestHelpers : TestHelpers
    {
        protected FirebirdTestHelpers()
        {
        }

        public static FirebirdTestHelpers Instance { get; } = new FirebirdTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
            => services.AddEntityFrameworkFirebird();

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseFirebird("Database=DummyDatabase");
    }
}
