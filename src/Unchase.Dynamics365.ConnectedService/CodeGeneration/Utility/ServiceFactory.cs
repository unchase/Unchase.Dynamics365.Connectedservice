using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Unchase.Dynamics365.ConnectedService.CodeGeneration.Utility
{
    internal static class ServiceFactory
	{
        internal static async Task<TIService> CreateInstanceAsync<TIService>(TIService defaultServiceInstance, string parameterValue, CrmSvcUtilParameters parameters)
		{
			var name = typeof(TIService).Name.Substring(1);
			await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Creating instance of {0}", typeof(TIService).Name);
			var text = parameterValue;
			if (string.IsNullOrEmpty(text))
			{
				text = ConfigurationManager.AppSettings[name];
			}
			if (string.IsNullOrEmpty(text))
			{
				return defaultServiceInstance;
			}
			await CrmSvcUtil.CrmSvcUtilLogger.TraceInformationAsync("Looking for extension named {0}", text);
			var type = Type.GetType(text, false);
			if (type == null)
			{
				throw new NotSupportedException("Could not load provider of type '" + text + "'");
			}
			if (type.GetInterface(typeof(TIService).FullName) == null)
			{
				throw new NotSupportedException("Type '" + text + "'does not implement interface " + typeof(TIService).FullName);
			}
			if (type.IsAbstract)
			{
				throw new NotSupportedException("Cannot instantiate abstract type '" + text + "'.");
			}
			var constructor = type.GetConstructor(new []
			{
				typeof(TIService),
				typeof(IDictionary<string, string>)
			});
			if (constructor != null)
			{
				return (TIService)((object)constructor.Invoke(new object[]
				{
					defaultServiceInstance,
					parameters.ToDictionary()
				}));
			}
			constructor = type.GetConstructor(new []
			{
				typeof(TIService)
			});
			if (constructor != null)
			{
				return (TIService)((object)constructor.Invoke(new object[]
				{
					defaultServiceInstance
				}));
			}
			constructor = type.GetConstructor(new []
			{
				typeof(IDictionary<string, string>)
			});
			if (constructor != null)
			{
				return (TIService)((object)constructor.Invoke(new object[]
				{
					parameters.ToDictionary()
				}));
			}
			return (TIService)((object)Activator.CreateInstance(type));
		}
	}
}
