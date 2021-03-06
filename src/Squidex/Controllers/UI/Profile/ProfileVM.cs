﻿// ==========================================================================
//  ProfileVM.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Controllers.UI.Profile
{
    public sealed class ProfileVM
    {
        public string Email { get; set; }

        public string DisplayName { get; set; }

        public string PictureUrl { get; set; }

        public bool HasPassword { get; set; }

        public bool HasPasswordAuth { get; set; }
    }
}
