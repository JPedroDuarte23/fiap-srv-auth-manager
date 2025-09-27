using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using AspNetCore.DataProtection.Aws.S3;
using FiapSrvAuthManager.Application.Interface;
using FiapSrvAuthManager.Application.Services;
using FiapSrvAuthManager.Infrastructure.Configuration;
using FiapSrvAuthManager.Infrastructure.Mappings;
using FiapSrvAuthManager.Infrastructure.Middleware;
using FiapSrvAuthManager.Infrastructure.Repository;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Serilog;
using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage]

var builder = WebApplication.CreateBuilder(args);

Log.Logger = SerilogConfiguration.ConfigureSerilog();
builder.Host.UseSerilog();

// 1. Configura��o da AWS
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSimpleSystemsManagement>();
builder.Services.AddAWSService<Amazon.S3.IAmazonS3>();

string mongoConnectionString;
string jwtSigningKey;
string databaseName = builder.Configuration["MongoDbSettings:DatabaseName"]!;

if (!builder.Environment.IsDevelopment())
{
    Log.Information("Ambiente de Produ��o. Buscando segredos do AWS Parameter Store.");
    var ssmClient = new AmazonSimpleSystemsManagementClient();

    // Busca a Connection String do MongoDB
    var mongoParameterName = builder.Configuration["ParameterStore:MongoConnectionString"];
    var mongoResponse = await ssmClient.GetParameterAsync(new GetParameterRequest
    {
        Name = mongoParameterName,
        WithDecryption = true
    });
    mongoConnectionString = mongoResponse.Parameter.Value;

    // Busca a Chave de Assinatura do JWT
    var jwtParameterName = builder.Configuration["ParameterStore:JwtRsaPrivateKey"];
    var jwtResponse = await ssmClient.GetParameterAsync(new GetParameterRequest
    {
        Name = jwtParameterName,
        WithDecryption = true
    });
    jwtSigningKey = jwtResponse.Parameter.Value;

    // 2. Configura��o do Data Protection com AWS S3
    var s3Bucket = builder.Configuration["DataProtection:S3BucketName"];
    var s3KeyPrefix = builder.Configuration["DataProtection:S3KeyPrefix"];
    var s3DataProtectionConfig = new S3XmlRepositoryConfig(s3Bucket) { KeyPrefix = s3KeyPrefix };

    builder.Services.AddDataProtection()
        .SetApplicationName("FiapSrvAuthManager")
        .PersistKeysToAwsS3(s3DataProtectionConfig);
}
else
{
    Log.Information("Ambiente de Desenvolvimento. Usando appsettings.json.");
    // Adicione a connection string de desenvolvimento ao seu appsettings.Development.json
    mongoConnectionString = builder.Configuration.GetConnectionString("MongoDbConnection")!;
    jwtSigningKey = builder.Configuration["Jwt:DevKey"]!;
}

// 3. Configura��o do MongoDB e Reposit�rios
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnectionString));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
MongoMappings.ConfigureMappings();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 4. Inje��o de Depend�ncia do AuthService com a chave JWT
// Precisamos usar uma factory para injetar a string 'jwtSigningKey' que buscamos
builder.Services.AddScoped<IAuthService>(sp =>
    new AuthService(
        sp.GetRequiredService<IUserRepository>(),
        sp.GetRequiredService<IConfiguration>(),
        sp.GetRequiredService<ILogger<AuthService>>(),
        jwtSigningKey // Injetando a chave obtida do Parameter Store ou appsettings
    ));

builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// -- Resto da configura��o (Swagger, Middlewares, etc.) --
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "FIAP Cloud Games - Auth API", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu token}"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
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
           Array.Empty<string>()
       }
   });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandler>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapControllers();
app.Run();