﻿// ==========================================================================
//  RequiredValidatorTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Squidex.Core.Schemas.Validators
{
    public sealed class RequiredStringValidatorTests
    {
        private readonly List<string> errors = new List<string>();

        [Theory]
        [InlineData("MyString")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("  ")]
        public async Task Should_not_add_error_if_value_is_valid(string value)
        {
            var sut = new RequiredStringValidator();

            await sut.ValidateAsync(value, false, errors.Add);

            Assert.Equal(0, errors.Count);
        }

        [Fact]
        public async Task Should_not_add_error_if_optional()
        {
            var sut = new RequiredStringValidator();

            await sut.ValidateAsync(string.Empty, true, errors.Add);

            Assert.Equal(0, errors.Count);
        }

        [Fact]
        public async Task Should_not_add_error_if_value_is_another_type()
        {
            var sut = new RequiredStringValidator();

            await sut.ValidateAsync(true, false, errors.Add);

            Assert.Equal(0, errors.Count);
        }

        [Fact]
        public async Task Should_add_error_if_empty_strings_are_not_allowed()
        {
            var sut = new RequiredStringValidator(true);

            await sut.ValidateAsync(string.Empty, false, errors.Add);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required" });
        }

        [Fact]
        public async Task Should_add_error_if_value_is_null()
        {
            var sut = new RequiredStringValidator();

            await sut.ValidateAsync(null, false, errors.Add);

            errors.ShouldBeEquivalentTo(
                new[] { "<FIELD> is required" });
        }
    }
}
