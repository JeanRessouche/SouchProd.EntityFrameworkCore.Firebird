// Copyright (c) SouchProd. All rights reserved. // TODO: Credits Pomelo Foundation & EFCore
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class FirebirdDesignTimeServices : IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IScaffoldingProviderCodeGenerator, FirebirdScaffoldingCodeGenerator>()
                .AddSingleton<IDatabaseModelFactory, FirebirdDatabaseModelFactory>()
                .AddSingleton<IAnnotationCodeGenerator, FirebirdAnnotationCodeGenerator>()
                .AddSingleton<IRelationalTypeMapper, FirebirdTypeMapper>();
        }
    }
}
