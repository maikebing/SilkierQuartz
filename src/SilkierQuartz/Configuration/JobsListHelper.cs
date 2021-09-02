using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SilkierQuartz.Configuration
{
    internal static class JobsListHelper
    {
        private static List<Type> _silkierQuartzJobs = null;
        public static List<Type> GetSilkierQuartzJobs(List<Assembly> lists = null)
        {
            if (_silkierQuartzJobs == null)
            {
                try
                {
                    var types1 = from t in Assembly.GetEntryAssembly().GetTypes() where t.GetTypeInfo().ImplementedInterfaces.Any(tx => tx == typeof(IJob)) && t.GetTypeInfo().IsDefined(typeof(SilkierQuartzAttribute), true) select t;
                    var types = from t in Assembly.GetCallingAssembly().GetTypes() where t.GetTypeInfo().ImplementedInterfaces.Any(tx => tx == typeof(IJob)) && t.GetTypeInfo().IsDefined(typeof(SilkierQuartzAttribute), true) select t;
                    _silkierQuartzJobs = new List<Type>();
                    _silkierQuartzJobs.AddRange(types.ToList());
                    _silkierQuartzJobs.AddRange(types1.ToList());
                    lists?.ForEach(asm =>
                    {
                        var typeasm = from t in asm.GetTypes() where t.GetTypeInfo().ImplementedInterfaces.Any(tx => tx == typeof(IJob)) && t.GetTypeInfo().IsDefined(typeof(SilkierQuartzAttribute), true) select t;
                        _silkierQuartzJobs.AddRange(typeasm);
                    });
                }
                catch (Exception ex)
                {
                    throw new Exception("Can't  find  type with  IJob and have  SilkierQuartzAttribute", ex);
                }
            }
            return _silkierQuartzJobs;
        }
    }
}
