using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
	/// <summary>
	/// Trace Logger for this project
	/// </summary>
    internal class TraceLogger
	{
		/// <summary>
		/// Last Error from CRM.
		/// </summary>
        public string LastError => this._lastError.ToString();

        /// <summary>
		/// Last Exception from CRM.
		/// </summary>
        public Exception LastException => this._lastException;

        private readonly ConnectedServiceLogger _serviceLogger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceLogger">service logger</param>
        public TraceLogger(ConnectedServiceLogger serviceLogger)
        {
            this._serviceLogger = serviceLogger;
        }

		/// <summary>
		/// Last error reset.
		/// </summary>
        public void ResetLastError()
		{
			this._lastError.Remove(0, this.LastError.Length);
			this._lastException = null;
		}

		public async Task TraceInformationAsync(string message, params object[] messageData)
		{
            if (CrmSvcUtil.EnableDebugMode)
			    await this.LogAsync(string.Format(CultureInfo.CurrentUICulture, message, messageData));
		}

		public async Task TraceWarningAsync(string message, params object[] messageData)
		{
			await this.LogAsync(string.Format(CultureInfo.CurrentUICulture, message, messageData), LoggerMessageCategory.Warning);
		}

		public async Task TraceErrorAsync(string message, params object[] messageData)
		{
			await this.LogAsync(string.Format(CultureInfo.CurrentUICulture, message, messageData), LoggerMessageCategory.Error);
		}

		public async Task TraceMethodStartAsync(string message, params object[] messageData)
		{
            if (CrmSvcUtil.EnableDebugMode)
                await this.LogAsync(string.Format(CultureInfo.CurrentUICulture, message, messageData), LoggerMessageCategory.Debug);
		}

		public async Task TraceMethodStopAsync(string message, params object[] messageData)
		{
            if (CrmSvcUtil.EnableDebugMode)
                await this.LogAsync(string.Format(CultureInfo.CurrentUICulture, message, messageData), LoggerMessageCategory.Debug);
		}

		/// <summary>
		/// Log a Message.
		/// </summary>
		/// <param name="message"></param>
        public async Task LogAsync(string message)
		{
            await this._serviceLogger.WriteMessageAsync(LoggerMessageCategory.Information, message);
        }

        /// <summary>
        /// Log a Trace event.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="loggerMessageCategory"></param>
        public async Task LogAsync(string message, LoggerMessageCategory loggerMessageCategory)
        {
            await this._serviceLogger.WriteMessageAsync(loggerMessageCategory, message);
            if (loggerMessageCategory == LoggerMessageCategory.Error)
			{
				this._lastError.Append(message);
			}
        }

        /// <summary>
        /// Log a Trace event.
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="loggerMessageCategory">Trace Event type Information</param>
        /// <param name="exception">Exception object</param>
        public async Task LogAsync(string message, LoggerMessageCategory loggerMessageCategory, Exception exception)
		{
            var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Message: " + message);
			TraceLogger.LogExceptionToFile(exception, stringBuilder, 0);
			if (stringBuilder.Length > 0)
			{
                await this._serviceLogger.WriteMessageAsync(LoggerMessageCategory.Error, stringBuilder.ToString());
            }
			else
			{
                await this._serviceLogger.WriteMessageAsync(loggerMessageCategory, stringBuilder.ToString());
            }
			if (loggerMessageCategory == LoggerMessageCategory.Error)
			{
				this._lastError.Append(stringBuilder);
				this._lastException = exception;
			}
		}

		/// <summary>
		/// Log an error with an Exception.
		/// </summary>
		/// <param name="exception"></param>
        public async Task LogAsync(Exception exception)
		{
            var stringBuilder = new StringBuilder();
			TraceLogger.LogExceptionToFile(exception, stringBuilder, 0);
			if (stringBuilder.Length > 0)
            {
                await this._serviceLogger.WriteMessageAsync(LoggerMessageCategory.Error, stringBuilder.ToString());
            }
			this._lastError.Append(stringBuilder);
			this._lastException = exception;
		}

		/// <summary>
		/// Logs the error text to the stream.
		/// </summary>
		/// <param name="objException">Exception to be written.</param>
		/// <param name="sw">Stream writer to use to write the exception.</param>
		/// <param name="level">level of the exception, this deals with inner exceptions.</param>
        private static void LogExceptionToFile(Exception objException, StringBuilder sw, int level)
		{
			if (level != 0)
			{
				sw.AppendLine(string.Format(CultureInfo.InvariantCulture, "Inner Exception Level {0}\t: ", level));
			}
			sw.AppendLine("Source\t: " + ((objException.Source != null) ? objException.Source.ToString().Trim() : "Not Provided"));
			sw.AppendLine("Method\t: " + ((objException.TargetSite != null) ? objException.TargetSite.Name.ToString() : "Not Provided"));
			sw.AppendLine("Date\t: " + DateTime.Now.ToLongTimeString());
			sw.AppendLine("Time\t: " + DateTime.Now.ToShortDateString());
			sw.AppendLine("Error\t: " + (string.IsNullOrEmpty(objException.Message) ? "Not Provided" : objException.Message.ToString().Trim()));
			sw.AppendLine("Stack Trace\t: " + (string.IsNullOrEmpty(objException.StackTrace) ? "Not Provided" : objException.StackTrace.ToString().Trim()));
			sw.AppendLine("======================================================================================================================");
			level++;
			if (objException.InnerException != null)
			{
                TraceLogger.LogExceptionToFile(objException.InnerException, sw, level);
			}
		}

        /// <summary>
		/// String Builder Info.
		/// </summary>
        private readonly StringBuilder _lastError = new StringBuilder();

		/// <summary>
		/// Last Exception.
		/// </summary>
        private Exception _lastException;
    }
}
