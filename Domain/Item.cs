namespace Pr1.MinWebService.Domain;

/// <summary>
/// Элемент реестра — игровой диск (название игры и год выпуска).
/// </summary>
public sealed record Item(Guid Id, string Name, int Year);
