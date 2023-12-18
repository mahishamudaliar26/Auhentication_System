using Authentication_System.DataAccessLayer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
        builder.Services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = false;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
            };
        });

        builder.Services.AddSwaggerGen(option =>
        {
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
                             {
                         {
                               new OpenApiSecurityScheme
                                 {
                                     Reference = new OpenApiReference
                                     {
                                         Type = ReferenceType.SecurityScheme,
                                         Id = "Bearer"
                                     }
                                 },
                                 new string[] {}
                         }
                             });

        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        string logFilePath = "logs/logfile.txt";

        var localIPAddress = GetLocalIPAddress();
        var hostName = Dns.GetHostName().ToString();
       

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj} - Local IP: {LocalIPAddress}{NewLine}{Exception} - DeviceName: {hostName}{NewLine}{Exception}")
            .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj} - Local IP: {LocalIPAddress}{NewLine}{Exception} - DeviceName: {hostName}{NewLine}{Exception}")
            .Enrich.FromLogContext()
            .Enrich.WithProperty("LocalIPAddress", localIPAddress)
            .Enrich.WithProperty("hostName", hostName)
            .CreateLogger();

        builder.Host.UseSerilog();

        builder.Services.AddScoped<IAuthenticationDataAccess, AuthenticationDataAccess>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                // c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

        }

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
        static string GetLocalIPAddress()
        {
            string ipAddress = "";

            string hostName = Dns.GetHostName();
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);

            foreach (IPAddress address in addresses)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = address.ToString();
                    break;
                }
            }
            return ipAddress;
        }
    }
}