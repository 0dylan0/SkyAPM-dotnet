﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SkyWalking.Common
{
    internal class FastPathMatcher
    {
        public static bool Match(string pattern, string path)
        {
            if (path == null) return false;
            return NormalMatch(pattern, 0, path, 0);
        }

        private static bool NormalMatch(string pat, int p, string str, int s)
        {
            while (p < pat.Length)
            {
                char pc = pat[p];
                char sc = SafeCharAt(str, s);

                // Got * in pattern, enter the wildcard mode.
                //            ↓        ↓
                // pattern: a/*      a/*
                //            ↓        ↓
                // string:  a/bcd    a/
                if (pc == '*')
                {
                    p++;
                    // Got * in pattern again, enter the multi-wildcard mode.
                    //             ↓        ↓
                    // pattern: a/**     a/**
                    //            ↓        ↓
                    // string:  a/bcd    a/
                    if (SafeCharAt(pat, p) == '*')
                    {
                        p++;
                        // Enter the multi-wildcard mode.
                        //              ↓        ↓
                        // pattern: a/**     a/**
                        //            ↓        ↓
                        // string:  a/bcd    a/
                        return MultiWildcardMatch(pat, p, str, s);
                    }
                    else
                    {
                        // Enter the wildcard mode.
                        //             ↓
                        // pattern: a/*
                        //            ↓
                        // string:  a/bcd
                        return WildcardMatch(pat, p, str, s);
                    }
                }

                // Matching ? for non-'/' char, or matching the same chars.
                //            ↓        ↓       ↓
                // pattern: a/?/c    a/b/c    a/b
                //            ↓        ↓       ↓
                // string:  a/b/c    a/b/d    a/d
                if ((pc == '?' && sc != 0 && sc != '/') || pc == sc)
                {
                    s++;
                    p++;
                    continue;
                }

                // Not matched.
                //            ↓
                // pattern: a/b
                //            ↓
                // string:  a/c
                return false;
            }

            return s == str.Length;
        }

        private static bool WildcardMatch(string pat, int p, string str, int s)
        {
            char pc = SafeCharAt(pat, p);

            while (true)
            {
                char sc = SafeCharAt(str, s);

                if (sc == '/')
                {
                    // Both of pattern and string '/' matched, exit wildcard mode.
                    //             ↓
                    // pattern: a/*/
                    //              ↓
                    // string:  a/bc/
                    if (pc == sc)
                    {
                        return NormalMatch(pat, p + 1, str, s + 1);
                    }

                    // Not matched string in current path part.
                    //             ↓        ↓
                    // pattern: a/*      a/*d
                    //              ↓        ↓
                    // string:  a/bc/    a/bc/
                    return false;
                }

                // Try to enter normal mode, if not matched, increasing pointer of string and try again.
                if (!NormalMatch(pat, p, str, s))
                {
                    // End of string, not matched.
                    if (s >= str.Length)
                    {
                        return false;
                    }

                    s++;
                    continue;
                }

                // Matched in next normal mode.
                return true;
            }
        }

        private static bool MultiWildcardMatch(string pat, int p, string str, int s)
        {
            // End of pattern, just check the end of string is '/' quickly.
            if (p >= pat.Length && s < str.Length)
            {
                return str[str.Length - 1] != '/';
            }

            while (true)
            {
                // Try to enter normal mode, if not matched, increasing pointer of string and try again.
                if (!NormalMatch(pat, p, str, s))
                {
                    // End of string, not matched.
                    if (s >= str.Length)
                    {
                        return false;
                    }

                    s++;
                    continue;
                }

                return true;
            }
        }

        private static char SafeCharAt(string value, int index)
        {
            if (index >= value.Length)
            {
                return (char)0;
            }

            return value[index];
        }
    }
}
