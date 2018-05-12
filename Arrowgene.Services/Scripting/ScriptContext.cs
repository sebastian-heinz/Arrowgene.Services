using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Arrowgene.Services.Scripting
{
    /// <summary>
    /// Describes the dependencies
    /// </summary>
    public class ScriptContext
    {
        private readonly Dictionary<string, List<string>> _typeResolver;
        private readonly List<Assembly> _assemblies;
        private readonly List<MetadataReference> _metadataReferences;

        public ScriptContext()
        {
            _typeResolver = new Dictionary<string, List<string>>();
            _assemblies = new List<Assembly>();
            _metadataReferences = new List<MetadataReference>();
        }

        public ScriptContext(Dictionary<string, List<string>> typeResolver, List<Assembly> assemblies,
            List<MetadataReference> metadataReferences)
        {
            _typeResolver = typeResolver;
            _assemblies = assemblies;
            _metadataReferences = metadataReferences;
        }

        public Dictionary<string, List<string>> TypeResolver => _typeResolver;
        public List<Assembly> Assemblies => _assemblies;
        public List<MetadataReference> MetadataReferences => _metadataReferences;

        public void AddReference(Assembly assembly)
        {
            if (!_assemblies.Contains(assembly))
            {
                _assemblies.Add(assembly);
                _metadataReferences.Add(MetadataReference.CreateFromFile(assembly.Location));
                foreach (Type type in assembly.GetTypes())
                {
                    if (!string.IsNullOrWhiteSpace(type.Namespace) && !string.IsNullOrWhiteSpace(type.Name))
                    {
                        if (_typeResolver.ContainsKey(type.Name))
                        {
                            List<string> namespaces = _typeResolver[type.Name];
                            if (!namespaces.Contains(type.Namespace))
                            {
                                namespaces.Add(type.Namespace);
                            }
                        }
                        else
                        {
                            _typeResolver.Add(type.Name, new List<string> {type.Namespace});
                        }
                    }
                }
            }
        }
    }
}