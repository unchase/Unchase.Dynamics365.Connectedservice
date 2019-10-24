using EnvDTE;
using Microsoft.VisualStudio.ConnectedServices;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Unchase.Dynamics365.ConnectedService.Common;
using Unchase.Dynamics365.Customization;
using Window = System.Windows.Window;

namespace Unchase.Dynamics365.ConnectedService.View
{
    /// <summary>
    /// Логика взаимодействия для CustomizationDynamics365Window.xaml
    /// </summary>
    public partial class CustomizationDynamics365Window : Window
    {
        private readonly Project _project;

        private readonly ConnectedServiceLogger _logger;

        public CustomizationDynamics365Window(ConnectedServiceHandlerContext context)
        {
            InitializeComponent();
            this._project = context.ProjectHierarchy.GetProject();
            this._logger = context.Logger;
            var customizeCodeDomServiceClasses = GetClassesImplementsInterface<ICustomizeCodeDomService>();
            var codeWriterFilterServiceClasses = GetClassesImplementsInterface<ICodeWriterFilterService>();
            var codeWriterMessageFilterServiceClasses = GetClassesImplementsInterface<ICodeWriterMessageFilterService>();
            var metadataProviderServiceClasses = GetClassesImplementsInterface<IMetadataProviderService>();
            var codeGenerationServiceClasses = GetClassesImplementsInterface<ICodeGenerationService>();
            var namingServiceClasses = GetClassesImplementsInterface<INamingService>();
            //
        }

        private ObservableCollection<string> GetClassesImplementsInterface<T>()
        {
            var result = new ObservableCollection<string>();
            var assemblies = this._project.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    if (!assembly.IsDynamic)
                    {
                        foreach (TypeInfo assemblyType in assembly.DefinedTypes)
                        {
                            if (assemblyType.ImplementedInterfaces.Any(i => i.FullName == typeof(T).FullName))
                            {
                                result.Add($"{assemblyType.FullName}, {assemblyType.Assembly.GetName().Name}");
                            }
                        }
                    }
                    else
                    {

                    }
                }
                catch (Exception ex)
                {

                }
            }

            //var vsproject = _project.Object as VSLangProj.VSProject;
            //foreach (CodeElement element in vsproject.Project.CodeModel.CodeElements)
            //{
            //    if (element is CodeClass myClass)
            //    {
            //        //var myClass = (EnvDTE.CodeClass)element;
            //        var interfaces = myClass.ImplementedInterfaces;
            //        // do stuff with that class here
            //    }
            //}

            return result;
        }
    }
}
