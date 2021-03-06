﻿// ==========================================================================
//  FieldExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Core
{
    public static class FieldExtensions
    {
        public static void AddError(this ICollection<ValidationError> errors, string message, Field field, IFieldPartitionItem partitionItem = null)
        {
            AddError(errors, message, !string.IsNullOrWhiteSpace(field.RawProperties.Label) ? field.RawProperties.Label : field.Name, field.Name, partitionItem);
        }

        public static void AddError(this ICollection<ValidationError> errors, string message, string fieldName, IFieldPartitionItem partitionItem = null)
        {
            AddError(errors, message, fieldName, fieldName, partitionItem);
        }

        public static void AddError(this ICollection<ValidationError> errors, string message, string displayName, string fieldName, IFieldPartitionItem partitionItem = null)
        {
            if (partitionItem != null && partitionItem != InvariantPartitioning.Instance.Master)
            {
                displayName += $" ({partitionItem.Key})";
            }

            errors.Add(new ValidationError(message.Replace("<FIELD>", displayName), fieldName));
        }

        public static async Task ValidateAsync(this Field field, JToken value, bool isOptional, Action<string> addError)
        {
            Guard.NotNull(value, nameof(value));

            try
            {
                var typedValue = value.IsNull() ? null : field.ConvertValue(value);

                foreach (var validator in field.Validators)
                {
                    await validator.ValidateAsync(typedValue, isOptional, addError);
                }
            }
            catch
            {
                addError("<FIELD> is not a valid value");
            }
        }
    }
}
