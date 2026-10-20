using System.ComponentModel.DataAnnotations;

namespace TrainingCourseApi.Endpoints;

/// <summary>
/// Ручная проверка DataAnnotations для minimal API.
/// Возвращает null, если модель валидна, иначе — словарь ошибок.
/// </summary>
internal static class ValidationHelper
{
    public static Dictionary<string, string[]>? Validate(object model)
    {
        var results = new List<ValidationResult>();

        if (Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true))
        {
            return null;
        }

        return results
            .SelectMany(r => r.MemberNames.DefaultIfEmpty(string.Empty)
                .Select(member => (Member: member, Message: r.ErrorMessage ?? "Некорректное значение.")))
            .GroupBy(x => x.Member, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Message).ToArray(), StringComparer.Ordinal);
    }
}