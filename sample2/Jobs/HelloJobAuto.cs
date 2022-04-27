using Quartz;
using SilkierQuartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SilkierQuartz.Example.Jobs
{
 
    [SilkierQuartz(5, "this e sq test","_hellojobauto")]
    public class HelloJobAuto : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"Hello {DateTime.Now}");
            return Task.CompletedTask;
        }
    }

    [DisallowConcurrentExecution]
    [SilkierQuartz(5, 0, 0, Desciption = "自动下载欢迎信息")]
    public class HelloJobAuto1 : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"Hello {DateTime.Now}");
            return Task.CompletedTask;
        }
    }

}
