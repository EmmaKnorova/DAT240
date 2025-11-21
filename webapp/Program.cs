using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TarlBreuJacoBaraKnor.Core.Domain.Users.Services;
using TarlBreuJacoBaraKnor.Infrastructure.Data;
using TarlBreuJacoBaraKnor.Pages.Admin.Helpers;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Ordering.Services;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Users;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;
using TarlBreuJacoBaraKnor.webapp.Pages.Courier.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<ShopContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(
        typeof(Program).Assembly,
        typeof(ShopContext).Assembly
    )   
);

builder.Services.AddScoped<RequireChangingPasswordFilter>();
builder.Services.AddScoped<RequireAccountApprovalFilter>();


// Register the OrderingService
builder.Services.AddScoped<IOrderingService, OrderingService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddIdentity<User, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ShopContext>()
                .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 12;
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Identity/AccessDenied";
    options.LoginPath = "/Identity/Login";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    options.SlidingExpiration = true;
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            var requestPath = context.Request.Path.Value ?? "";
            if (requestPath.StartsWith("/Admin"))
            {
                context.Response.Redirect("/Admin/Identity/Login?ReturnUrl=" + 
                    Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
            }
            else
            {
                context.Response.Redirect("/Identity/Login?ReturnUrl=" +
                    Uri.EscapeDataString(context.Request.Path + context.Request.QueryString));
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddGoogle("Google", googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId is missing in configuration");;
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret is missing in configuration");;
    googleOptions.CallbackPath = "/signin-google";
    googleOptions.CorrelationCookie.SameSite = SameSiteMode.Lax;
    googleOptions.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    googleOptions.Scope.Add("profile");
    googleOptions.Scope.Add("email");
    googleOptions.SaveTokens = true;
})
.AddMicrosoftAccount("Microsoft", microsoftOptions => 
{
    microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"] ?? throw new InvalidOperationException("Microsoft ClientSecret is missing in configuration");;
    microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"] ?? throw new InvalidOperationException("Microsoft ClientSecret is missing in configuration");;
    microsoftOptions.CallbackPath = "/signin-microsoft";
    microsoftOptions.CorrelationCookie.SameSite = SameSiteMode.Lax;
    microsoftOptions.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    microsoftOptions.SaveTokens = true;
});

builder.Services.ConfigureExternalCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"/keys"))
    .SetApplicationName("CampusEats")
    .UseCryptographicAlgorithms(
        new AuthenticatedEncryptorConfiguration
        {
            EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
            ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
        });

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHttpsRedirection();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopContext>();
    db.Database.Migrate();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Referrer-Policy", "no-referrer-when-downgrade");
    await next();
});

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

await DbSeeder.SeedData(app);

app.Run();
