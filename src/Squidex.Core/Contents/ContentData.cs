﻿// ==========================================================================
//  ContentData.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;

// ReSharper disable InvertIf

namespace Squidex.Core.Contents
{
    public sealed class ContentData : Dictionary<string, ContentFieldData>, IEquatable<ContentData>
    {
        public ContentData()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public ContentData(IDictionary<string, ContentFieldData> copy)
            : base(copy, StringComparer.OrdinalIgnoreCase)
        {
        }

        public ContentData AddField(string fieldName, ContentFieldData data)
        {
            Guard.ValidPropertyName(fieldName, nameof(fieldName));

            this[fieldName] = data;

            return this;
        }

        public ContentData MergeInto(ContentData other)
        {
            Guard.NotNull(other, nameof(other));

            var result = new ContentData(this);

            if (ReferenceEquals(other, this))
            {
                return result;
            }

            foreach (var otherValue in other)
            {
                var fieldValue = result.GetOrAdd(otherValue.Key, x => new ContentFieldData());

                foreach (var value in otherValue.Value)
                {
                    fieldValue[value.Key] = value.Value;
                }
            }

            return result;
        }

        public ContentData ToCleaned()
        {
            var result = new ContentData();

            foreach (var fieldValue in this.Where(x => x.Value != null))
            {
                var resultValue = new ContentFieldData();

                foreach (var partitionValue in fieldValue.Value.Where(x => x.Value != null && x.Value.Type != JTokenType.Null))
                {
                    resultValue[partitionValue.Key] = partitionValue.Value;
                }

                if (resultValue.Count > 0)
                {
                    result[fieldValue.Key] = resultValue;
                }
            }

            return result;
        }

        public ContentData ToIdModel(Schema schema, bool encodeJsonField)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new ContentData();

            foreach (var fieldValue in this)
            {
                if (!schema.FieldsByName.TryGetValue(fieldValue.Key, out Field field))
                {
                    continue;
                }

                var fieldId = field.Id.ToString();

                if (encodeJsonField && field is JsonField)
                {
                    var encodedValue = new ContentFieldData();

                    foreach (var partitionValue in fieldValue.Value)
                    {
                        if (partitionValue.Value == null || partitionValue.Value.Type == JTokenType.Null)
                        {
                            encodedValue[partitionValue.Key] = null;
                        }
                        else
                        {
                            var value = Convert.ToBase64String(Encoding.UTF8.GetBytes(partitionValue.Value.ToString()));

                            encodedValue[partitionValue.Key] = value;
                        }
                    }

                    result[fieldId] = encodedValue;
                }
                else
                {
                    result[fieldId] = fieldValue.Value;
                }
            }

            return result;
        }

        public ContentData ToNameModel(Schema schema, bool decodeJsonField)
        {
            Guard.NotNull(schema, nameof(schema));

            var result = new ContentData();

            foreach (var fieldValue in this)
            {
                if (!long.TryParse(fieldValue.Key, out long fieldId) || !schema.FieldsById.TryGetValue(fieldId, out Field field))
                {
                    continue;
                }

                if (decodeJsonField && field is JsonField)
                {
                    var encodedValue = new ContentFieldData();
                    
                    foreach (var partitionValue in fieldValue.Value)
                    {
                        if (partitionValue.Value == null || partitionValue.Value.Type == JTokenType.Null)
                        {
                            encodedValue[partitionValue.Key] = null;
                        }
                        else
                        {
                            var value = Encoding.UTF8.GetString(Convert.FromBase64String(partitionValue.Value.ToString()));

                            encodedValue[partitionValue.Key] = JToken.Parse(value);
                        }
                    }

                    result[field.Name] = encodedValue;
                }
                else
                {

                    result[field.Name] = fieldValue.Value;
                }
            }

            return result;
        }

        public ContentData ToApiModel(Schema schema, LanguagesConfig languagesConfig, IReadOnlyCollection<Language> languagePreferences = null, bool excludeHidden = true)
        {
            Guard.NotNull(schema, nameof(schema));
            Guard.NotNull(languagesConfig, nameof(languagesConfig));

            var codeForInvariant = InvariantPartitioning.Instance.Master.Key;
            var codeForMasterLanguage = languagesConfig.Master.Language.Iso2Code;

            var result = new ContentData();

            foreach (var fieldValue in this)
            {
                if (!schema.FieldsByName.TryGetValue(fieldValue.Key, out Field field) || (excludeHidden && field.IsHidden))
                {
                    continue;
                }

                var fieldResult = new ContentFieldData();
                var fieldValues = fieldValue.Value;

                if (field.Paritioning.Equals(Partitioning.Language))
                {
                    foreach (var languageConfig in languagesConfig)
                    {
                        var languageCode = languageConfig.Key;

                        if (fieldValues.TryGetValue(languageCode, out JToken value))
                        {
                            fieldResult.Add(languageCode, value);
                        }
                        else if (languageConfig == languagesConfig.Master && fieldValues.TryGetValue(codeForInvariant, out value))
                        {
                            fieldResult.Add(languageCode, value);
                        }
                    }
                }
                else
                {
                    if (fieldValues.TryGetValue(codeForInvariant, out JToken value))
                    {
                        fieldResult.Add(codeForInvariant, value);
                    }
                    else if (fieldValues.TryGetValue(codeForMasterLanguage, out value))
                    {
                        fieldResult.Add(codeForInvariant, value);
                    }
                    else if (fieldValues.Count > 0)
                    {
                        fieldResult.Add(codeForInvariant, fieldValues.Values.First());
                    }
                }

                result.Add(field.Name, fieldResult);
            }

            return result;
        }

        public object ToLanguageModel(LanguagesConfig languagesConfig, IReadOnlyCollection<Language> languagePreferences = null)
        {
            Guard.NotNull(languagesConfig, nameof(languagesConfig));

            if (languagePreferences == null || languagePreferences.Count == 0)
            {
                return this;
            }

            if (languagePreferences.Count == 1 && languagesConfig.TryGetConfig(languagePreferences.First(), out var languageConfig))
            {
                languagePreferences = languagePreferences.Union(languageConfig.Fallback).ToList();
            }

            var result = new Dictionary<string, JToken>();

            foreach (var fieldValue in this)
            {
                var fieldValues = fieldValue.Value;

                foreach (var language in languagePreferences)
                {
                    if (fieldValues.TryGetValue(language, out JToken value) && value != null)
                    {
                        result[fieldValue.Key] = value;

                        break;
                    }
                }
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ContentData);
        }

        public bool Equals(ContentData other)
        {
            return other != null && (ReferenceEquals(this, other) || this.EqualsDictionary(other));
        }

        public override int GetHashCode()
        {
            return this.DictionaryHashCode();
        }
    }
}
