using System;
using System.Reflection;
using DbUp;

namespace SqlMigration
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // default to local
            string connectionString = "Server=localhost\\SQLEXPRESS;Database=Profiles;Trusted_Connection=True;";
            // This will eventually be passed on through the ci pipeline
            if (args.Length > 0)
            {
                connectionString = args[0];
            }
            Console.WriteLine("Connection string: " + connectionString);
            UpdateDatabase(connectionString);
        }

        private static void UpdateDatabase(string connectionString)
        {
            EnsureDatabase.For.SqlDatabase(connectionString); //Creates database if not exist

            var upgradeEngineBuilder = DeployChanges.To
                            .SqlDatabase(connectionString, null)
                            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                            .WithTransaction()
                            .LogToConsole();

            var upgrader = upgradeEngineBuilder.Build();
            if (upgrader.IsUpgradeRequired())
            {
                Console.WriteLine("Upgrade is requireD");
                var result = upgrader.PerformUpgrade();
                if (!result.Successful)
                {
                    Console.Out.WriteLine("Failed to upgrade database");
                }
                else
                {
                    Console.Out.WriteLine("Database  up grade successful");
                }
            }
        }
    }
}