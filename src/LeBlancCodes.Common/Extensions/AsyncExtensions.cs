using System;
using System.Threading.Tasks;

namespace LeBlancCodes.Common.Extensions
{
    public static class AsyncExtensions
    {
        public static bool IsAsync<T>(this Func<T> action) => typeof(Task).IsAssignableFrom(typeof(T));
    }
}
