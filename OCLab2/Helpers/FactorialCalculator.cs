using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Numerics;

namespace OCLab2.Helpers;

public static class FactorialCalculator
{
    public static BigInteger Calculate(int n)
    {
        if (n < 0) return -1;
        if (n == 0 || n == 1) return 1;

        BigInteger result = 1;
        for (int i = 2; i <= n; i++)
        {
            result *= i;
        }
        return result;
    }

    public static Dictionary<int, BigInteger> CalculateParallel(List<int> numbers)
    {
        var results = new ConcurrentDictionary<int, BigInteger>();

        Parallel.ForEach(numbers, number =>
        {
            var factorial = Calculate(number);
            results.TryAdd(number, factorial);
        });

        return results.ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}