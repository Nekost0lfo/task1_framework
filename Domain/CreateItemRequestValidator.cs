namespace Pr1.MinWebService.Domain;

/// <summary>
/// Правила валидации запроса на создание элемента реестра (предметная область).
/// </summary>
public static class CreateItemRequestValidator
{
    /// <summary>
    /// Проверяет запрос. Возвращает null при успехе, иначе сообщение об ошибке.
    /// </summary>
    public static string? Validate(CreateItemRequest request)
    {
        if (request is null)
            return "Запрос не задан";

        if (string.IsNullOrWhiteSpace(request.Name))
            return "Поле name не должно быть пустым";

        if (request.Year < 0)
            return "Поле year не может быть отрицательным";

        return null;
    }
}
