using Api.Attributes;
using AutoMapper;
using DataLayer;
using DataLayer.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Services;
using System;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("AllowAllCorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
            services.AddAutoMapper(typeof(ProfileAutoMapping));
            services.AddAutoMapper(typeof(UserAuthenticationMapper));
            services.AddTransient<IDiagnosticsService, DiagnosticsService>();
            services.AddTransient<IProfileService, UserProfileService>();
            services.AddTransient<IProfileDataService, ProfileDataDatabaseService>();
            services.AddTransient<IPasswordService, PasswordService>();

            //We need to add NewtonSoft because net core json handler does not handle json patches yet - always get empty collection
            // See https://github.com/dotnet/aspnetcore/issues/13938
            services.AddControllers().AddNewtonsoftJson();

            services.AddSingleton(BuildLogger);

            // there may be a better way to include this middleware
            // I got it from https://stackoverflow.com/questions/41972518/how-to-add-global-authorizefilter-or-authorizeattribute-in-asp-net-core
            services.AddMvc(options =>
            {
                options.Filters.Add<SerilogMvcLoggingAttribute>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Get Serilog to auto add some middleware that auto logs request information
            // It logs the request round trip time which is pretty cool https://nblumhardt.com/2019/10/serilog-mvc-logging/

            app.UseSerilogRequestLogging();
            // TODO - we need to move to https at some point
            // app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors("AllowAllCorsPolicy");  //should be after UseRouting and before UseEndpoint

            //    app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // close and flush the log file to prevent second log file being created first time BuildLogger is run
            Log.CloseAndFlush();
        }

        public ILogger BuildLogger(IServiceProvider provider)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();
            return Log.Logger;
        }
    }
}