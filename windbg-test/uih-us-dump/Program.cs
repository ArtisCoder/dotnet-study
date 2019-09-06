using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace uih_us_dump
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("start...");

            new AppMonitor("UIH_US").Start();
        }
    }

    class AppMonitor
    {
        private readonly string _appName;

        private long MEMORY_1G = 1024 * 1024 * 1024;

        //程序使用内存的阈值，物理内存+虚拟内存
        private long MAX_MEMORY_G = 4;

        private long MAX_COLLECT_COUNT = 3;

        //检测程序的间隔
        private int MONITOR_MILISECONDS_INTERVAL = 1000;

        //收集完成后间隔
        private int COLLECT_MILISECONDS_INTERVAL = 10 * 1000;

        public AppMonitor(string appName)
        {
            _appName = appName;
        }

        public void Start()
        {
            int collectCt = 0;

            while (true)
            {
                var process = Process.GetProcessesByName(_appName).FirstOrDefault();
                if (null == process)
                    continue;

                Thread.Sleep(MONITOR_MILISECONDS_INTERVAL);

                var detail = GetProcessOccupyMemory(process);

                try
                {
                    if ((detail.WorkingSet64 + detail.VirtualMemorySize64) / MEMORY_1G > MAX_MEMORY_G)
                    {
                        Log("开始写内存");
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = "procdump64.exe"; //启动的应用程序名称
                        startInfo.Arguments = "-ma " + _appName;
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        var childProcess = Process.Start(startInfo);
                        childProcess.WaitForExit();
                        Log("写内存完成");
                        collectCt++;

                        Thread.Sleep(COLLECT_MILISECONDS_INTERVAL);
                    }
                }
                catch (Exception ex)
                {
                    Log(ex.Message + ex.StackTrace);
                }

                if (collectCt > MAX_COLLECT_COUNT)
                    break;
            }
        }

        public class ProcessMemDetail
        {
            public long WorkingSet64 { get; set; }
            public long VirtualMemorySize64 { get; set; }
            public long PeakPagedMemorySize64 { get; set; }
            public long PeakVirtualMemorySize64 { get; set; }
            public long PeakWorkingSet64 { get; set; }
        }

        private ProcessMemDetail GetProcessOccupyMemory(Process process)
        {
            if (null == process || process.HasExited)
                return null;

            var detail = new ProcessMemDetail();
            detail.WorkingSet64 = process.WorkingSet64;
            detail.VirtualMemorySize64 = process.VirtualMemorySize64;
            detail.PeakPagedMemorySize64 = process.PeakPagedMemorySize64;
            detail.PeakVirtualMemorySize64 = process.PeakVirtualMemorySize64;
            detail.PeakWorkingSet64 = process.PeakWorkingSet64;
            return detail;
        }

        private void Log(string msg, params object[] args)
        {
            Console.WriteLine(msg, args);

            using (var fs = new FileStream("dump.log", FileMode.Append, FileAccess.Write))
            using (var sw = new StreamWriter(fs))
            {
                sw.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")+ msg, args);
            }
        }
    }
}
