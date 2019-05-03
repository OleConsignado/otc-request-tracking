namespace Otc.RequestTracking.AspNetCore
{
    /// <summary>
    /// String manipulation Utilities
    /// </summary>
    internal static class StringUtil
    {
        /// <summary>
        /// Truncate string if value is greater than maxLength
        /// </summary>
        /// <param name="value">The string</param>
        /// <param name="maxLength">Max length</param>
        /// <param name="suffixIfTruncated">Suffix to apply when value length is greater than maxLength</param>
        /// <returns></returns>
        public static string TruncateIfLengthExceeds(string value, int maxLength, string suffixIfTruncated = " ... [TRUNCATED]")
        {
            if (value != null && value.Length > maxLength)
            {
                int newLength = maxLength - suffixIfTruncated.Length;

                if (newLength <= 0)
                {
                    value = suffixIfTruncated;
                }
                else
                {
                    value = value.Substring(0, newLength) + suffixIfTruncated;
                }
            }

            return value;
        }
    }
}
