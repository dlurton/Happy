/**************************************************************************** 
 * Copyright 2012 David Lurton
 * This Source Code Form is subject to the terms of the Mozilla Public 
 * License, v. 2.0. If a copy of the MPL was not distributed with this file, 
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 ****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Yaml;

namespace Happy.RuntimeLib
{
    /// <summary>
    /// TODO:  cache DyanamicMapping instances instead of creating new ones every single time...
    /// </summary>
    public class QuickYaml : DynamicObject
    {
        readonly YamlMapping _mapping;

        internal QuickYaml(YamlMapping mapping)
        {
            _mapping = mapping;
        }

        public static dynamic FromFile(string filename)
        {
            YamlNode[] nodes = YamlNode.FromYamlFile(filename);
            return wrapNodeInDynamic(nodes[0]);
        }

        public static dynamic FromString(string @string)
        {
            YamlNode[] nodes = YamlNode.FromYaml(@string);
            return wrapNodeInDynamic(nodes[0]);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            YamlNode node;
            result = null;
            if (_mapping.TryGetValue(binder.Name, out node))
            {
                result = wrapNodeInDynamic(node);
                return true;
            }
            return false;
        }

        static dynamic wrapNodeInDynamic(YamlNode node)
        {
            if (node is YamlScalar)
                return ((YamlScalar)node).Value;
            if (node is YamlMapping)
                return new QuickYaml((YamlMapping)node);
            if (node is YamlSequence)
                return new DynamicYamlSequence((YamlSequence)node);

            throw new InvalidOperationException("I don't know how to wrap " + node.GetType().FullName);
        }

        // ReSharper disable MemberCanBePrivate.Local
        public class DynamicYamlSequence : IEnumerable<object> // ReSharper restore MemberCanBePrivate.Local
        {
            readonly YamlSequence _sequence;

            public DynamicYamlSequence(YamlSequence sequence)
            {
                _sequence = sequence;
            }

            public int _count { get { return _sequence.Count; } }

            // ReSharper disable UnusedMember.Local
            public object this[int index] { get { return wrapNodeInDynamic(_sequence[index]); } }
            // ReSharper restore UnusedMember.Local

            public IEnumerator<object> GetEnumerator()
            {
                return _sequence.Select(wrapNodeInDynamic).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _sequence.Select(wrapNodeInDynamic).GetEnumerator();
            }
        }
    }
}

