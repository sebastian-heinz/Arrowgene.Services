using System.Reflection;

namespace Arrowgene.Services.Scripting
{
    /// <summary>
    /// Creates Instances from scripts.
    /// </summary>
    public class ScriptEngine
    {
        private readonly ScriptContext _globalScriptContext;

        public ScriptEngine()
        {
            _globalScriptContext = new ScriptContext();
        }

        public void AddReference(Assembly assembly)
        {
            _globalScriptContext.AddReference(assembly);
        }

        /// <summary>
        /// Creates a new <see cref="ScriptTask"/> with a global context.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="code"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public ScriptTask CreateTask(string name, string code, params object[] args)
        {
            return new ScriptTask(name, code, _globalScriptContext, args);
        }
    }
}