using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace objectstudio.Net.Core
{
	/// <summary>
	/// Extensions to support convenient access to the logging framework.
	/// Currently dependent on log4net but could work with any framework
	/// providing an ILog interface can be exposed.
	/// </summary>
	public static class LoggingExtensions
	{
		/// <summary>
		/// Log extension method that will return an ILog instance for the calling type.
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static ILog Log(this Object o)
		{
			return LogForType(o.GetType());
		}

		/// <summary>
		/// Log method that can be used in static methods.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static ILog Log<T>()
		{
			Type type = typeof(T);

			return LogForType(type);
		}

		/// <summary>
		/// Get the log for a certain type.
		/// This method is generally not necessary as Log[T]() is preferred. Only required for logging in
		/// static types (ie mostly other extension methods).        
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static ILog LogForType(Type t)
		{
			// Name loggers no longer support with .NET Standard version
			// Use type name for now until better solution found (if needed)
			// BP 05/07/18
			
			/*
			string logger = t.FullName;

			if (t.IsGenericType)
			{
				string genericNames = String.Empty;
				foreach (var gt in t.GenericTypeArguments) genericNames += ", " + gt.FullName;
				if (genericNames.Length > 2) genericNames = genericNames.Remove(0, 2);
				logger = t.Name + "[" + genericNames + "]";
			}
			*/

			return log4net.LogManager.GetLogger(t);
		}

		/// <summary>
		/// Handle and log exceptions that occur in async tasks.
		/// </summary>
		/// <param name="task"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public static Task Log(this Task task, string message = "")
		{
			if (task.IsCompleted)
			{
				if (task.IsFaulted)
				{
					var ex = task.Exception;
					LoggingExtensions.Log<Task>().Error(message, ex);
				}
				return task;
			}

			task.ContinueWith(t =>
			{
				var ex = t.Exception;
				LoggingExtensions.Log<Task>().Error(message, ex);
			}, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

			return task;
		}
	}
}
