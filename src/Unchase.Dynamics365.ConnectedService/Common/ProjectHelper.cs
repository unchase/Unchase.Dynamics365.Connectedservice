﻿using System;
using System.Collections.Generic;
using System.IO;
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
    }
}
