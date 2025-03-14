using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using BLL.Abstractions.Utilities;
using Microsoft.Extensions.Logging;

namespace BLL.Utilities;

internal class FilterUtility : IFilterUtility
{
    private readonly ILogger<FilterUtility> _logger;

    private readonly Regex EscapeTagRegex = new(
        @"<(?<tag>\w+)[^>]*>.*?</\k<tag>>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline
    );


    public FilterUtility(ILogger<FilterUtility> logger)
    {
        _logger = logger;
    }

    public async Task<TEntity> Filter<TEntity>(TEntity entity) where TEntity : class, new()
    {
        if (entity == null)
        {
            _logger.LogError($"Null entity passed to Filter method. Entity type: {typeof(TEntity).Name}.");
            throw new ArgumentNullException(nameof(entity));
        }

        var filteredEntity = new TEntity();

        var properties = typeof(TEntity)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(string) && p.CanRead && p.CanWrite);

        foreach (var property in properties)
        {
            var value = (string)property.GetValue(entity);

            if (value != null)
            {
                value = EscapeScriptTags(value);
                property.SetValue(filteredEntity, value);
            }
            else
            {
                property.SetValue(filteredEntity, null);
            }

            var minLengthAttr = property.GetCustomAttribute<MinLengthAttribute>();
            if (minLengthAttr != null && (string.IsNullOrWhiteSpace(value) || value.Length < minLengthAttr.Length))
            {
                _logger.LogError($"Validation failed for {property.Name}. Problem entity: {nameof(entity)}.");
                throw new Exception($"{property.Name} is required to be filled.");
            }
        }

        var nonStringProperties = typeof(TEntity)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType != typeof(string) && p.CanRead && p.CanWrite);

        foreach (var property in nonStringProperties)
        {
            var value = property.GetValue(entity);
            property.SetValue(filteredEntity, value);
        }

        return filteredEntity;
    }

    public async Task<List<TEntity>> FilterCollection<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class, new()
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        var filteredEntities = new List<TEntity>();

        foreach (var entity in entities)
        {
            var filteredEntity = await Filter(entity);
            filteredEntities.Add(filteredEntity);
        }

        return filteredEntities;
    }

    private string EscapeScriptTags(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return EscapeTagRegex.Replace(input, string.Empty);
    }
}