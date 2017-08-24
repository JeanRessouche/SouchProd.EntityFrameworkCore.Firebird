[![Travis build status](https://img.shields.io/travis/souchprod/SouchProd.EntityFrameworkCore.Firebird.svg?label=travis-ci&branch=master)](https://travis-ci.org/souchprod/SouchProd.EntityFrameworkCore.Firebird)

# SouchProd.EntityFrameworkCore.Firebird

SouchProd.EntityFrameworkCore.Firebird is an Entity Framework Core provider built on top of the Firebird ADO.NET Data Provider. It enables use the Entity Framework Core ORM with Firebird (2.1, 2.5, 3.0).

## Status

Work in progress at an early stage**, **not** production ready, **not** beta ready.

## Roadmap

Version                | EF Core version | Content | Status
------------|------------|------------|------------
2.0 Preview 1 | 2.0 | First release, read/write support limited to basic field types | Available
2.0 Preview 2 | 2.0 | Last inserted id support | In progress
2.0 Preview 3 | 2.0 | Read/Write support for all the Firebird data types  | In progress
2.0 Preview 4 | 2.0 | Scaffolding support | Scheduled
2.0 Preview 5 | 2.0 | Migration support | Scheduled
2.0 Final | 2.0 | Cleanup, refactoring and perf tuning | Scheduled

## Contributing 

Wanna add a feature, fix a bug or improve my crappy code ? 

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request :D

## Credits

This project inherit from the [Microsoft Entity Framework Core](https://github.com/aspnet/EntityFrameworkCore) (under Apache licence).

This  benefit also from the great work made on the [Pomelos MySql EF Core provider](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql), a large part of this repository code was forked from this it.

## License

[MIT](https://github.com/SouchProd/SouchProd.EntityFrameworkCore.Firebird/blob/master/LICENSE)
