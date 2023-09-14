// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
#if NETSTANDARD2_0
using System.Diagnostics;
#endif
using System.IO;

namespace Roslyn.Utilities
{
    /// <summary>
    /// Abstraction over the file system that is useful for test hooks
    /// </summary>
    internal interface ICommonCompilerFileSystem
    {
        bool FileExists(string filePath);

        Stream OpenFile(string filePath, FileMode mode, FileAccess access, FileShare share, List<FileAccessDataSlim>? fileAccessData = null);

        Stream OpenFileEx(
            string filePath,
            FileMode mode,
            FileAccess access,
            FileShare share,
            int bufferSize,
            FileOptions options,
            out string normalizedFilePath,
            List<FileAccessDataSlim>? fileAccessData = null);
    }

    internal static class CommonCompilerFileSystemExtensions
    {
        /// <summary>
        /// Open a file and ensure common exception types are wrapped to <see cref="IOException"/>.
        /// </summary>
        internal static Stream OpenFileWithNormalizedException(
            this ICommonCompilerFileSystem fileSystem,
            string filePath,
            FileMode fileMode,
            FileAccess fileAccess,
            FileShare fileShare,
            List<FileAccessDataSlim>? fileAccessData = null)
        {
            try
            {
                return fileSystem.OpenFile(filePath, fileMode, fileAccess, fileShare, fileAccessData);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (DirectoryNotFoundException e)
            {
                throw new FileNotFoundException(e.Message, filePath, e);
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new IOException(e.Message, e);
            }
        }
    }

    internal sealed class StandardFileSystem : ICommonCompilerFileSystem
    {
        private static readonly uint s_processId = (uint)
#if NETSTANDARD2_0
            Process.GetCurrentProcess().Id;
#else
            // More performant than Process.GetCurrentProcess().Id.
            Environment.ProcessId;    
#endif

        private static readonly IReadOnlyDictionary<FileAccess, (RequestedAccess, DesiredAccess)> s_fileAccessTranslator = new Dictionary<FileAccess, (RequestedAccess, DesiredAccess)>()
        {
            {
                FileAccess.Read, (RequestedAccess.Read, DesiredAccess.GENERIC_READ)
            },
            {
                FileAccess.Write, (RequestedAccess.Write, DesiredAccess.GENERIC_WRITE)
            },
            {
                // Set the DesiredAccess to GENERIC_WRITE since BuildXL (whence these enums come)
                // does not define a specific Read/Write DesiredAccess.
                FileAccess.ReadWrite, (RequestedAccess.ReadWrite, DesiredAccess.GENERIC_WRITE)
            },
        };

        public static StandardFileSystem Instance { get; } = new StandardFileSystem();

        private StandardFileSystem()
        {
        }

        public bool FileExists(string filePath) => File.Exists(filePath);

        public Stream OpenFile(string filePath, FileMode mode, FileAccess access, FileShare share, List<FileAccessDataSlim>? fileAccessData = null)
        {
            if (fileAccessData is not null)
            {
                CollectFileAccessData(access, filePath, fileAccessData);
            }

            return new FileStream(filePath, mode, access, share);
        }

        public Stream OpenFileEx(
            string filePath,
            FileMode mode,
            FileAccess access,
            FileShare share,
            int bufferSize,
            FileOptions options,
            out string normalizedFilePath,
            List<FileAccessDataSlim>? fileAccessData = null)
        {
            var fileStream = new FileStream(filePath, mode, access, share, bufferSize, options);
            normalizedFilePath = fileStream.Name;
            if (fileAccessData is not null)
            {
                CollectFileAccessData(access, normalizedFilePath, fileAccessData);
            }

            return fileStream;
        }

        private static void CollectFileAccessData(FileAccess fileAccess, string absoluteFilePath, List<FileAccessDataSlim> fileAccessData)
        {
            (RequestedAccess requestedAccess, DesiredAccess desiredAccess) = s_fileAccessTranslator[fileAccess];
            fileAccessData.Add(new FileAccessDataSlim(
               requestedAccess,
               s_processId,
               desiredAccess,
               absoluteFilePath));
        }
    }
}
