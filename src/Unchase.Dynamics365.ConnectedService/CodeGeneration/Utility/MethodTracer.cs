using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
    internal class MethodTracer
    {
        internal async Task EnterAsync()
		{
            if (CrmSvcUtil.EnableDebugMode)
            {
                var stackTrace = new StackTrace();
                await LogMessageAsync("Entering {0}", stackTrace.GetFrame(Math.Min(1, stackTrace.FrameCount - 1)).GetMethod());
            }
		}

		internal async Task ExitAsync()
		{
            if(CrmSvcUtil.EnableDebugMode)
            {
                var stackTrace = new StackTrace();
                await LogMessageAsync("Exiting {0}", stackTrace.GetFrame(Math.Min(1, stackTrace.FrameCount - 1)).GetMethod());
            }
		}

		internal async Task LogMessageAsync(string message)
		{
            if (CrmSvcUtil.EnableDebugMode)
            {
                var stackTrace = new StackTrace();
                await LogMessageAsync(message, stackTrace.GetFrame(Math.Min(1, stackTrace.FrameCount - 1)).GetMethod());
            }
		}

		internal async Task LogWarningAsync(string message)
		{
            var stackTrace = new StackTrace();
			await LogWarningAsync(message, stackTrace.GetFrame(Math.Min(1, stackTrace.FrameCount - 1)).GetMethod());
		}

		private async Task LogMessageAsync(string message, MethodBase method)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync(message, MethodTracer.GetMethodString(method));
        }

		private async Task LogWarningAsync(string message, MethodBase method)
		{
            await CrmSvcUtil.CrmSvcUtilLogger.TraceWarningAsync(message, MethodTracer.GetMethodString(method));
            await CrmSvcUtil.CrmSvcUtilLogger.LogAsync(string.Format(message, MethodTracer.GetMethodString(method)), LoggerMessageCategory.Warning);
        }

		private static string GetMethodString(MethodBase method)
		{
			return $"{method.DeclaringType?.Name}.{method.Name}";
		}
	}
}
