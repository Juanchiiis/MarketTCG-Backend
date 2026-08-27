using MarketGO.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar los controladores al servidor
builder.Services.AddControllers();

// 2. Registrar el DbContext y decirle que use SQL Server con la conexión del appsettings
builder.Services.AddDbContext<MarketGOContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSQL")));

// 3. Agregar los servicios necesarios para que Swagger funcione
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // Le decimos a Swagger que vamos a usar tokens tipo Bearer (JWT)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT aquí.\r\n\r\nFormato: 'Bearer [espacio] [tu_token]'\r\nEjemplo: 'Bearer eyJhbGciOiJIUzI1Ni...'"
    });

    // Le decimos a Swagger que aplique este candado a todos los endpoints
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {} // No se requieren scopes adicionales
        }
    });
});

// 4. Configuramos las politicas de Cors para permitir que el frontend (que corre en otro puerto) pueda hacer peticiones a este backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        // Cambiamos WithOrigins por AllowAnyOrigin temporalmente
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// --- INICIO CONFIGURACIÓN JWT ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        // Aquí C# va a tu appsettings.json a sacar los datos:
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
// --- FIN CONFIGURACIÓN JWT ---

// Aquí debería estar tu línea original: var app = builder.Build();

var app = builder.Build();

// ==========================================
// ZONA DE MIDDLEWARES (El orden es estricto)
// ==========================================

// 5. Configurar la interfaz gráfica de Swagger (solo para cuando estás desarrollando)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
//Habilita la carpeta de wwwroot para servir imagenes fisicas.
app.UseStaticFiles();


// 6. Encendemos el CORS usando el nombre exacto que le dimos arriba 
// (Debe ir entre UseRouting y UseAuthorization)
app.UseCors("PermitirFrontend");

app.UseAuthentication(); // <-- ESTE ES EL NUEVO (Verifica quién eres)
app.UseAuthorization();

app.MapControllers();

// 7. Arrancar el servidor (Solo debe existir un app.Run y siempre va al final)
app.Run();

