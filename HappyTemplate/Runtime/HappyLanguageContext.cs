#if DEBUG
//#define WRITE_AST
#endif

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using HappyTemplate.Compiler;
using HappyTemplate.Compiler.Ast;
using HappyTemplate.Compiler.AstVisitors;
using HappyTemplate.Runtime.Binding;
using HappyTemplate.Runtime.Trackers;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Scripting;
using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace HappyTemplate.Runtime
{
	public class HappyLanguageContext : LanguageContext
	{
		public HappyErrorSink ErrorSink { get; private set; }
		public TextWriter Output { get; private set; }
		public TextWriter ErrorOutput { get; private set; }
		internal HappyBinder Binder { get; private set; }
		internal HappyOverloadResolverFactory OverloadResolverFactory { get; private set; }
		public ScriptDomainManager _manager;
		public ScriptDomainManager ScriptDomainManager { get { return _manager;  } }

		internal const bool BinderNoThrow = false;

		//public TopNamespaceTracker TopNamespaceTracker { get; private set; }
		
		public HappyLanguageContext(ScriptDomainManager manager, IDictionary<string, object> options) : base(manager)
		{
			_manager = manager;
			this.ErrorSink = new HappyErrorSink(this);
			this.Output = manager.SharedIO.GetWriter(ConsoleStreamType.Output);
			this.ErrorOutput = manager.SharedIO.GetWriter(ConsoleStreamType.ErrorOutput);
			this.Binder = new HappyBinder();
			this.OverloadResolverFactory = new HappyOverloadResolverFactory(this.Binder);
			//this.TopNamespaceTracker = new TopNamespaceTracker(manager);
		}

		public override ErrorSink GetCompilerErrorSink() 
		{
			return this.ErrorSink;
		}
		public override ScriptCode CompileSourceCode(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink)
		{
			Module module = Parser.ParseModule(sourceUnit, this);
			//AstAnalyzer ctsg = new AstAnalyzer(this);
			//return ctsg.Analyze(ts, sourceUnit);

			//AstAnalyzer_New ctsg = new AstAnalyzer_New(this);
			//return ctsg.Analyze(ts, sourceUnit);

#if WRITE_AST
			module.WriteString(new AstWriter(Console.Out));
#endif
			AstAnalyzer ctsg = new AstAnalyzer(this);
			return ctsg.Analyze(module, sourceUnit);
		}


		#region Binder factories
		/////////////////////////
		// Canonicalizing Binders
		/////////////////////////

		// We need to canonicalize binders so that we can share L2 dynamic
		// dispatch caching across common call sites.  Every call site with the
		// same operation and same metadata on their binders should return the
		// same rules whenever presented with the same kinds of inputs.  The
		// DLR saves the L2 cache on the binder instance.  If one site somewhere
		// produces a rule, another call site performing the same operation with
		// the same metadata could get the L2 cached rule rather than computing
		// it again.  For this to work, we need to place the same binder instance
		// on those functionally equivalent call sites.


		private readonly Dictionary<string, HappyGetMemberBinder> _getMemberBinders = new Dictionary<string, HappyGetMemberBinder>();
		public override GetMemberBinder CreateGetMemberBinder(string name, bool ignoreCase)
		{
			lock (_getMemberBinders)
			{
				if (_getMemberBinders.ContainsKey(name))
					return _getMemberBinders[name];
				var b = new HappyGetMemberBinder(this, name);
				_getMemberBinders[name] = b;
				return b;
			}
		}

		private readonly Dictionary<string, HappySetMemberBinder> _setMemberBinders = new Dictionary<string, HappySetMemberBinder>();
		public override SetMemberBinder CreateSetMemberBinder(string name, bool ignoreCase)
		{
			lock (_setMemberBinders)
			{
				if (_setMemberBinders.ContainsKey(name))
					return _setMemberBinders[name];
				var b = new HappySetMemberBinder(this, name);
				_setMemberBinders[name] = b;
				return b;
			}
		}


		private readonly Dictionary<CallInfo, HappyInvokeBinder> _invokeBinders = new Dictionary<CallInfo, HappyInvokeBinder>();
		public override InvokeBinder CreateInvokeBinder(CallInfo callInfo)
		{
			lock (_invokeBinders)
			{
				if (_invokeBinders.ContainsKey(callInfo))
					return _invokeBinders[callInfo];
				var b = new HappyInvokeBinder(callInfo);
				_invokeBinders[callInfo] = b;
				return b;
			}
		}

		private readonly Dictionary<InvokeMemberBinderKey, HappyInvokeMemberBinder> _invokeMemberBinders = new Dictionary<InvokeMemberBinderKey, HappyInvokeMemberBinder>();
		public override InvokeMemberBinder  CreateCallBinder(string name, bool ignoreCase, CallInfo callInfo)
		{
			lock (_invokeMemberBinders)
			{
				InvokeMemberBinderKey key = new InvokeMemberBinderKey(name, callInfo);
				if (_invokeMemberBinders.ContainsKey(key))
					return _invokeMemberBinders[key];
				var b = new HappyInvokeMemberBinder(this, key.Name, key.Info);
				_invokeMemberBinders[key] = b;
				return b;
			}
		}

		


		private readonly Dictionary<CallInfo, HappyCreateInstanceBinder> _createInstanceBinders = new Dictionary<CallInfo, HappyCreateInstanceBinder>();
		public override CreateInstanceBinder CreateCreateBinder(CallInfo info)
		{
			lock (_createInstanceBinders)
			{
				if (_createInstanceBinders.ContainsKey(info))
					return _createInstanceBinders[info];
				var b = new HappyCreateInstanceBinder(this, info);
				_createInstanceBinders[info] = b;
				return b;
			}
		}

		
		private readonly Dictionary<ExpressionType, HappyBinaryOperationBinder> _binaryOperationBinders = new Dictionary<ExpressionType, HappyBinaryOperationBinder>();
		public override BinaryOperationBinder CreateBinaryOperationBinder(ExpressionType op)
		{
			lock (_binaryOperationBinders)
			{
				if (_binaryOperationBinders.ContainsKey(op))
					return _binaryOperationBinders[op];
				var b = new HappyBinaryOperationBinder(op);
				_binaryOperationBinders[op] = b;
				return b;
			}
		}

		private readonly Dictionary<CallInfo, HappyGetIndexBinder> _getIndexBinders = new Dictionary<CallInfo, HappyGetIndexBinder>();
		public HappyGetIndexBinder CreateGetIndexBinder(CallInfo info)
		{
			lock (this._getIndexBinders)
			{
				if (this._getIndexBinders.ContainsKey(info))
					return this._getIndexBinders[info];
				var b = new HappyGetIndexBinder(info);
				this._getIndexBinders[info] = b;
				return b;
			}
		}

		private readonly Dictionary<CallInfo, HappySetIndexBinder> _setIndexBinders = new Dictionary<CallInfo, HappySetIndexBinder>();
		public HappySetIndexBinder CreateSetIndexBinder(CallInfo info)
		{
			lock (this._setIndexBinders)
			{
				if (this._setIndexBinders.ContainsKey(info))
					return this._setIndexBinders[info];
				var b = new HappySetIndexBinder(info);
				this._setIndexBinders[info] = b;
				return b;
			}
		}

		//private readonly Dictionary<ExpressionType, HappyUnaryOperationBinder> _unaryOperationBinders = new Dictionary<ExpressionType, HappyUnaryOperationBinder>();
		//public override UnaryOperationBinder CreateUnaryOperationBinder(ExpressionType op)
		//{
		//    lock (_unaryOperationBinders)
		//    {
		//        if (_unaryOperationBinders.ContainsKey(op))
		//            return _unaryOperationBinders[op];
		//        var b = new HappyUnaryOperationBinder(op);
		//        _unaryOperationBinders[op] = b;
		//        return b;
		//    }
		//}

		#endregion

	}
}