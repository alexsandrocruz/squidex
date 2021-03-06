﻿// ==========================================================================
//  AppLanguageRemoved.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Events.Apps
{
    [TypeName("AppLanguageRemovedEvent")]
    public sealed class AppLanguageRemoved : AppEvent
    {
        public Language Language { get; set; }
    }
}
