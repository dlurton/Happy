/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HappyTemplate.Runtime;

namespace HappyTemplate.Compiler
{
	class AssemblyGenerator
	{
		readonly AssemblyBuilder _assemblyBuilder;
		readonly AssemblyName _assemblyName;
		ModuleBuilder _moduleBuilder;
		TypeBuilder _typeBuilder;

		public Assembly EmittedAssembly { get { return _assemblyBuilder; } }

		public AssemblyGenerator(string assemblyName)
		{
			_assemblyName = new AssemblyName { Name = assemblyName };
			_assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.Run);
		}

		public void DefineModule(string moduleName, bool includeSymbolInfo)
		{
			_moduleBuilder = _assemblyBuilder.DefineDynamicModule(moduleName, includeSymbolInfo);
		}

		public void DefineType(string typeName)
		{
			if(_moduleBuilder == null)
				throw new InvalidOperationException("Module has not been specified (call DefineModule first)");

			_typeBuilder = _moduleBuilder.DefineType(typeName, TypeAttributes.Public, null, null);
		}

		public void DefineMethod(string methodName, LambdaExpression lambda)
		{
			if (_moduleBuilder == null)
				throw new InvalidOperationException("Type has not been specified (call DefineModule first)");

			MethodBuilder methodBuilder = _typeBuilder.DefineMethod(
				methodName,
				MethodAttributes.Public | MethodAttributes.Static,
				CallingConventions.Standard,
				typeof(Action<HappyRuntimeContext>),
				null);

			LambdaExpression getRciMethod = Expression.Lambda(lambda);
			getRciMethod.CompileToMethod(methodBuilder, DebugInfoGenerator.CreatePdbGenerator());
		}

		public FieldInfo DefineField(string fieldName, Type type)
		{
			return _typeBuilder.DefineField(fieldName, type, FieldAttributes.Public | FieldAttributes.Static);
		}

		public Type DefineDelegate(string delegateName, Type returnType, Type[] paramTypes)
		{
			// Create a delegate that has the same signature as the method we would like to hook up to
			var delegateBuilder = _moduleBuilder.DefineType(
				delegateName,
				TypeAttributes.Class |
				TypeAttributes.Public |
				TypeAttributes.Sealed |
				TypeAttributes.AnsiClass |
				TypeAttributes.AutoClass,
				typeof(MulticastDelegate));

			var constructorBuilder = delegateBuilder.DefineConstructor(
				MethodAttributes.RTSpecialName |
				MethodAttributes.HideBySig |
				MethodAttributes.Public,
				CallingConventions.Standard,
				new[] { typeof(object), typeof(IntPtr) });
			constructorBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

			var invokeMethodBuilder = delegateBuilder.DefineMethod(
				"Invoke", 
				MethodAttributes.Public | 
				MethodAttributes.HideBySig | 
				MethodAttributes.NewSlot | 
				MethodAttributes.Virtual, 
				returnType, 
				paramTypes);

			invokeMethodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

			return delegateBuilder.CreateType();
		}

		public Type CompleteType()
		{
			Type retval = _typeBuilder.CreateType();
			_typeBuilder = null;
			return retval;
		}

		
	}
}

