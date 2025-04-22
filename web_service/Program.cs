/*
 * Главный файл настройки и запуска ASP.NET Core приложения.
 * Ответственности:
 * 1. Конфигурация сервисов приложения
 * 2. Настройка pipeline обработки запросов
 * 3. Инициализация системных компонентов
 * 4. Управление жизненным циклом приложения
 */
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Identity;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args); // Создание билдера приложения

/***************************
 * РАЗДЕЛ 1: КОНФИГУРАЦИЯ СЕРВИСОВ
 ***************************/

// 1.1 Подключение к базе данных
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

// Настройка контекста БД с использованием PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)); // Использование провайдера Npgsql для PostgreSQL

// 1.2 Настройка системы Identity
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true; // Требование подтверждения email
        // Дополнительные настройки (пример):
        options.Password.RequiredLength = 8;                  // Минимум 8 символов
        options.Password.RequireDigit = true;                 // Обязательная цифра
        options.Password.RequireLowercase = true;             // Строчная буква
        options.Password.RequireUppercase = true;             // Заглавная буква
    })
    .AddRoles<IdentityRole>() // Включение системы ролей
    .AddEntityFrameworkStores<ApplicationDbContext>(); // Хранение данных в настроенной БД

// 1.3 Настройка MVC и Razor Pages
builder.Services.AddControllersWithViews(); // Регистрация MVC-контроллеров
builder.Services.AddRazorPages(options =>
{
    // Автоматическая защита всех страниц в папке Manage
    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
    // Пример: Требовать роль Admin для всей области
    // options.Conventions.AuthorizeAreaFolder("Admin", "/", "Administrator");
});

// 1.4 Регистрация кастомных сервисов
builder.Services.AddAutoMapper(typeof(DomainToEntityProfile).Assembly); // AutoMapper
builder.Services.AddScoped<ICarService, CarService>(); // Сервис работы с автомобилями

/***************************
 * РАЗДЕЛ 2: ИНИЦИАЛИЗАЦИЯ ПРИЛОЖЕНИЯ
 ***************************/
var app = builder.Build(); // Сборка приложения

// 2.1 Инициализация системных ролей
using (var scope = app.Services.CreateScope()) // Создание временного scope
{
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Client", "Storekeeper", "Administrator" }; // Основные роли системы

    foreach (var role in roles)
    {
        if (!await roleMgr.RoleExistsAsync(role)) // Проверка существования роли
        {
            await roleMgr.CreateAsync(new IdentityRole(role)); // Создание отсутствующей роли
            Console.WriteLine($"Created role: {role}");
        }
    }
}

/***************************
 * РАЗДЕЛ 3: НАСТРОЙКА PIPELINE
 ***************************/

// 3.1 Обработка ошибок
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); // Страница ошибок миграций
    app.UseDeveloperExceptionPage(); // Детальные ошибки для разработки
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Пользовательская страница ошибок
    app.UseHsts(); // HTTP Strict Transport Security
}

// 3.2 Безопасность и статические файлы
app.UseHttpsRedirection(); // Перенаправление HTTP -> HTTPS
app.UseStaticFiles(); // Обслуживание статических файлов (CSS, JS, изображения)

// 3.3 Маршрутизация и авторизация
app.UseRouting(); // Активация системы маршрутизации

// Важно соблюдать порядок:
app.UseAuthentication(); // Проверка аутентификации
app.UseAuthorization();  // Проверка прав доступа

// 3.4 Настройка конечных точек
app.MapControllerRoute( // Маршрутизация для MVC
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // /Home/Index/5

app.MapRazorPages(); // Маршрутизация для Razor Pages

/***************************
 * РАЗДЕЛ 4: ЗАПУСК ПРИЛОЖЕНИЯ
 ***************************/
app.Run(); // Запуск веб-сервера и начало обработки запросов