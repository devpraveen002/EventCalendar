1. Once Cloned Create a User and Database in PostgreSQL
   a. CREATE USER username WITH password 'password';
   CREATE DATABASE database ;
   GRANT ALL PRIVILEGES ON DATABASE database  TO username;
   \c database
	 GRANT ALL on SCHEMA public to username;
   \c postgres
	GRANT ALL PRIVILEGES ON DATABASE database TO username;

	Note : Db name and username should be in lowercase

2. Do Migration in Powershell
   a. dotnet tool install --global dotnet-ef
      dotnet tool update --global dotnet-ef
      dotnet ef migrations add InitialCreate
      dotnet ef database update 
