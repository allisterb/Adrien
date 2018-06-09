﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using Serilog;
using SerilogTimings;
using SerilogTimings.Extensions;
namespace Adrien
{
    public abstract class CompilerApi<T>
    {
        #region Properties
        static DirectoryInfo AssemblyDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
        static Version AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
        public static ILogger L { get; } = Log.ForContext<T>();
        public static Dictionary<string, object> CompilerApiOptions { get; } = new Dictionary<string, object>();
        #endregion

        #region Methods
        [DebuggerStepThrough] protected virtual void Info(string messageTemplate, params object[] propertyValues) => L.Information(messageTemplate, propertyValues);
        [DebuggerStepThrough] protected virtual void Debug(string messageTemplate, params object[] propertyValues) => L.Debug(messageTemplate, propertyValues);
        [DebuggerStepThrough] protected virtual void Error(string messageTemplate, params object[] propertyValues) => L.Error(messageTemplate, propertyValues);
        [DebuggerStepThrough] protected virtual void Error(Exception e, string messageTemplate, params object[] propertyValues) => L.Error(e, messageTemplate, propertyValues);
        [DebuggerStepThrough] protected virtual void Verbose(string messageTemplate, params object[] propertyValues) => L.Verbose(messageTemplate, propertyValues);
        [DebuggerStepThrough] protected virtual void Warn(string messageTemplate, params object[] propertyValues) => L.Warning(messageTemplate, propertyValues);

        [DebuggerStepThrough]
        public virtual Operation Begin(string messageTemplate, params object[] args)
        {
            Info(messageTemplate + "...", args);
            return L.BeginOperation(messageTemplate, args);
        }

        protected static void SetPropFromDict(Type t, object o, Dictionary<string, object> p)
        {
            foreach (PropertyInfo prop in t.GetProperties())
            {
                if (p.ContainsKey(prop.Name) && prop.PropertyType == p[prop.Name].GetType())
                {
                    prop.SetValue(o, p[prop.Name]);
                }
            }
        }

        protected string GetAssemblyDirectoryFullPath(string path) => Path.Combine(AssemblyDirectory.FullName, path);
        #endregion
    }
}
