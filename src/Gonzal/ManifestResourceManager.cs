// Copyright 2020 Alberto Gonzalez. All rights reserved.
// Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.
// TODO: add Gonzal copyright to License.txt???

using System.IO;
using System.Reflection;

namespace Gonzal
{
    public class ManifestResourceManager
    {
        private readonly Assembly _assembly;
        private readonly string _assemblyName;

        public ManifestResourceManager(Assembly assembly = null)
        {
            _assembly = assembly ?? Assembly.GetEntryAssembly();
            _assemblyName = _assembly.GetName().Name;
        }

        public string GetFullName(string name)
            => $"{_assemblyName}.{name?.Replace('/', '.').Replace('\\','.')}";

        public ManifestResourceInfo GetInfo(string name)
            => _assembly.GetManifestResourceInfo(GetFullName(name));

        public string[] GetNames()
            => _assembly.GetManifestResourceNames();

        public Stream GetStream(string name)
            => _assembly.GetManifestResourceStream(GetFullName(name));

        public string GetString(string name) {
            var stream = GetStream(name);
            if (stream == null)
                return null;
            using var sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }
    }
}
