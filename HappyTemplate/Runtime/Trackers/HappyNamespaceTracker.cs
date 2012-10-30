using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace HappyTemplate.Runtime.Trackers
{
	
    class HappyNamespaceTracker : IHappyTracker, IEnumerable<IHappyTracker>
	{
		private string _name;
		public string Name { get { return _name; } }
		private string _fullName;
    	public HappyNamespaceTracker _parent;
		readonly Dictionary<string, IHappyTracker> _members = new Dictionary<string, IHappyTracker>();

		public HappyNamespaceTracker(HappyNamespaceTracker parent, string name)
		{
			_name = name;
			_parent = parent;
		}

		public override string ToString()
		{
			return _name;
		}

    	public IEnumerator<IHappyTracker> GetEnumerator()
    	{
    		return _members.Values.GetEnumerator();
    	}

    	IEnumerator IEnumerable.GetEnumerator()
    	{
			return _members.Values.GetEnumerator();
    	}

    	private void RecurseAppendName(StringBuilder sb)
		{
			if (_parent != null)
				_parent.RecurseAppendName(sb);

			if(sb.Length != 0)
				sb.Append('.');
			sb.Append(this.Name);
		}

    	
		public string FullName 
		{ 
			get
			{
				if(_fullName == null)
				{
					StringBuilder sb = new StringBuilder();
					this.RecurseAppendName(sb);
					_fullName = sb.ToString();
				}
				return _fullName;
			}
		}


        //public DynamicMetaObject GetMetaObject(Expression parameter)
        //{
        //    return new HappyNamespaceTrackerMetaObject(parameter, this);
        //}



		internal dynamic SetMember(string name, IHappyTracker value)
		{
			_members[name] = value;
			return value;
		}

		public bool TryGetMember(string memberName, out IHappyTracker result)
		{
			IHappyTracker ns;
			bool success = _members.TryGetValue(memberName, out ns);
			result = ns;
			return success;
		}

		internal bool HasMember(string name)
		{
			return _members.ContainsKey(name);
		}

		internal IHappyTracker GetMember(string name)
		{
			return _members[name];
		}

        //class HappyNamespaceTrackerMetaObject : DynamicMetaObject
        //{
        //    HappyNamespaceTracker _namespaceTracker;

        //    public HappyNamespaceTrackerMetaObject(Expression expression, HappyNamespaceTracker namespaceTracker)
        //        : base(expression, BindingRestrictions.Empty, namespaceTracker)
        //    {
        //        _namespaceTracker = namespaceTracker;
        //    }

        //    public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        //    {
        //        IHappyTracker tracker;

        //        if (_namespaceTracker.TryGetMember(binder.Name, out tracker))
        //        {
        //            BindingRestrictions restrictions = BindingRestrictions.GetInstanceRestriction(this.Expression, _namespaceTracker);
        //            return new DynamicMetaObject(Expression.Constant(tracker), restrictions);
        //        }

        //        return base.BindGetMember(binder);
        //    }
        //}
    	public HappyNamespaceTracker FindNestedNamespace(IEnumerable<string> segments)
    	{
    		HappyNamespaceTracker current = this;

			foreach(string segment in segments)
				current = Util.CastAssert<HappyNamespaceTracker>(current.GetMember(segment));

    		return current;
    	}
	}
}