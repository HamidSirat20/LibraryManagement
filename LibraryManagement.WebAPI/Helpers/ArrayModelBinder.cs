﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;
using System.Reflection;

namespace LibraryManagement.WebAPI.Helpers
{
    public class ArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            var value = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName).ToString();

            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            var elementType = bindingContext.ModelType.GetTypeInfo()
                .GenericTypeArguments[0];
            var converter = TypeDescriptor.GetConverter(elementType);

            var values = value?
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(x =>
                        {
                            try
                            {
                                return converter.ConvertFromString(x);
                            }
                            catch (FormatException)
                            {
                                bindingContext.ModelState.AddModelError(
                                    bindingContext.ModelName,
                                    $"Value '{x}' is not a valid {elementType.Name}");
                                return null;
                            }
                        })
                        .Where(x => x != null)
                        .ToArray() ?? Array.Empty<object>();

            var typedValues = Array.CreateInstance(elementType, values.Length);
            values.CopyTo(typedValues, 0);
            bindingContext.Model = typedValues;

            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;
        }
    }
}
