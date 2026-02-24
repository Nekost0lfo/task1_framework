using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;
using Pr1.MinWebService.Domain;
using Pr1.MinWebService.Errors;
using Pr1.MinWebService.Middlewares;
using Pr1.MinWebService.Services;

var builder = WebApplication.CreateBuilder(args);

// Настройка сериализации, чтобы ответы были компактнее
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddSingleton<IItemRepository, InMemoryItemRepository>();

var app = builder.Build();

// Конвейер из трёх обработчиков:
// 1) RequestId — обеспечивает идентификатор запроса (для логов и ответов об ошибках)
// 2) TimingAndLog — измеряет время выполнения и пишет в журнал информацию о запросе и ответе
// 3) ErrorHandling — превращает исключения в согласованный JSON-ответ с кодом, сообщением и requestId
app.UseMiddleware<RequestIdMiddleware>();
app.UseMiddleware<TimingAndLogMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

// GET /api/items — список элементов реестра (игровых дисков)
app.MapGet("/api/items", (IItemRepository repo) =>
{
    return Results.Ok(repo.GetAll());
});

// GET /api/items/{id} — один элемент по id; при отсутствии — согласованный ответ об ошибке (404)
app.MapGet("/api/items/{id:guid}", (Guid id, IItemRepository repo) =>
{
    var item = repo.GetById(id);
    if (item is null)
        throw new NotFoundException("Элемент не найден");

    return Results.Ok(item);
});

// POST /api/items — создание элемента; при некорректных данных — согласованный ответ об ошибке (400)
app.MapPost("/api/items", (HttpContext ctx, CreateItemRequest request, IItemRepository repo) =>
{
    var validationError = CreateItemRequestValidator.Validate(request);
    if (validationError is not null)
        throw new ValidationException(validationError);

    var created = repo.Create(request.Name!.Trim(), request.Year);

    // Адрес созданного ресурса без привязки к конкретному хосту
    var location = $"/api/items/{created.Id}";
    ctx.Response.Headers.Location = location;

    return Results.Created(location, created);
});

app.Run();

// Нужен для проекта с испытаниями
public partial class Program { }
