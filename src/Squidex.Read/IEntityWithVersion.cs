﻿// ==========================================================================
//  IEntityWithVersion.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Read
{
    public interface IEntityWithVersion
    {
        long Version { get; set; }
    }
}
