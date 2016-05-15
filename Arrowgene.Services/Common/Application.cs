/*
 *  Copyright 2015 Sebastian Heinz <sebastian.heinz.gt@googlemail.com>
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 */
namespace Arrowgene.Services.Common
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    public static class Application
    {
        public static Random Random = new Random();

        public static string DirectoryPath
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().EscapedCodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string Version
        {
            get
            {
                System.Reflection.Assembly current = System.Reflection.Assembly.GetExecutingAssembly();
                string exe = current.GetModules()[0].FullyQualifiedName;
                FileVersionInfo fi = FileVersionInfo.GetVersionInfo(exe);
                return fi.FileVersion;
            }
        }

    }
}
