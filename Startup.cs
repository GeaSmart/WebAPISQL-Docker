using LibraryNetCoreAPI.Filtros;
using LibraryNetCoreAPI.Servicios;
using LibraryNetCoreAPI.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace LibraryNetCoreAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
                {
                    options.Filters.Add(typeof(FiltroExcepciones));
                    options.Conventions.Add(new SwaggerAgruparPorVersion());
                }
            )
                .AddNewtonsoftJson(); //Configuring NewtonsoftJson patch

            services.AddDbContext<ApplicationDBContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("defaultConnection"));
            });

            //Configuraciones de Swagger
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo() { 
                    Title = "Web API",
                    Description = "Este es un api para el control de una biblioteca, donde se manejan libros y autores, también comentarios de libros",
                    Version = "version 1",
                    Contact = new OpenApiContact
                    {
                        Name = "Gerson Azabache",
                        Email = "gerson@bravedeveloper.com",
                        Url = new Uri("https://bravedeveloper.com")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT"
                    },
                    TermsOfService = new Uri("https://www.washingtonpost.com/terms-of-service/2011/11/18/gIQAldiYiN_story.html")
                });

                config.SwaggerDoc("v2", new OpenApiInfo() { Title = "Web API", Description = "v2" });

                config.OperationFilter<AgregarHeaderVersion>();


                config.IncludeXmlComments(xmlPath);

                config.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme()
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header
                    }
                );

                config.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
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
                            new string[]{}
                        }
                    }
                    
                );
            });
            services.AddAutoMapper(typeof(Startup));

            //servicios de Identity
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWTKey"])),
                    ClockSkew =TimeSpan.Zero
                }
            );

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDBContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization(options =>
                options.AddPolicy("isAdmin", x => x.RequireClaim("isAdmin"))
                //options.AddPolicy("isSeller", x => x.RequireClaim("isSeller")) se puede agregar varias políticas segun usuarios
            );

            //Encriptacion y desencriptación
            services.AddDataProtection();

            //Habilitacion de CORS
            services.AddCors( options =>
                options.AddDefaultPolicy( x => x.WithOrigins("https://apirequest.io").AllowAnyMethod().AllowAnyHeader()
                .WithExposedHeaders(new string[] { "cantidadTotalRegistros" }))
            );

            //servicio de hash
            services.AddTransient<HashService>();

            //application insights for azure debugging
            services.AddApplicationInsightsTelemetry(Configuration["ApplicationInsights:ConnectionString"]);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //Configuración de middleware SWAGGER
            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                config.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
                config.RoutePrefix = ""; //para evitar problemas con la ruta en la que se lanza la aplicación al correr el proyecto
            }
            );
        }
    }
}
