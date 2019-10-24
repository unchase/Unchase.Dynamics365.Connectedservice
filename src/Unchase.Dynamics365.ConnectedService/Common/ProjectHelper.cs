using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Unchase.Dynamics365.ConnectedService.Common
{
    /// <summary>
    /// A utility class for working with Visual Studio project system.
    /// </summary>
    internal static class ProjectHelper
    {
        public const int VshpropidVshpropidExtObject = -2027;

        public static Project GetProject(this IVsHierarchy projectHierarchy)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var result = projectHierarchy.GetProperty(
                VSConstants.VSITEMID_ROOT,
                VshpropidVshpropidExtObject, //(int)__VSHPROPID.VSHPROPID_ExtObject,
                out var projectObject);
            ErrorHandler.ThrowOnFailure(result);
            return (Project)projectObject;
        }

        public static string GetNameSpace(this Project project)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            return project?.Properties?.Item("DefaultNamespace")?.Value.ToString();
        }

        public static string GetServiceFolderPath(this Project project, string rootFolder, string serviceName)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            string servicePath;
            try
            {
                servicePath = project?.ProjectItems?
                .Item(rootFolder)?.ProjectItems?
                .Item(serviceName)?.Properties?
                .Item("FullPath")?.Value?.ToString() ?? Path.Combine(project.Properties.Item("FullPath").Value.ToString(), rootFolder, serviceName);
            }
            catch
            {
                servicePath = Path.Combine(project.Properties.Item("FullPath").Value.ToString(), rootFolder, serviceName);
            }

            return servicePath;
        }

        public static string GetSelectedPath(DTE2 dte)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var items = (Array)dte.ToolWindows.SolutionExplorer?.SelectedItems;
            if (items == null)
                return null;

            var files = new List<string>();

            foreach (UIHierarchyItem selItem in items)
            {
                if (selItem?.Object is ProjectItem item)
                    files.Add(item.GetFilePath());
            }

            return files.Count > 0 ? string.Join(" ", files) : null;
        }

        private static string GetFilePath(this ProjectItem item)
        {
            return $"\"{item?.FileNames[1]}\""; // Indexing starts from 1
        }

        internal enum GetAssembliesFrom
        {
            All = 0,

            AddedProjects = 1,

            NotAddedProjects = 2
        }

        //ToDo: поменять на IEnumerable<Assembly> и yield return
        internal static List<Assembly> GetAssemblies(this Project project, GetAssembliesFrom getAssembliesFrom = GetAssembliesFrom.All)
        {
            var result = new List<Assembly>();
            var vsproject = project.Object as VSLangProj.VSProject;
            foreach (VSLangProj.Reference reference in vsproject?.References)
            {
                try
                {
                    Assembly assembly = null;
                    switch (getAssembliesFrom)
                    {
                        case GetAssembliesFrom.All:
                            assembly = Assembly.ReflectionOnlyLoadFrom(reference.Path);
                            result.Add(assembly);
                            break;
                        case GetAssembliesFrom.AddedProjects:
                            if (reference.SourceProject == null)
                            {
                                assembly = Assembly.ReflectionOnlyLoadFrom(reference.Path);
                                result.Add(assembly);
                            }
                            break;
                        case GetAssembliesFrom.NotAddedProjects:
                            if (reference.SourceProject != null)
                            {
                                assembly = Assembly.ReflectionOnlyLoadFrom(reference.Path);
                                result.Add(assembly);
                            }
                            break;
                        default:
                            assembly = Assembly.ReflectionOnlyLoadFrom(reference.Path);
                            result.Add(assembly);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    //ToDo: как обрабатывать?
                }
            }

            return result;
        }

        //public static List<AssemblyName> CollectSettings(this Project project, bool fromAddedProject = false)
        //{
        //    var result = new List<AssemblyName>();
        //    var vsproject = project.Object as VSLangProj.VSProject;
        //    // note: you could also try casting to VsWebSite.VSWebSite

        //    foreach (VSLangProj.Reference reference in vsproject?.References)
        //    {
        //        if (reference.SourceProject == null && !fromAddedProject)
        //        {
        //            // This is an assembly reference
        //            var fullName = GetFullName(reference);
        //            var assemblyName = new AssemblyName(fullName);
        //            result.Add(assemblyName);
        //            //yield return assemblyName;
        //        }
        //        else if (reference.SourceProject != null && fromAddedProject)
        //        {
        //            var asm = Assembly.LoadFrom(reference.Path);

        //            //Get List of Class Name
        //            Type[] types = asm.GetTypes();

        //            foreach (Type tc in types)
        //            {
        //                var ttt = tc.GetInterfaces();
        //                if (tc.GetInterfaces().Contains(typeof(ICustomizeCodeDomService)))
        //                {

        //                }

        //                if (tc.IsAbstract)
        //                {
        //                    //Response.Write("Abstract Class : " + tc.Name);
        //                }
        //                else if (tc.IsPublic)
        //                {
        //                    //Response.Write("Public Class : " + tc.Name);
        //                }
        //                else if (tc.IsSealed)
        //                {
        //                    //Response.Write("Sealed Class : " + tc.Name);
        //                }
        //            }

        //            // This is a project reference
        //            var fullName = GetFullName(reference);
        //            var assemblyName = new AssemblyName(fullName);
        //            result.Add(assemblyName);
        //        }
        //    }

        //    return result;
        //}

        //internal static string GetFullName(VSLangProj.Reference reference)
        //{
        //    return string.Format("{0},Version={1}.{2}.{3}.{4},Culture={5},PublicKeyToken={6}",
        //                    reference.Name,
        //                    reference.MajorVersion, reference.MinorVersion, reference.BuildNumber, reference.RevisionNumber,
        //                    reference.Culture.Or("neutral"),
        //                    reference.PublicKeyToken.Or("null"));
        //}

        //internal static string Or(this string text, string alternative)
        //{
        //    return string.IsNullOrWhiteSpace(text) ? alternative : text;
        //}
    }
}
