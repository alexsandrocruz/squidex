﻿// ==========================================================================
//  AssetsValue.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Squidex.Core.Schemas
{
    public sealed class AssetsValue
    {
        private readonly List<Guid> EmptyAssetIds = new List<Guid>();

        public IReadOnlyList<Guid> AssetIds { get; }

        public AssetsValue(IReadOnlyList<Guid> assetIds)
        {
            AssetIds = assetIds ?? EmptyAssetIds;
        }
    }
}
