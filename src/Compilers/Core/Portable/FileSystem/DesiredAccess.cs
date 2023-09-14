// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
namespace Roslyn.Utilities;

internal enum DesiredAccess : uint
{
    GENERIC_WRITE = 0x40000000,
    GENERIC_READ = 0x80000000,
}
