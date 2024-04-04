
using System.Collections.Generic;
using UnityEngine.Splines;

namespace Comparers
{
    namespace KnotLinkSetComparer
    {
        public class KnotLinkSetEqualityComparer : IEqualityComparer<SortedSet<SplineKnotIndex>>
        {
            public int GetHashCode(SortedSet<SplineKnotIndex> x)
            {
                List<int> hashes = new List<int>();
                int sumHash = 0;
                foreach (SplineKnotIndex ski in x)
                {
                    hashes.Add(ski.GetHashCode());
                }

                // Multiply each ski hash with each ski hash
                for (int i = 0; i <= hashes.Count - 1; i++)
                {
                    for (int j = i + 1; j <= hashes.Count; j++)
                    {
                        if (j == hashes.Count)
                        {
                            sumHash += hashes[i] * 1;
                        }
                        else
                        {
                            sumHash += hashes[i] * hashes[j];
                        }
                    }
                }
                return sumHash;
            }
            public bool Equals(SortedSet<SplineKnotIndex> x, SortedSet<SplineKnotIndex> y)
            {

                foreach (SplineKnotIndex ski in x)
                {
                    if (!y.Contains(ski))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }

    namespace SplineKnotIndexComparer
    {
        public class SplineKnotIndexComparer : IComparer<SplineKnotIndex>
        {
            public int Compare(SplineKnotIndex x, SplineKnotIndex y)
            {
                if (x.Spline < y.Spline)
                    return -1;
                else if (x.Spline > y.Spline)
                    return 1;
                else
                {
                    // If Spline values are equal, compare Knot values
                    if (x.Knot < y.Knot)
                        return -1;
                    else if (x.Knot > y.Knot)
                        return 1;
                    else
                        return 0; // SKIs are equal
                }
            }
        }
    }
}