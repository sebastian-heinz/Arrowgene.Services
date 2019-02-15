/*
 * MIT License
 * 
 * Copyright (c) 2018 Sebastian Heinz <sebastian.heinz.gt@googlemail.com>
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Arrowgene.Services.Extra
{
    public class Utils
    {
        public static readonly CryptoRandom Random = new CryptoRandom();
        private static readonly Random RandomNum = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            lock (RandomNum)
            {
                return RandomNum.Next(min, max);
            }
        }

        public static long GetUnixTime(DateTime dateTime)
        {
            return ((DateTimeOffset) dateTime).ToUnixTimeSeconds();
        }

        public static string PathDifference(string directoryInfo1, string directoryInfo2, bool unRoot)
        {
            return PathDifference(new DirectoryInfo(directoryInfo1), new DirectoryInfo(directoryInfo2), unRoot);
        }

        public static string PathDifference(FileSystemInfo directoryInfo1, FileSystemInfo directoryInfo2, bool unRoot)
        {
            string result;
            if (directoryInfo1.FullName == directoryInfo2.FullName)
            {
                result = "";
            }
            else if (directoryInfo1.FullName.StartsWith(directoryInfo2.FullName))
            {
                result = directoryInfo1.FullName.Split(new[] {directoryInfo2.FullName},
                    StringSplitOptions.RemoveEmptyEntries)[0];
            }
            else if (directoryInfo2.FullName.StartsWith(directoryInfo1.FullName))
            {
                result = directoryInfo2.FullName.Split(new[] {directoryInfo1.FullName},
                    StringSplitOptions.RemoveEmptyEntries)[0];
            }
            else
            {
                result = "";
            }

            if (unRoot)
            {
                result = UnrootPath(result);
            }

            return result;
        }

        public static string UnrootPath(string path)
        {
            // https://stackoverflow.com/questions/53102/why-does-path-combine-not-properly-concatenate-filenames-that-start-with-path-di
            if (Path.IsPathRooted(path))
            {
                path = path.TrimStart(Path.DirectorySeparatorChar);
                path = path.TrimStart(Path.AltDirectorySeparatorChar);
            }

            return path;
        }

        public static AssemblyName GetAssemblyName(string name)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                AssemblyName assemblyName = assembly.GetName();
                if (assemblyName.Name == name)
                {
                    return assemblyName;
                }
            }

            return null;
        }

        public static Version GetAssemblyVersion(string name)
        {
            AssemblyName assemblyName = GetAssemblyName(name);
            if (assemblyName != null)
            {
                return assemblyName.Version;
            }

            return null;
        }

        public static string GetAssemblyVersionString(string name)
        {
            Version version = GetAssemblyVersion(name);
            if (version != null)
            {
                return version.ToString();
            }

            return null;
        }

        public static byte[] ReadFile(string source)
        {
            if (!File.Exists(source))
            {
                throw new Exception($"'{source}' does not exist or is not a file");
            }

            return File.ReadAllBytes(source);
        }

        public static string ReadFileText(string source)
        {
            if (!File.Exists(source))
            {
                throw new Exception($"'{source}' does not exist or is not a file");
            }

            return File.ReadAllText(source);
        }

        public static void WriteFile(byte[] content, string destination)
        {
            if (content != null)
            {
                File.WriteAllBytes(destination, content);
            }
            else
            {
                throw new Exception($"Content of '{destination}' is null");
            }
        }

        public static List<FileInfo> GetFiles(DirectoryInfo directoryInfo, string[] extensions, bool recursive)
        {
            if (recursive)
            {
                List<FileInfo> filteredFiles = GetFiles(directoryInfo, extensions);
                DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
                foreach (DirectoryInfo dInfo in directoryInfos)
                {
                    List<FileInfo> files = GetFiles(dInfo, extensions, true);
                    filteredFiles.AddRange(files);
                }

                return filteredFiles;
            }

            return GetFiles(directoryInfo, extensions);
        }

        public static List<FileInfo> GetFiles(DirectoryInfo directoryInfo, string[] extensions)
        {
            List<FileInfo> filteredFiles = new List<FileInfo>();
            FileInfo[] files = directoryInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            foreach (FileInfo file in files)
            {
                if (extensions != null)
                {
                    foreach (string extension in extensions)
                    {
                        if (file.Extension.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                        {
                            filteredFiles.Add(file);
                            break;
                        }
                    }
                }
                else
                {
                    filteredFiles.Add(file);
                }
            }

            return filteredFiles;
        }
        
        public static List<DirectoryInfo> GetFolders(DirectoryInfo directoryInfo, string[] extensions, bool recursive)
        {
            if (recursive)
            {
                List<DirectoryInfo> result = new List<DirectoryInfo>();
                List<DirectoryInfo> filteredDirectories = GetFolders(directoryInfo, extensions);
                result.AddRange(filteredDirectories);
                foreach (DirectoryInfo directory in filteredDirectories)
                {
                    List<DirectoryInfo> directories = GetFolders(directory, extensions, true);
                    result.AddRange(directories);
                }

                return result;
            }

            return GetFolders(directoryInfo, extensions);
        }

        public static List<DirectoryInfo> GetFolders(DirectoryInfo directoryInfo, string[] extensions)
        {
            List<DirectoryInfo> filteredDirectories = new List<DirectoryInfo>();
            DirectoryInfo[] directories = directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
            foreach (DirectoryInfo directory in directories)
            {
                if (extensions != null)
                {
                    foreach (string extension in extensions)
                    {
                        if (directory.Name.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                        {
                            filteredDirectories.Add(directory);
                            break;
                        }
                    }
                }
                else
                {
                    filteredDirectories.Add(directory);
                }
            }

            return filteredDirectories;
        }


        public static DirectoryInfo EnsureDirectory(string directory)
        {
            return Directory.CreateDirectory(directory);
        }

        /// <summary>
        /// The directory of the executing assembly.
        /// This might not be the location where the .dll files are located.
        /// </summary>
        /// <returns></returns>
        public static string ExecutingDirectory()
        {
            string path = Assembly.GetEntryAssembly().CodeBase;
            Uri uri = new Uri(path);
            string directory = Path.GetDirectoryName(uri.LocalPath);
            return directory;
        }

        /// <summary>
        /// The relative directory of the executing assembly.
        /// This might not be the location where the .dll files are located.
        /// </summary>
        public static string RelativeExecutingDirectory()
        {
            return RelativeDirectory(Environment.CurrentDirectory, ExecutingDirectory());
        }

        /// <summary>
        /// Directory of Common.dll
        /// This is expected to contain ressource files.
        /// </summary>
        public static string CommonDirectory()
        {
            string location = typeof(Utils).GetTypeInfo().Assembly.Location;
            Uri uri = new Uri(location);
            string directory = Path.GetDirectoryName(uri.LocalPath);
            return directory;
        }

        /// <summary>
        /// Relative Directory of Common.dll.
        /// This is expected to contain ressource files.
        /// </summary>
        public static string RelativeCommonDirectory()
        {
            return RelativeDirectory(Environment.CurrentDirectory, CommonDirectory());
        }

        public static string CreateMD5(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }

            return sb.ToString().ToLower();
        }

        public static string RelativeDirectory(string fromDirectory, string toDirectory)
        {
            return RelativeDirectory(fromDirectory, toDirectory, toDirectory, Path.DirectorySeparatorChar);
        }

        public static string RelativeDirectory(string fromDirectory, string toDirectory, string defaultDirectory)
        {
            return RelativeDirectory(fromDirectory, toDirectory, defaultDirectory, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Returns a directory that is relative.
        /// </summary>
        /// <param name="fromDirectory">The directory to navigate from.</param>
        /// <param name="toDirectory">The directory to reach.</param>
        /// <param name="defaultDirectory">A directory to return on failure.</param>
        /// <param name="directorySeparator"></param>
        /// <returns>The relative directory or the defaultDirectory on failure.</returns>
        public static string RelativeDirectory(string fromDirectory, string toDirectory, string defaultDirectory,
            char directorySeparator)
        {
            string result;

            if (fromDirectory.EndsWith("\\") || fromDirectory.EndsWith("/"))
            {
                fromDirectory = fromDirectory.Remove(fromDirectory.Length - 1);
            }

            if (toDirectory.EndsWith("\\") || toDirectory.EndsWith("/"))
            {
                toDirectory = toDirectory.Remove(toDirectory.Length - 1);
            }

            if (toDirectory.StartsWith(fromDirectory))
            {
                result = toDirectory.Substring(fromDirectory.Length);
                if (result.StartsWith("\\") || result.StartsWith("/"))
                {
                    result = result.Substring(1, result.Length - 1);
                }

                if (result != "")
                {
                    result += directorySeparator;
                }
            }
            else
            {
                string[] fromDirs = fromDirectory.Split(':', '\\', '/');
                string[] toDirs = toDirectory.Split(':', '\\', '/');
                if (fromDirs.Length <= 0 || toDirs.Length <= 0 || fromDirs[0] != toDirs[0])
                {
                    return defaultDirectory;
                }

                int offset = 1;
                for (; offset < fromDirs.Length; offset++)
                {
                    if (toDirs.Length <= offset)
                    {
                        break;
                    }

                    if (fromDirs[offset] != toDirs[offset])
                    {
                        break;
                    }
                }

                StringBuilder relativeBuilder = new StringBuilder();
                for (int i = 0; i < fromDirs.Length - offset; i++)
                {
                    relativeBuilder.Append("..");
                    relativeBuilder.Append(directorySeparator);
                }

                for (int i = offset; i < toDirs.Length - 1; i++)
                {
                    relativeBuilder.Append(toDirs[i]);
                    relativeBuilder.Append(directorySeparator);
                }

                result = relativeBuilder.ToString();
            }

            result = DirectorySeparator(result, directorySeparator);
            return result;
        }

        public static string DirectorySeparator(string path)
        {
            return DirectorySeparator(path, Path.DirectorySeparatorChar);
        }

        public static string DirectorySeparator(string path, char directorySeparator)
        {
            if (directorySeparator != '\\')
            {
                path = path.Replace('\\', directorySeparator);
            }

            if (directorySeparator != '/')
            {
                path = path.Replace('/', directorySeparator);
            }

            return path;
        }

        public static string GenerateSessionKey(int desiredLength)
        {
            StringBuilder sessionKey = new StringBuilder();
            using (RNGCryptoServiceProvider cryptoProvider = new RNGCryptoServiceProvider())
            {
                byte[] random = new byte[1];
                int length = 0;
                while (length < desiredLength)
                {
                    cryptoProvider.GetBytes(random);
                    char c = (char) random[0];
                    if ((Char.IsDigit(c) || Char.IsLetter(c)) && random[0] < 127)
                    {
                        length++;
                        sessionKey.Append(c);
                    }
                }
            }

            return sessionKey.ToString();
        }

        public static byte[] GenerateKey(int desiredLength)
        {
            byte[] random = new byte[desiredLength];
            using (RNGCryptoServiceProvider cryptoProvider = new RNGCryptoServiceProvider())
            {
                cryptoProvider.GetNonZeroBytes(random);
            }

            return random;
        }

        /// <summary>
        /// Removes entries from a collection.
        /// The input lists are not modified, instead a new collection is returned.
        /// </summary>
        public static TList SubtractList<TList, TItem>(TList entries, params TItem[] excepts)
            where TList : ICollection<TItem>, new()
        {
            TList result = new TList();
            foreach (TItem entry in entries)
            {
                result.Add(entry);
            }

            foreach (TItem except in excepts)
            {
                result.Remove(except);
            }

            return result;
        }
    }
}