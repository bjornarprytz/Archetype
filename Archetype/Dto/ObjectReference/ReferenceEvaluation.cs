using System;
using System.Reflection;

namespace Archetype
{
    public class ReferenceEvaluation<T, V>
    {
        private PropertyInfo _propertyInfo { get; }
        private Func<V, bool> _evaluationFunc { get; }

        public ReferenceEvaluation(string memberName, Func<V, bool> evaluateFunc)
        {
            _propertyInfo = typeof(T).GetProperty(memberName);

            if (_propertyInfo == null) throw new Exception($"Invalid member name {memberName}");

            if (_propertyInfo.PropertyType != typeof(V)) throw new ArgumentException($"Member type {typeof(V)} is different from {_propertyInfo.PropertyType}");

            _evaluationFunc = evaluateFunc;
        }

        public bool Evaluate(T toEvaluate)
        {
            V val = GetValue(toEvaluate);

            if (val == null) return false;

            return _evaluationFunc(val);
        }

        public V GetValue(T refd) => (V)_propertyInfo.GetValue(refd);
    }
}
