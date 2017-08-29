[![Travis build status](https://img.shields.io/travis/souchprod/SouchProd.EntityFrameworkCore.Firebird.svg?label=build&branch=master)](https://travis-ci.org/souchprod/SouchProd.EntityFrameworkCore.Firebird) [![NuGet][main-nuget-badge]][main-nuget]  [![NuGet][pre-nuget-badge]][pre-nuget] [![Open Source Love](https://badges.frapsoft.com/os/mit/mit.svg?v=102)](https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird/blob/master/LICENSE)

# SouchProd.EntityFrameworkCore.Firebird

SouchProd.EntityFrameworkCore.Firebird is an Entity Framework Core provider built on top of the Firebird ADO.NET Data Provider. It enables use the Entity Framework Core 2.0 ORM with Firebird (2.x, 3.0) and Interbase.

## Status

Work in progress at an early stage, **not** production ready, but now beta ready.
 
  - Migrations partially supported.

  - Table Inludes not yet supported.
  
## Features

CRUD operations are working (insert, update, delete, select), Scaffolding and migrations too (still contain a few bugs, some scenario could lead to an exception). Firebird 3 Identity columns are supported. Firebird 4 Alpha metadata charlength limitation (63 vs 31 before) supported.

## Roadmap

Version | Content | Status
------------|------------|------------
**2.0 Preview 1** | **First release, read/write support limited to basic field types** | **:heavy_check_mark: Available**
**2.0 Preview 2** | **Last inserted id support** | **:heavy_check_mark: Available**
**2.0 Preview 3** | **Cast, Substring, Replace, Math & other Linq/Db functions support** | **:heavy_check_mark: Available**
**2.0 Preview 4** | **Migration support** | **:heavy_check_mark: Available**
**2.0 Preview 5** | **Read/Write support for the BLOB & CLOB fields** | **:heavy_check_mark: Available**
**2.0 Preview 6** | **:exclamation: Scaffolding support** | **:heavy_check_mark: Available**
2.0 Final | Cleanup, refactoring and perf tuning | :confetti_ball: Scheduled

## Hot-to

I recommend you [this lecture](http://www.learnentityframeworkcore.com/) to discover more about Entity Core.

If you are starting from an **existing** database (Database First), you should use the Scaffolding capability. It will create your DbContext the classes for all the discovered entities (do not foget, all your tables need a PK to match EntityFramework needs!) and the FluentApi description.

Under VS2017, open a Package Manager console in your project and execute the scaffold command as below (you must adapt the parameters):

  `Scaffold-DbContext "User=SYSDBA;Password=masterkey;Database=PATH_OR_ALIAS_TO_YOUR_DB_HERE;DataSource=127.0.0.1;Port=3050;Dialect=3;Charset=UTF8;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;" "souchprod.EntityFrameworkCore.Firebird" -OutputDir Entities --Context CardioXpDb -DataAnnotations -force -verbose`

If you are in a CODE FIRST mode, you should use the **Migrations** system. Please refert to the [relevant documentation](http://www.learnentityframeworkcore.com/migrations).

Please refer to the (Asp.Net Core sample application)[https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird/tree/master/samples/AspNetCore] in this repo for guidance.

## Dependencies 

The nuget package [**SouchProd.Data.FirebirdClient**](https://www.nuget.org/packages/SouchProd.Data.FirebirdClient/) is currently required. It will be replaced by the official FirebirdSql.Data.FirebirdClient as soon as it will be updated accordingly to support .Net standard 2 (Will be released soon with the version 5.11.0.0).

## Compatibility

This assembly can be conssumed in a project targeting .NETSTANDARD 2.0 or the .NET Core 2.0 Framework.
The framework 1.0 and 1.1, as well as .NETSTANDARD 1.6 and older are **not** supported.

## Contributing 

Wanna add a feature, fix a bug or improve my crappy code ? 

1. Fork it!
2. Create your feature branch: \"git checkout -b my-new-feature\"
3. Commit your changes: \"git commit -am 'Add some feature'\"
4. Push to the branch: \"git push origin my-new-feature\"
5. Submit a pull request :D

## Credits

This project inherit from the [Microsoft Entity Framework Core](https://github.com/aspnet/EntityFrameworkCore) (under Apache licence).

It benefit also from the great work made on the [Pomelos MySql EF Core provider](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql), a large part of this repository code was forked from this it.

This project couldn´t exist without the [.NET Firebird Client](https://github.com/cincuranet/FirebirdSql.Data.FirebirdClient) made and supported by Jiri Cincura.

## License

[MIT](https://github.com/SouchProd/SouchProd.EntityFrameworkCore.Firebird/blob/master/LICENSE)

[main-nuget]: https://www.nuget.org/packages/SouchProd.EntityFrameworkCore.Firebird/
[main-nuget-badge]: https://img.shields.io/nuget/v/SouchProd.EntityFrameworkCore.Firebird.svg?label=nuget 

[pre-nuget]: https://www.nuget.org/packages/SouchProd.EntityFrameworkCore.Firebird/
[pre-nuget-badge]: https://img.shields.io/nuget/vpre/SouchProd.EntityFrameworkCore.Firebird.svg?label=nuget
