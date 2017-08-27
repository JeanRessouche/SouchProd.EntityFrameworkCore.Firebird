# Asp.Net Core Sample Projet

This very basic sample demonstrate the usage of the [Entity Framework Core Provider for Firebird](https://github.com/souchprod/SouchProd.EntityFrameworkCore.Firebird).

# Configuration

	1. Create a Firebird database
	
	2. Edit the ConnectionString in the appsettings.json file to match your newly create database.
	
	3. Use the [migration](http://www.learnentityframeworkcore.com/migrations#applying-a-migration) capability to restore the database schema and default data :
		
		a. Simply start the application, **_dbContext.Database.Migrate()** is automatically called and will apply all the pending migrations.
		
		b. [Command line]
		    dotnet ef database update

		c.[Package Manager Console]
			update-database
		
# Usage

	- GET /api/autors list the authors
		GET /api/autors/1 return the author id 1.
		POST api/authors add the embeded item to the db.
		PUT api/authors/1 update the specified item with then embeded item.
		DELETE api/authors/5 delete the specified item from the db.