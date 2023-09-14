// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
namespace Roslyn.Utilities;

/// <summary>
/// A slimmed down version of the FileAccessData data structure defined in MSBuild.
/// Eliminates data that are constant when reporting file accesses (e.g., whether
/// the file access is "augmented").
/// </summary>
internal readonly struct FileAccessDataSlim
{
    internal FileAccessDataSlim(RequestedAccess requestedAccess, uint processId, DesiredAccess desiredAccess, string path)
    {
        RequestedAccess = requestedAccess;
        ProcessId = processId;
        DesiredAccess = desiredAccess;
        Path = path;
    }

    internal RequestedAccess RequestedAccess { get; }

    internal uint ProcessId { get; }

    internal DesiredAccess DesiredAccess { get; }

    internal string Path { get; }
};
