using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // from launchSettings.json
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            //Get our logging settings from appsettings file with overwrites for our environment
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            //Create a logger here so we can log the application before services have been configured
            // https://nblumhardt.com/2019/10/serilog-in-aspnetcore-3/
            // I am using settings from app settingsfile so they can be overridden per environment
            // In particular I am using:
            //      json formatted log file - gives us a structured log that can be easily analyzed with free tools
            //      Serilog supports a tool called Seq sink that lets you view the structured stuff
            //      set the log max file size
            //      set how many log files to keep before serilog auto deletes the old ones - no more endlessly growing logs
            Log.Logger = new LoggerConfiguration()
                // USe the app settings file - https://github.com/serilog/serilog-settings-configuration,
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("starting app");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Application failed to start");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}