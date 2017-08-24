// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
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
