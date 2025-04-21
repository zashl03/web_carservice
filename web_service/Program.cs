using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Identity;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

// 1. Подключение БД
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Identity: UI + токены + куки + роли
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        // другие опции пароля/логина…
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 3. MVC + Razor Pages (с конвенциями безопасности)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
    // защита всего раздела Manage (куда входит MyCars)
    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
});

// 4. Прочие сервисы
builder.Services.AddAutoMapper(typeof(DomainToEntityProfile).Assembly);
builder.Services.AddScoped<ICarService, CarService>();

var app = builder.Build();

// 5. Создание ролей при старте (без изменений)
using (var scope = app.Services.CreateScope())
{
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Client", "Storekeeper", "Administrator" };
    foreach (var role in roles)
    {
        if (!await roleMgr.RoleExistsAsync(role))
            await roleMgr.CreateAsync(new IdentityRole(role));
    }
}

// 6. Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Обязательно: сначала аутентификация, затем авторизация
app.UseAuthentication();
app.UseAuthorization();

// 7. Маршруты
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
