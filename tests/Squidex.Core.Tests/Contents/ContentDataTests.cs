﻿// ==========================================================================
//  ContentDataTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Core.Contents
{
    public class ContentDataTests
    {
        private readonly Schema schema =
            Schema.Create("schema", new SchemaProperties())
                .AddOrUpdateField(new NumberField(1, "field1", Partitioning.Language))
                .AddOrUpdateField(new NumberField(2, "field2", Partitioning.Invariant))
                .AddOrUpdateField(new NumberField(3, "field3", Partitioning.Invariant).Hide())
                .AddOrUpdateField(new JsonField(4, "json", Partitioning.Language));
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Create(Language.EN, Language.DE);

        [Fact]
        public void Should_convert_to_id_model()
        {
            var input =
               new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 3))
                    .AddField("invalid",
                        new ContentFieldData()
                            .AddValue("iv", 3));

            var actual = input.ToIdModel(schema, false);

            var expected =
                new ContentData()
                    .AddField("1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"))
                    .AddField("2",
                        new ContentFieldData()
                            .AddValue("iv", 3));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_convert_to_encoded_id_model()
        {
            var input =
               new ContentData()
                    .AddField("json",
                        new ContentFieldData()
                            .AddValue("en", new JObject())
                            .AddValue("de", null)
                            .AddValue("it", JValue.CreateNull()));

            var actual = input.ToIdModel(schema, true);

            var expected =
                new ContentData()
                    .AddField("4",
                        new ContentFieldData()
                            .AddValue("en", "e30=")
                            .AddValue("de", null)
                            .AddValue("it", null));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_convert_from_id_model()
        {
            var input =
               new ContentData()
                    .AddField("1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"))
                    .AddField("2",
                        new ContentFieldData()
                            .AddValue("iv", 3))
                    .AddField("99",
                        new ContentFieldData()
                            .AddValue("iv", 3));

            var actual = input.ToNameModel(schema, false);

            var expected =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 3));
            
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_convert_from_encoded_id_model()
        {
            var input =
               new ContentData()
                    .AddField("4",
                        new ContentFieldData()
                            .AddValue("en", "e30=")
                            .AddValue("de", null)
                            .AddValue("it", null));

            var actual = input.ToNameModel(schema, true);

            Assert.True(actual["json"]["en"] is JObject);
        }

        [Fact]
        public void Should_cleanup_old_fields()
        {
            var expected =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"));

            var input =
                new ContentData()
                    .AddField("field0",
                        new ContentFieldData()
                            .AddValue("en", "en_string"))
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"));

            var actual = input.ToApiModel(schema, languagesConfig);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_cleanup_old_languages()
        {
            var expected =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string"));

            var input =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", "en_string")
                            .AddValue("de", "de_string")
                            .AddValue("it", "it_string"));

            var actual = input.ToApiModel(schema, languagesConfig);

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void Should_provide_invariant_from_master_language()
        {
            var expected =
                new ContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 3));

            var input =
                new ContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("de", 2)
                            .AddValue("en", 3));

            var actual = input.ToApiModel(schema, languagesConfig);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_provide_master_language_from_invariant()
        {
            var expected =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", 3));

            var input =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("iv", 3));

            var actual = input.ToApiModel(schema, languagesConfig);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_remove_null_values_when_cleaning()
        {
            var expected =
                new ContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("en", 2));

            var input =
                new ContentData()
                    .AddField("field1", null)
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("en", 2)
                            .AddValue("it", null));

            var actual = input.ToCleaned();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_provide_invariant_from_first_language()
        {
            var expected =
                new ContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 2));

            var input =
                new ContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("de", 2)
                            .AddValue("it", 3));

            var actual = input.ToApiModel(schema, languagesConfig);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_not_include_hidden_field()
        {
            var expected =
                new ContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 5));

            var input =
                new ContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 5))
                    .AddField("field3",
                        new ContentFieldData()
                            .AddValue("iv", 2));

            var actual = input.ToApiModel(schema, languagesConfig);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_return_original_when_no_language_preferences_defined()
        {
            var data =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("iv", 1));

            Assert.Same(data, data.ToLanguageModel(languagesConfig));
        }

        [Fact]
        public void Should_return_flat_list_when_single_languages_specified()
        {
            var data =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("de", 1)
                            .AddValue("en", 2))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("de", null)
                            .AddValue("en", 4))
                    .AddField("field3",
                        new ContentFieldData()
                            .AddValue("en", 6))
                    .AddField("field4",
                        new ContentFieldData()
                            .AddValue("it", 7));

            var fallbackConfig =
                LanguagesConfig.Create(Language.DE).Add(Language.EN)
                    .Update(Language.DE, false, false, new[] { Language.EN });

            var output = (Dictionary<string, JToken>)data.ToLanguageModel(fallbackConfig, new List<Language> { Language.DE });

            var expected = new Dictionary<string, JToken>
            {
                { "field1", 1 },
                { "field2", 4 },
                { "field3", 6 }
            };

            Assert.True(expected.EqualsDictionary(output));
        }

        [Fact]
        public void Should_return_flat_list_when_languages_specified()
        {
            var data =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("de", 1)
                            .AddValue("en", 2))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("de", null)
                            .AddValue("en", 4))
                    .AddField("field3",
                        new ContentFieldData()
                            .AddValue("en", 6))
                    .AddField("field4",
                        new ContentFieldData()
                            .AddValue("it", 7));

            var output = (Dictionary<string, JToken>)data.ToLanguageModel(languagesConfig, new List<Language> { Language.DE, Language.EN });

            var expected = new Dictionary<string, JToken>
            {
                { "field1", 1 },
                { "field2", 4 },
                { "field3", 6 }
            };

            Assert.True(expected.EqualsDictionary(output));
        }

        [Fact]
        public void Should_merge_two_data()
        {
            var lhs =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("iv", 1))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("de", 2));

            var rhs =
                new ContentData()
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("en", 3))
                    .AddField("field3",
                        new ContentFieldData()
                            .AddValue("iv", 4));

            var expected =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("iv", 1))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("de", 2)
                            .AddValue("en", 3))
                    .AddField("field3",
                        new ContentFieldData()
                            .AddValue("iv", 4));

            var actual = lhs.MergeInto(rhs);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_be_equal_when_data_have_same_structure()
        {
            var lhs =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("iv", 2))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 2));

            var rhs =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("iv", 2))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 2));

            Assert.True(lhs.Equals(rhs));
            Assert.True(lhs.Equals((object)rhs));
            Assert.Equal(lhs.GetHashCode(), rhs.GetHashCode());
        }

        [Fact]
        public void Should_not_be_equal_when_data_have_not_same_structure()
        {
            var lhs =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("iv", 2))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddValue("iv", 2));

            var rhs =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddValue("en", 2))
                    .AddField("field3",
                        new ContentFieldData()
                            .AddValue("iv", 2));

            Assert.False(lhs.Equals(rhs));
            Assert.False(lhs.Equals((object)rhs));
            Assert.NotEqual(lhs.GetHashCode(), rhs.GetHashCode());
        }
    }
}
