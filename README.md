1. Once Cloned Create a User and Database in PostgreSQL
 	1. CREATE USER username WITH password 'password';
	2. CREATE DATABASE database ;
	3. GRANT ALL PRIVILEGES ON DATABASE database  TO username;
	4. \c database
	5. GRANT ALL on SCHEMA public to username;
	6.  \c postgres
	7. GRANT ALL PRIVILEGES ON DATABASE database TO username;

	Note : Db name and username should be in lowercase

2. Do Migration in Powershell
 	1. dotnet tool install --global dotnet-ef
 	2. dotnet tool update --global dotnet-ef
  	3. dotnet ef migrations add InitialCreate
	4. dotnet ef database update 
