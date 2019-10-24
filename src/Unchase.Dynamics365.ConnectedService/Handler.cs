using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ConnectedServices;
using Unchase.Dynamics365.ConnectedService.CodeGeneration;
using Unchase.Dynamics365.ConnectedService.Common;
using Unchase.Dynamics365.ConnectedService.View;

namespace Unchase.Dynamics365.ConnectedService
{
    [ConnectedServiceHandlerExport(Constants.ProviderId, AppliesTo = "VB | CSharp | Web")]
    internal class Handler : ConnectedServiceHandler
    {
        public override async Task<AddServiceInstanceResult> AddServiceInstanceAsync(ConnectedServiceHandlerContext context, CancellationToken cancellationToken)
        {
            var instance = (Instance)context.ServiceInstance;
            //ToDo: раскомментировать
            //if (instance.ServiceConfig.UseGenerationCustomization)
            //    await ShowCustomizationWindowAsync(context, instance);
            //
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, $"Adding service instance for \"{instance.ServiceConfig.Endpoint}\"...");

            var codeGenDescriptor = await GenerateCodeAsync(context, instance);
            context.SetExtendedDesignerData(instance.ServiceConfig);
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, "Adding service instance complete!");
            return new AddServiceInstanceResult(context.ServiceInstance.Name, new Uri(Constants.Website));
        }

        public override async Task<UpdateServiceInstanceResult> UpdateServiceInstanceAsync(ConnectedServiceHandlerContext context, CancellationToken cancellationToken)
        {
            var instance = (Instance)context.ServiceInstance;
            //ToDo: раскомментировать
            //if (instance.ServiceConfig.UseGenerationCustomization)
            //    await ShowCustomizationWindowAsync(context, instance);
            //
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, $"Re-adding service instance for \"{instance.ServiceConfig.Endpoint}\"...");

            var codeGenDescriptor = await ReGenerateCodeAsync(context, instance);
            context.SetExtendedDesignerData(instance.ServiceConfig);
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, "Re-Adding service instance complete!");
            return await base.UpdateServiceInstanceAsync(context, cancellationToken);
        }

        private static async Task<BaseCodeGenDescriptor> GenerateCodeAsync(ConnectedServiceHandlerContext context, Instance instance)
        {
            var codeGenDescriptor = new Dynamics365CodeGenDescriptor(context, instance);
            if (instance.ServiceConfig.AddClientNuGet)
                await codeGenDescriptor.AddNugetPackagesAsync();
            await codeGenDescriptor.AddGeneratedCodeAsync();
            return codeGenDescriptor;
        }

        private static async Task<BaseCodeGenDescriptor> ReGenerateCodeAsync(ConnectedServiceHandlerContext context, Instance instance)
        {
            var codeGenDescriptor = new Dynamics365CodeGenDescriptor(context, instance);
            await codeGenDescriptor.AddGeneratedCodeAsync();
            return codeGenDescriptor;
        }

        //ToDo: доделать
        private static async Task ShowCustomizationWindowAsync(ConnectedServiceHandlerContext context, Instance instance)
        {
            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, $"Openning customization window...");
            //ToDo: доделать
            //---------------------------------------------------------------------------------------------------------------------
            //ToDo: передать в windows при создании и там уже с ним работать! =>
            //ToDo: открыть новое окно с выбором кастомизаций (там загрузить сборки (если стоит кастомизация))?
            var customizationDynamics365Window = new CustomizationDynamics365Window(context);
            if (!customizationDynamics365Window.ShowDialog().Value)
            {
                return;
            }
            else
            {

            }


            //var dict = new Dictionary<Type, (string, string)>();
            //var assemblies = project.GetAssemblies(true);
            //foreach (var assembly in assemblies)
            //{
            //    try
            //    {
            //        foreach (Type tc in assembly.GetTypes())
            //        {
            //            if (tc.GetInterfaces().Contains(typeof(ICustomizeCodeDomService)))
            //            {
            //                dict.Add(typeof(ICustomizeCodeDomService), (tc.FullName, tc.Name));
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}
            //---------------------------------------------------------------------------------------------------------------------

            await context.Logger.WriteMessageAsync(LoggerMessageCategory.Information, $"Openning customization window complete!");
        }
    }
}
