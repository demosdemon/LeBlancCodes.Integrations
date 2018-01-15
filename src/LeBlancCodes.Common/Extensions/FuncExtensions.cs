using System;
using System.Threading.Tasks;

namespace LeBlancCodes.Common.Extensions
{
    public static class FuncExtensions
    {
        public static bool IsAsync<TResult>(this Func<TResult> func) => typeof(Task).IsAssignableFrom(typeof(TResult));

        public static bool IsAsync<T1, TResult>(this Func<T1, TResult> func) => typeof(Task).IsAssignableFrom(typeof(TResult));

        public static bool IsAsync<T1, T2, TResult>(this Func<T1, T2, TResult> func) => typeof(Task).IsAssignableFrom(typeof(TResult));

        public static bool IsAsync<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func) => typeof(Task).IsAssignableFrom(typeof(TResult));

        public static bool IsAsync<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> func) => typeof(Task).IsAssignableFrom(typeof(TResult));

        public static bool IsAsync<T1, T2, T3, T4, T5, TResult>(this Func<T1, T2, T3, T4, T5, TResult> func) => typeof(Task).IsAssignableFrom(typeof(TResult));

        public static bool IsAsync<T1, T2, T3, T4, T5, T6, TResult>(this Func<T1, T2, T3, T4, T5, T6, TResult> func) => typeof(Task).IsAssignableFrom(typeof(TResult));

        public static bool IsAsync<T1, T2, T3, T4, T5, T6, T7, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, TResult> func) => typeof(Task).IsAssignableFrom(typeof(TResult));

        public static bool IsAsync<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func) => typeof(Task).IsAssignableFrom(typeof(TResult));

        public static bool IsAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> func) => typeof(Task).IsAssignableFrom(typeof(TResult));

        public static bool IsAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> func) => typeof(Task).IsAssignableFrom(typeof(TResult));

        public static bool IsAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> func) => typeof(Task).IsAssignableFrom(typeof(TResult));

        public static bool IsAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> func) => typeof(Task).IsAssignableFrom(typeof(TResult));

        public static bool IsAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> func) => typeof(Task).IsAssignableFrom(typeof(TResult));

        public static bool IsAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> func) => typeof(Task).IsAssignableFrom(typeof(TResult));

        public static bool IsAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> func) => typeof(Task).IsAssignableFrom(typeof(TResult));

    }
}
