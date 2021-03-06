﻿// ==========================================================================
//  IAppFeature.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Read.Apps;

namespace Squidex.Pipeline
{
    public interface IAppFeature
    {
        IAppEntity App { get; }
    }
}
