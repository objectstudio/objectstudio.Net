using System;
using System.Collections.Generic;
using System.Text;

namespace objectstudio.Net.Core
{
	/// <summary>
	/// Extensions for coding convenience. 
	/// </summary>
    public static class ConvenienceExtensions
    {
		/// <summary>
		/// Returns true when the object is not null.
		/// </summary>
		/// <param name="candidate"></param>
		/// <returns></returns>
		public static bool HasValue(this object candidate)
		{
			if (candidate != null) return true;
			return false;
		}

		/// <summary>
		/// Returns true when the object is null.
		/// </summary>
		/// <param name="candidate"></param>
		/// <returns></returns>
		public static bool IsNull(this object candidate)
		{
			return !candidate.HasValue();
		}

		/// <summary>
		/// Returns true when the string is not null and not empty.
		/// (Internally uses String.IsNullOrWhiteSpace).
		/// </summary>
		/// <param name="stringCandidate"></param>
		/// <returns></returns>
		public static bool HasValue(this string stringCandidate)
		{
			return !String.IsNullOrWhiteSpace(stringCandidate);
		}

		/// <summary>
		/// Returns true when the string is null or empty.
		/// (Internally uses String.IsNullOrWhiteSpace).
		/// </summary>
		/// <param name="stringCandidate"></param>
		/// <returns></returns>
		public static bool IsEmpty(this string stringCandidate)
		{
			return !stringCandidate.HasValue();
		}
	}
}
