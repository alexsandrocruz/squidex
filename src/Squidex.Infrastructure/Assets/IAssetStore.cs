﻿// ==========================================================================
//  IAssetStorage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Assets
{
    public interface IAssetStore
    {
        Task DownloadAsync(Guid id, long version, string suffix, Stream stream);

        Task UploadAsync(Guid id, long version, string suffix, Stream stream);
    }
}