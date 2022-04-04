public static class FasterStringOperations
{
    public static bool StartsWithFast(this string s1, params string[] s2)
    {
        foreach (var a in s2)
            if (s1.StartsWith(a))
                return true;
        return false;
    }
    public static bool StartsWithFast(this string s1, string s2, int startIndex = 0)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return false;

        // We need the index end
        int s1Len = s1.Length;
        int s2Len = s2.Length;

        // Too short?
        if (s1Len < s2Len)
            return false;

        for (int iChar = startIndex; iChar < s2Len; iChar++)
        {
            if (s1[iChar] != s2[iChar])
                return false;
        }

        return true;
    }

       
    public static bool EndsWithFast(this string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return false;

        int s1Len = s1.Length - 1;
        int s2Len = s2.Length - 1;

        // Too short?
        if (s1Len < s2Len)
            return false;

        for (int iChar = 0; iChar <= s2Len; iChar++)
        {
            if (s1[s1Len - iChar] != s2[s2Len - iChar])
                return false;
        }

        return true;
    }

    public static bool ContainsFastIc(this string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1))
            return false;

        if (string.IsNullOrEmpty(s2))
            return true;

        int s1Len = s1.Length - s2.Length + 1;
        int s2Len = s2.Length;

        for (int iChar1 = 0; iChar1 < s1Len; iChar1++)
        {
            var l = s1[iChar1];
            var u = s2[0];
            if (l == u || l == u + 32 || l == u - 32) //skip for loop creation if not nessesery
            {
                var match = true;
                for (var iChar2 = 1; iChar2 < s2Len; iChar2++)
                {
                    var c = s1[iChar1 + iChar2];
                    var i = s2[iChar2];
                    if (c != i && c != i - 32 && c != i + 32)
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return true;
            }
        }
        return false;
    }
    
    private static bool ContainsFast(this string s1, string s2) //obsolete 
    {
        if (string.IsNullOrEmpty(s1))
            return false;

        // s1 always contains an empty string
        // Matching String.Contains() behavior
        if (string.IsNullOrEmpty(s2))
            return true;

        // Example:
        // 0123456789
        // 789
        // We only need to check the first string up to
        // the last 3 characters.

        // Length we need to check the first string
        int s1Len = s1.Length - s2.Length + 1;
        int s2Len = s2.Length;
        int iChar2 = 0;
        bool match = false;

        for (int iChar1 = 0; iChar1 < s1Len; iChar1++)
        {
            // Found a possible match
            if (s1[iChar1] == s2[0])
            {
                match = true;

                // Loop until we find a mismatch.
                // If we don't, then it matches.
                for (iChar2 = 0; iChar2 < s2Len; iChar2++)
                {
                    if (s1[iChar1 + iChar2] != s2[iChar2])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return true;
            }
        }
        return false;
    }
}