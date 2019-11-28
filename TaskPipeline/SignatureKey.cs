namespace Pipeline
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Funaction signature for using as a key in dictionary
    /// </summary>
    public class SignatureKey
    {
        private Type _retType;
        private ParameterInfo[] _params;

        /// <summary>
        /// Function return type
        /// </summary>
        /// <value>Type</value>
        public Type ReturnType => _retType;

        /// <summary>
        /// Function params
        /// </summary>
        /// <value>Array of ParameterInfo</value>
        public ParameterInfo[] Params => _params;

        public SignatureKey(Type retType, ParameterInfo[] param)
        {
            _retType = retType;
            _params = param;
        }

        public static SignatureKey GetSignature(Delegate func)
        {
            return new SignatureKey(func.Method.ReturnType, func.Method.GetParameters());
        }

        public static SignatureKey GetSignature<T>(Func<T> func)
        {
            return new SignatureKey(typeof(T), null);
        }

        public static SignatureKey GetSignature<T, TResult>(Func<T, TResult> func)
        {
            return new SignatureKey(typeof(TResult), func.GetMethodInfo().GetParameters());
        }

        public static SignatureKey GetSignature<T1, T2, TResult>(Func<T1, T2, TResult> func)
        {
            return new SignatureKey(typeof(TResult), func.GetMethodInfo().GetParameters());
        }

        public static SignatureKey GetSignature(Action func)
        {
            return new SignatureKey(typeof(void), null);
        }

        public static SignatureKey GetSignature<T>(Action<T> func)
        {
            return new SignatureKey(typeof(void), func.GetMethodInfo().GetParameters());
        }

        public static SignatureKey GetSignature<T1, T2>(Action<T1, T2> func)
        {
            return new SignatureKey(typeof(void), func.GetMethodInfo().GetParameters());
        }

        public static SignatureKey GetSignature<T1, T2, T3>(Action<T1, T2, T3> func)
        {
            return new SignatureKey(typeof(void), func.GetMethodInfo().GetParameters());
        }

        public override bool Equals(object obj)
        {
            return obj is SignatureKey key &&
                   EqualityComparer<Type>.Default.Equals(_retType, key._retType) &&
                   EqualityComparer<ParameterInfo[]>.Default.Equals(_params, key._params) &&
                   EqualityComparer<Type>.Default.Equals(ReturnType, key.ReturnType) &&
                   EqualityComparer<ParameterInfo[]>.Default.Equals(Params, key.Params);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_retType, _params, ReturnType, Params);
        }
    }
}