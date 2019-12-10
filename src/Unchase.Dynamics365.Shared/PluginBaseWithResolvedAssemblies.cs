using Microsoft.Xrm.Sdk;
using System;
using System.Globalization;
using System.Reflection;

namespace Unchase.Dynamics365.Shared
{
    /// <summary>
    /// Базовый класс подключаемых модулей с внедрёнными в файл плагина DLL-библиотеками.
    /// </summary>
    /// <remarks>
    /// <para>1. Выгрузить проект. Кликнуть правой кнопкой по названию проекта и выбрать пункт "Unload Project".</para>
    /// <para>2. Открыть файл проекта для редактирования. Кликнуть правой кнопкой по названию проекта и выбрать пункт "Edit ... .csproj".</para>
    /// <para>3. В самый конец файла, перед закрывающимся тегом Project добавить следующий код (с угловыми скобками вместо фигурных):
    ///
    /// <para>
    /// {Target Name="AfterResolveReferences"}
    ///     {ItemGroup}
    ///         {EmbeddedResource Include = "@(ReferenceCopyLocalPaths)" Condition="'%(ReferenceCopyLocalPaths.Extension)' == '.dll'"}
    ///             {LogicalName}%(ReferenceCopyLocalPaths.DestinationSubDirectory)%(ReferenceCopyLocalPaths.Filename)%(ReferenceCopyLocalPaths.Extension){/LogicalName}
    ///         {/EmbeddedResource}
    ///     {/ItemGroup}
    /// {/Target}
    /// </para>
    ///
    /// </para>
    /// <para>4. Загрузить проект. Кликнуть правой кнопкой по названию проекта и выбрать пункт "Reload Project".</para>
    /// <para>5. Для всех библиотек, которые необходимо всклочить в сборку плагина необходимо установить в настройках свойство Copy Local в True.</para>
    /// </remarks>
    /// <seealso cref="http://www.pzone.ru/mscrm/plugin-dll-custom-merge/"/>
    public abstract class PluginBaseWithResolvedAssemblies : IPlugin
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns>
        /// Возвращает сборку <see cref="Assembly"/>.
        /// </returns>
        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var assemblyName = new AssemblyName(args.Name);
            var path = $"{assemblyName.Name}.dll";
            if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
                path = $"{assemblyName.CultureInfo}\\{path}"; //string.Format(@"{0}\{1}", assemblyName.CultureInfo, path);

            using (var stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null) return null;
                var assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return Assembly.Load(assemblyRawBytes);
            }
        }


        /// <summary>
        /// Статический конструктор класса <see cref="PluginBaseWithResolvedAssemblies"/>.
        /// </summary>
        static PluginBaseWithResolvedAssemblies()
        {
            //ToDo: раскомментировать, когда заработает
            //AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
        }


        /// <summary>
        /// Стандартный метод запуска основной бизнес-логики подключаемого модуля.
        /// </summary>
        /// <param name="serviceProvider">Провайдер контекста выполенения подключаемого модуля.</param>
        public abstract void Execute(IServiceProvider serviceProvider);
    }
}
