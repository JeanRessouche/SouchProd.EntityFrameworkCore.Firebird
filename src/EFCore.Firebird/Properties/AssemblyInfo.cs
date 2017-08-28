// Copyright (c) 2017 Jean Ressouche @SouchProd. All rights reserved.
// https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird
// This code inherit from the .Net Foundation Entity Core repository (Apache licence)
// and from the Pomelo Foundation Mysql provider repository (MIT licence).
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Design;

[assembly: AssemblyTitle("SouchProd.EntityFrameworkCore.Firebird")]
[assembly: AssemblyDescription("Entity Framework Core provider built on top of the Firebird ADO.NET Data Provider. It enables use the Entity Framework Core 2.0 ORM with Firebird (2.x, 3.0) and Interbase.")]
[assembly: DesignTimeProviderServices("Microsoft.EntityFrameworkCore.Design.Internal.FirebirdDesignTimeServices")]
[assembly: InternalsVisibleTo("SouchProd.EntityFrameworkCore.Firebird.Tests")]
