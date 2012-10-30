using System.Collections.Generic;
using System.IO;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;

namespace HappyTemplate.Runtime
{
	public class HappyRuntimeContext
	{
		readonly dynamic _globals;
		readonly ScriptScope _globalScope;
		readonly HappyLanguageContext _languageContext;

		public dynamic Globals { get { return _globals;  } }
		public ScriptScope GlobalScope { get { return _globalScope; } }
		
		readonly Stack<TextWriter> _writerStack = new Stack<TextWriter>();
		public TextWriter OutputWriter { get { return _writerStack.Peek(); } }

		//TODO:  Make internal?
		public HappyRuntimeContext(ScriptEngine scriptEngine, TextWriter outputWriter, ScriptScope globalScope)
		{
			_languageContext = HostingHelpers.GetLanguageContext(scriptEngine) as HappyLanguageContext;
			_writerStack.Push(outputWriter);
			
			_globals = globalScope;
			_globals.__runtimeContext__ = this;

			_globalScope = globalScope;
		}

		public StringWriter PushWriter()
		{
			var stringWriter = new StringWriter();
			_writerStack.Push(stringWriter);
			return stringWriter;
		}

		public string PopWriter()
		{
			TextWriter retval = _writerStack.Pop();
			if (retval is StringWriter)
				return retval.ToString();

			return "";
		}

		public static HappyRuntimeContext CompileModule(string template, string configFile)
		{
			ScriptRuntime _runtime = new ScriptRuntime(ScriptRuntimeSetup.ReadConfiguration(configFile));
			ScriptEngine engine = _runtime.GetEngine("ht");
			ScriptScope globals = engine.CreateScope();
			HappyRuntimeContext retval = new HappyRuntimeContext(engine, new StringWriter(), globals);
			engine.Execute(template, globals);

			return retval;
		}

		//public void LoadAssembly(string name)
		//{
		//    AssemblyName assemblyName = new AssemblyName(name);
		//    Assembly assembly = Assembly.Load(assemblyName);

		//    foreach (Type type in assembly.GetTypes().Where(t => t.Namespace != n	ull))
		//    {
		//        string[] namespaceSegments = type.Namespace.Split('.');

		//        if (!DynamicObjectHelpers.HasMember(_globals, namespaceSegments[0]))
		//            DynamicObjectHelpers.SetMember(_globals, namespaceSegments[0],
		//                                           new HappyNamespaceTracker(null, namespaceSegments[0]));

		//        HappyNamespaceTracker current = DynamicObjectHelpers.GetMember(_globals, namespaceSegments[0]);
		//        foreach (string segment in namespaceSegments.Skip(1))
		//        {
		//            if (current.HasMember(segment))
		//                current = (HappyNamespaceTracker) current.GetMember(segment);
		//            else
		//            {
		//                HappyNamespaceTracker next = new HappyNamespaceTracker(current, segment);
		//                current.SetMember(segment, next);
		//                current = next;
		//            }
		//        }

		//        //DynamicObjectHelpers.SetMember(current, type.Name, new HappyTypeTracker(type));
		//        current.SetMember(type.Name, new HappyTypeTracker(/*current,*/ type));
		//    }
		//}


		///// <summary>
		///// This method is intended to be called behind the scenes by the compiled script
		///// and should not be called directly.
		///// </summary>
		///// <param name="?"></param>
		//public void UseNamespace(string[] nsSegments)
		//{
		//    HappyNamespaceTracker namespaceTracker = Util.CastAssert<HappyNamespaceTracker>(_globalScope.GetVariable(nsSegments[0]));

		//    namespaceTracker.FindNestedNamespace(nsSegments.Skip(1));

		//    foreach(var namespaceMember in namespaceTracker)
		//        _globalScope.SetVariable(namespaceMember.Name, namespaceMember);
		//}

		//public void LoadNamespace(string name)
		//{
		//    TopNamespaceTracker ns = new TopNamespaceTracker();
		//    var package ns.TryGetPackage("")
		//}

		//public object this[string key]
		//{
		//    get
		//    {
		//        return DynamicObjectHelpers.MemberAccess(_globals, key);
		//    }
		//    set
		//    {

		//        DynamicObjectHelpers.SetMember(_globals, key, value);
		//    }
		//}
	}
}
