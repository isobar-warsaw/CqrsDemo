﻿namespace tBlabs.Cqrs.Core.Extensions
{
    public static class StringExtension
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNotEmpty(this string str)
        {
	        str = str.Trim();

	        return !str.IsNullOrEmpty();
        }
    }
}
