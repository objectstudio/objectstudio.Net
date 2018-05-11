using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace objectstudio.Net.Core
{
	/// <summary>
	/// Contains the success/failure status of an operation.
	/// </summary>
	public class Result
	{		
		/// <summary>
		/// Whether the operation's result was successful.
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// An error code that can be used to identify the error and potentially process the error programatically.
		/// </summary>
		public string Code { get; set; }

#if !DEBUG
        [JsonIgnore]
#endif
		/// <summary>
		/// The Exception that generated the error (if any).
		/// </summary>
		public Exception Exception
		{
			get { return _Exception; }
			set { _Exception = value; if (value != null) { Success = false; Code = Codes.UNEXPECTED_ERROR; } }
		}
		private Exception _Exception;

		/// <summary>
		/// Any additional messages to be communicated to the user about the error.
		/// </summary>
		public List<string> Messages { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Result"/> class.
		/// </summary>
		public Result()
		{
			this.Messages = new List<string>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Result"/> class.
		/// </summary>
		public Result(bool success)
		{
			this.Success = success;
			this.Messages = new List<string>();
		}

		/// <summary>
		/// Sets Success to false and records the specified error.
		/// </summary>
		/// <param name="exc">The exc.</param>
		/// <param name="caller">The object calling this method. Passing this paramater will enable automatic logging.</param>
		/// <param name="description">A description for use in logging if enabled.</param>
		/// <returns></returns>
		public virtual Result AddException(Exception exc, object caller = null, string description = null)
		{
			this.Exception = exc;

			if (caller != null) return this.WithLog(caller, description);
			return this;
		}

		/// <summary>
		/// Sets Success to false and records the provided validation failure message.
		/// </summary>
		/// <param name="failureMessage">The failure message.</param>
		/// <param name="caller">The object calling this method. Passing this paramater will enable automatic logging.</param>
		/// <param name="description">A description for use in logging if enabled.</param>
		/// <returns></returns>
		public virtual Result AddValidationFailure(string failureMessage, object caller = null, string description = null)
		{
			this.Success = false;
			this.Code = Codes.VALIDATION_FAILURE;
			this.Messages.Add(failureMessage);

			if (caller != null) return this.WithLog(caller, description, failureMessage);
			return this;
		}

		/// <summary>
		/// Sets Success to false and records the provided failure code and message.
		/// </summary>
		/// <param name="failureCode">The failure code.</param>
		/// <param name="failureMessage">The failure message.</param>
		/// <param name="caller">The object calling this method. Passing this paramater will enable automatic logging.</param>
		/// <param name="description">A description for use in logging if enabled.</param>
		/// <returns></returns>
		public virtual Result AddFailure(string failureCode, string failureMessage = null, object caller = null, string description = null)
		{
			this.Success = false;
			this.Code = failureCode;
			if (failureMessage.HasValue()) this.Messages.Add(failureMessage);

			if (caller != null) return this.WithLog(caller, description, failureMessage);
			return this;
		}

		/// <summary>
		/// Adds a generic message to the messages list.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="caller">The object calling this method. Passing this paramater will enable automatic logging.</param>
		/// <param name="description">A description for use in logging if enabled.</param>
		/// <returns></returns>
		public virtual Result AddMessage(string message, object caller = null, string description = null)
		{
			if (!string.IsNullOrWhiteSpace(message)) this.Messages.Add(message);

			if (caller != null) return this.WithLog(caller, description, message);
			return this;
		}

		/// <summary>
		/// Implements the operator +.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static Result operator +(Result left, Result right)
		{
			left.Success = right.Success;
			left.Code = right.Code;
			left.Exception = right.Exception;
			left.Messages.AddRange(right.Messages);
			return left;
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			if (this.Success) return "SUCCESS";

			string message = "FAIL";
			if (!String.IsNullOrEmpty(this.Code)) message += String.Format(" ({0})", this.Code);
			if (this.Exception != null) message += String.Format(" {0}.", this.Exception.Message);
			if (this.Messages != null && this.Messages.Count > 0) message += " " + String.Join(".", this.Messages.ToArray());
			return message;
		}

		/// <summary>
		/// Log this result object. This method is chainable.
		/// </summary>
		/// <param name="caller"></param>
		/// <param name="description"></param>
		/// <param name="message">If not specified, will log all messages in this result object.</param>
		/// <returns></returns>
		public Result WithLog(object caller, string description = null, string message = null)
		{
			return this.WithLog(caller.GetType(), description, message);
		}

		/// <summary>
		/// Log this result object. This method is chainable.        
		/// </summary>
		/// <typeparam name="Tc">The caller's class.</typeparam>
		/// <param name="description"></param>
		/// <param name="message">If not specified, will log all messages in this result object.</param>
		/// <returns></returns>
		public Result WithLog<Tc>(string description = null, string message = null)
		{
			return this.WithLog(typeof(Tc), description, message);
		}

		/// <summary>
		/// Log this result object. This method is chainable.
		/// </summary>
		/// <param name="callerType">The type of the caller.</param>
		/// <param name="description"></param>
		/// <param name="message">If not specified, will log all messages in this result object.</param>
		/// <returns></returns>
		public Result WithLog(Type callerType, string description = null, string message = null)
		{
			if (this.Success)
			{
				if (message.HasValue()) LoggingExtensions.LogForType(callerType).Info(message);
				if (description.HasValue()) LoggingExtensions.LogForType(callerType).Info(description);
				if (!message.HasValue()) foreach (string m in this.Messages) LoggingExtensions.LogForType(callerType).Info(m);
			}
			else
			{
				if (message.HasValue()) LoggingExtensions.LogForType(callerType).Error(message);
				if (this.Code.HasValue() && description.HasValue()) LoggingExtensions.LogForType(callerType).Error($"{this.Code} {description}");
				else if (this.Code.HasValue()) LoggingExtensions.LogForType(callerType).Error(this.Code);
				else if (description.HasValue()) LoggingExtensions.LogForType(callerType).Error(description);
				if (!message.HasValue()) foreach (string m in this.Messages) LoggingExtensions.LogForType(callerType).Error(m);
				if (this.Exception != null) LoggingExtensions.LogForType(callerType).Error("Exception: ", this.Exception);
			}

			return this;
		}

		/// <summary>
		/// A listing of common error codes.
		/// </summary>
		public class Codes
		{
			/// <summary>
			/// A general error code.
			/// </summary>
			public const string GENERAL_ERROR = "ERROR";

			/// <summary>
			/// A general error code for exceptions.
			/// </summary>
			public const string UNEXPECTED_ERROR = "UNEXPECTED_ERROR";

			/// <summary>
			/// The user must log in to access the requested resource.
			/// </summary>
			public const string AUTHENTICATION_REQUIRED = "AUTHENTICATION_REQUIRED";

			/// <summary>
			/// The user does not have permission to the requested resource.
			/// </summary>
			public const string NOT_AUTHORIZED = "NOT_AUTHORIZED";

			/// <summary>
			/// A general error when a request could not be read due to formatting issues.
			/// </summary>
			public const string INVALID_REQUEST_FORMAT = "INVALID_REQUEST_FORMAT";

			/// <summary>
			/// A generel error when the provided data is missing required fields or not provided at all.
			/// </summary>
			public const string MISSING_REQUIRED_FIELDS = "MISSING_REQUIRED_FIELDS";

			/// <summary>
			/// A generel error when the provided data does not pass validation checks.
			/// </summary>
			public const string VALIDATION_FAILURE = "VALIDATION_FAILURE";

			/// <summary>
			/// A generel error when the requested object was not found.
			/// </summary>
			public const string NOT_FOUND = "NOT_FOUND";

			/// <summary>
			/// The feature requested is not implemented.
			/// </summary>
			public const string NOT_IMPLEMENTED = "NOT_IMPLEMENTED";
		}
	}

	/// <summary>
	/// Contains the success/failure status of an operation and the associated output value.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Result<T> : Result
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Result{T}"/> class.
		/// </summary>
		public Result()
			: base()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Result{T}"/> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public Result(T value)
			: base()
		{
			this.Value = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Result{T}"/> class.
		/// </summary>
		/// <param name="code">The code.</param>
		/// <param name="value">The value.</param>
		public Result(string code, T value)
		{
			this.Code = code;
			if (value != null)
				this.Value = value;
		}

		/// <summary>
		/// The output value of the operation.
		/// </summary>
		public T Value
		{
			get { return _Value; }
			set { _Value = value; if (value != null) Success = true; }
		}
		private T _Value;

		/// <summary>
		/// Allows concatenating 2 result.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static Result<T> operator +(Result<T> left, Result right)
		{
			left.Success = right.Success;
			left.Code = right.Code;
			left.Exception = right.Exception;
			left.Messages.AddRange(right.Messages);
			return left;
		}

		/// <summary>
		/// Allows concatenating 2 result.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static Result<T> operator +(Result<T> left, Result<T> right)
		{
			left.Success = right.Success;
			left.Code = right.Code;
			left.Exception = right.Exception;
			left.Messages.AddRange(right.Messages);
			left.Value = right.Value;
			return left;
		}
	}
}
