/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System.Collections.Generic;
using System.Dynamic;
using Happy.ScriptEngine.Runtime.Binding;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;

namespace Happy.ScriptEngine.Runtime
{
	internal sealed class HappyOverloadResolverFactory : OverloadResolverFactory
	{
		private readonly HappyBinder _binder;

		public HappyOverloadResolverFactory(HappyBinder binder)
		{
			_binder = binder;
		}

		public override DefaultOverloadResolver CreateOverloadResolver(IList<DynamicMetaObject> args, CallSignature signature, CallTypes callType)
		{
			return new DefaultOverloadResolver(_binder, args, signature, callType);
		}
	}
	// public sealed class HappyOverloadResolver : DefaultOverloadResolver {
	//    private readonly Expression _context;

	//    public Expression ContextExpression {
	//        get { return _context; }
	//    }

	//    // instance method call:
	//    public HappyOverloadResolver(HappyBinder binder, DynamicMetaObject instance, IList<DynamicMetaObject> args, CallSignature signature,
	//        Expression codeContext)
	//        : base(binder, instance, args, signature) {
	//        Assert.NotNull(codeContext);
	//        _context = codeContext;
	//    }

	//    // method call:
	//    public HappyOverloadResolver(HappyBinder binder, IList<DynamicMetaObject> args, CallSignature signature, Expression codeContext)
	//        : this(binder, args, signature, CallTypes.None, codeContext) {
	//    }

	//    // method call:
	//    public HappyOverloadResolver(HappyBinder binder, IList<DynamicMetaObject> args, CallSignature signature, CallTypes callType, Expression codeContext)
	//        : base(binder, args, signature, callType) {
	//        Assert.NotNull(codeContext);
	//        _context = codeContext;
	//    }

		//private new HappyBinder Binder
		//{
		//    get {
		//        return (HappyBinder)base.Binder;
		//    }
		//}

		//public override bool CanConvertFrom(Type fromType, DynamicMetaObject fromArg, ParameterWrapper toParameter, NarrowingLevel level) 
		//{
		//    //if ((fromType == typeof(List) || fromType.IsSubclassOf(typeof(List)))) {
		//    //    if (toParameter.Type.IsGenericType &&
		//    //        toParameter.Type.GetGenericTypeDefinition() == typeof(IList<>) &&
		//    //        (toParameter.ParameterInfo.IsDefined(typeof(BytesConversionAttribute), false) ||
		//    //         toParameter.ParameterInfo.IsDefined(typeof(BytesConversionNoStringAttribute), false))) {
		//    //        return false;
		//    //    }
		//    //} else if (fromType == typeof(string)) {
		//    //    if (toParameter.Type == typeof(IList<byte>) &&
		//    //        !Binder.Context.PythonOptions.Python30 &&
		//    //        toParameter.ParameterInfo.IsDefined(typeof(BytesConversionAttribute), false)) {
		//    //        // string -> byte array, we allow this in Python 2.6
		//    //        return true;
		//    //    }
		//    //} else if (fromType == typeof(Bytes)) {
		//    //    if (toParameter.Type == typeof(string) &&
		//    //        !Binder.Context.PythonOptions.Python30 &&
		//    //        toParameter.ParameterInfo.IsDefined(typeof(BytesConversionAttribute), false)) {
		//    //        return true;
		//    //    }
		//    //}

		//    return base.CanConvertFrom(fromType, fromArg, toParameter, level);
		//}

		//protected override BitArray MapSpecialParameters(ParameterMapping mapping) {
		//    var infos = mapping.ParameterInfos;
		//    BitArray special = base.MapSpecialParameters(mapping);

		//    if (infos.Length > 0) {
		//        bool normalSeen = false;
		//        for (int i = 0; i < infos.Length; i++) {
		//            bool isSpecial = false;
		//            if (infos[i].ParameterType.IsSubclassOf(typeof(SiteLocalStorage))) {
		//                mapping.AddBuilder(new SiteLocalStorageBuilder(infos[i]));
		//                isSpecial = true;
		//            } else if (infos[i].ParameterType == typeof(CodeContext) && !normalSeen) {
		//                mapping.AddBuilder(new ContextArgBuilder(infos[i]));
		//                isSpecial = true;
		//            } else {
		//                normalSeen = true;
		//            }

		//            if (isSpecial) {
		//                (special = special ?? new BitArray(infos.Length))[i] = true;
		//            }
		//        }
		//    }

		//    return special;
		//}

		//protected override Expression GetByRefArrayExpression(Expression argumentArrayExpression) {
		//    return Expression.Call(typeof(PythonOps).GetMethod("MakeTuple"), argumentArrayExpression);
		//}

		//protected override bool AllowKeywordArgumentSetting(MethodBase method) {
		//    return CompilerHelpers.IsConstructor(method) && !method.DeclaringType.IsDefined(typeof(PythonTypeAttribute), true);
		//}

		//public override Expression Convert(DynamicMetaObject metaObject, Type restrictedType, ParameterInfo info, Type toType) {
		//    return Binder.ConvertExpression(metaObject.Expression, toType, ConversionResultKind.ExplicitCast, new PythonOverloadResolverFactory(Binder, _context));
		//}

		//public override Expression GetDynamicConversion(Expression value, Type type) {
		//    return Expression.Dynamic(
		//        Binder.Context.Convert(type, ConversionResultKind.ExplicitCast), 
		//        type, 
		//        value);
		//}

		//public override Type GetGenericInferenceType(DynamicMetaObject dynamicObject) {            
		//    Type res = PythonTypeOps.GetFinalSystemType(dynamicObject.LimitType);
		//    if (res == typeof(ExtensibleString) ||
		//        res == typeof(ExtensibleComplex) || 
		//        (res.IsGenericType && res.GetGenericTypeDefinition() == typeof(Extensible<>))) {
		//        return typeof(object);
		//    }

		//    return res;
		//}

		//public override Func<object[], object> GetConvertor(int index, DynamicMetaObject metaObject, ParameterInfo info, Type toType) {
		//    return Binder.ConvertObject(index, metaObject, toType, ConversionResultKind.ExplicitCast);
		//}
    //}
}

