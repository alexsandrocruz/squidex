﻿// ==========================================================================
//  RemoveContributor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Write.Apps.Commands
{
    public class RemoveContributor : AppAggregateCommand, IValidatable
    {
        public string ContributorId { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(ContributorId))
            {
                errors.Add(new ValidationError("Contributor id not assigned", nameof(ContributorId)));
            }
        }
    }
}