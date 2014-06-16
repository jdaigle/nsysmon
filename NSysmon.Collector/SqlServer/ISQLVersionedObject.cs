using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

/// <remarks>
/// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
/// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
/// </remarks>
namespace NSysmon.Collector.SqlServer
{
    public interface ISQLVersionedObject
    {
        Version MinVersion { get; }
        string GetFetchSQL(Version v);
    }

    public static class ISQLVersionedObjectSingletons
    {
        public static Dictionary<Type, ISQLVersionedObject> VersionSingletons;

        static ISQLVersionedObjectSingletons()
        {
            VersionSingletons = new Dictionary<Type, ISQLVersionedObject>();
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(typeof(ISQLVersionedObject).IsAssignableFrom))
            {
                if (!type.IsClass)
                {
                    continue;
                }
                try
                {
                    VersionSingletons.Add(type, (ISQLVersionedObject)Activator.CreateInstance(type));
                }
                catch (Exception e)
                {
                    ExceptionManager.LogException("Error creating ISQLVersionedObject lookup for " + type, e);
                }
            }
        }

        public static bool TryGetValue(Type type, out ISQLVersionedObject lookup)
        {
            return VersionSingletons.TryGetValue(type, out lookup);
        }
    }
}
