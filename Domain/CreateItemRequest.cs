namespace Pr1.MinWebService.Domain;

/// <summary>
/// Тело запроса на создание записи в реестре игровых дисков.
/// </summary>
public sealed record CreateItemRequest(string Name, int Year);
