using System;
using System.Collections.Generic;

namespace CyBF.Utility
{
    public static class Extensions
    {
        public static bool MatchSequence<S,T>(this IEnumerable<S> xs, IEnumerable<T> ys, Func<S,T,bool> match)
        {
            IEnumerator<S> xsenum = xs.GetEnumerator();
            IEnumerator<T> ysenum = ys.GetEnumerator();

            while (xsenum.MoveNext())
            {
                if (!ysenum.MoveNext())
                    return false;

                S x = xsenum.Current;
                T y = ysenum.Current;

                if (!match(x, y))
                    return false;
            }

            if (ysenum.MoveNext())
                return false;

            return true;
        }
    }
}
