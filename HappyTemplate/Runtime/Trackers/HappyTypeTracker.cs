using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HappyTemplate.Exceptions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;

namespace HappyTemplate.Runtime.Trackers
{
	class HappyTypeTracker : IHappyTracker
	{
		readonly Type _myType;
		private readonly Dictionary<string, IHappyTracker> _members = new Dictionary<string, IHappyTracker>();

		public Type Type { get { return _myType; } }

			public string FullName
		{
			get
			{
				return _myType.FullName;
			}

		}
		public string Name
		{
			get { return _myType.Name; }
		}

		public ConstructorInfo[] Constructors
		{
			get
			{
				return _myType.GetConstructors();
			}
		}

		public HappyTypeTracker(/*HappyNamespaceTracker myNamespace, */Type myType)
		{
			_myType = myType;
		}


		public bool FindMemberTracker(string memberName, out IHappyTracker result)
		{
			IHappyTracker tracker;
			if (!_members.TryGetValue(memberName, out tracker))
			{
				MemberInfo[] memberInfos = _myType.GetMember(memberName);

				if (memberInfos.Length == 0)
				{
					result = null;
					return false;
				}
				MemberInfo found = memberInfos[0];
				if(found is MethodInfo) //<--if the first memberInfo is a MethodInfo, they all are MethodInfo
					tracker = new HappyMethodTracker(this, memberName, (from m in memberInfos select (MethodInfo)m).ToArray());
				else if (found is PropertyInfo || found is FieldInfo)
				{
					DebugAssert.IsTrue(memberInfos.Length == 1, "I think I found {0} PropertyInfos.  I really just wanted 1.", memberInfos.Length);
					tracker = new HappyPropertyOrFieldTracker(memberName, found);
				}
				else if (found is Type)
				{
					tracker = new HappyTypeTracker((Type)found);
				}
				else
					throw new UnhandledCaseException();

				_members[memberName] = tracker;

			}

			result = tracker;
			return true;
		}

		//class HappyTypeTrackerMetaObject : DynamicMetaObject
		//{
		//    HappyTypeTracker _typeTracker;

		//    public HappyTypeTrackerMetaObject(Expression expression, HappyTypeTracker typeTracker)
		//        : base(expression, BindingRestrictions.Empty, typeTracker)
		//    {
		//        _typeTracker = typeTracker;
		//    }

		//    public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
		//    {
		//        HappyMethodTracker tracker;

		//        if(_typeTracker.FindMemberTracker(binder.Name, out tracker))
		//        {
		//            BindingRestrictions restrictions = BindingRestrictions.GetInstanceRestriction(this.Expression, _typeTracker);
		//            return new DynamicMetaObject(Expression.Constant(tracker), restrictions);
		//        }
		//        return base.BindGetMember(binder);
		//    }

		//    public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
		//    {
		//        HappyMethodTracker tracker;

		//        if (_typeTracker.FindMemberTracker(binder.Name, out tracker))
		//        {
		//            OverloadResolver or = _typeTracker._languageContext.OverloadResolverFactory.CreateOverloadResolver(args, new CallSignature(args.Length), CallTypes.None);

		//            BindingTarget bt = or.ResolveOverload(binder.Name, tracker.Methods, NarrowingLevel.None, NarrowingLevel.All);

		//            BindingRestrictions restrictions = BindingRestrictions.Combine(args);
		//            if (!bt.Success)
		//            {
		//                foreach (DynamicMetaObject mo in args)
		//                    restrictions = restrictions.Merge(BindingRestrictions.GetTypeRestriction(mo.Expression, mo.GetLimitType()));

		//                return DefaultBinder.MakeError(or.MakeInvalidParametersError(bt), restrictions, typeof(object));
		//            }

		//            restrictions = BindingRestrictions.GetInstanceRestriction(this.Expression, _typeTracker);
		//            restrictions.Merge(bt.RestrictedArguments.GetAllRestrictions());
		//            Expression callExpression = bt.MakeExpression();

		//            if (callExpression.Type == typeof(void))
		//                callExpression = Expression.Block(new[] { callExpression, Expression.Constant(null) });
		//            else if (callExpression.Type != typeof(object))
		//                callExpression = Expression.Convert(callExpression, typeof(object));

		//            return new DynamicMetaObject(callExpression, restrictions);
		//        }
		//        return base.BindInvokeMember(binder, args);
		//    }
		//}
	}
}