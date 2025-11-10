using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DataDrivenConstants.Util;

internal class CacheableList<T>(ImmutableArray<T> inner) : IReadOnlyList<T>, IEquatable<CacheableList<T>> where T : IEquatable<T>
{
    private readonly ImmutableArray<T> inner = [.. inner];

    public T this[int index] => ((IReadOnlyList<T>)inner)[index];

    public int Count => ((IReadOnlyCollection<T>)inner).Count;

    public bool Equals(CacheableList<T> other)
    {
        if (other.inner.Length != this.inner.Length)
        {
            return false;
        }
        for (int i = 0; i < this.inner.Length; i++)
        {
            if (!this[i].Equals(other[i]))
            {
                return false;
            }
        }
        return true;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)inner).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)inner).GetEnumerator();
    }
}