using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Management;

namespace MyWindowsInfo
{
    class Program
    {

           static void Main(string[] args)
        {
            string result = string.Empty;
            if (args.Length > 0)
            {
                string type = args[0];
                switch (type)
                {
                    case "cpu":
                        result = getCpuRate();
                        break;
                    case "mem":
                        result = getMemRate();
                        break;
                    case "disk":
                        result = getDiskRate();
                        break;
                    default:
                        result = InvokeCmd(type);
                        break;
                }
            }
            Console.WriteLine(result);
        }

        private static string InvokeCmd(string cmdArgs)
        {
            string Tstr = "";
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            p.StandardInput.WriteLine(cmdArgs);
            p.StandardInput.WriteLine("exit");
            Tstr = p.StandardOutput.ReadToEnd();
            p.Close();
            return Tstr;
        }

        /// <summary>
        /// 取得cpu使用率
        /// </summary>
        /// <returns></returns>
        private static string getCpuRate()
        {
            PerformanceCounter cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpu.NextValue();
            System.Threading.Thread.Sleep(1000);
            return string.Format("{0:0.00}", cpu.NextValue());
        }

        /// <summary>
        /// 内存利用率
        /// </summary>
        /// <returns></returns>
        private static string getMemRate()
        {
            MEMORY_INFO MemInfo;
            MemInfo = new MEMORY_INFO();
            GlobalMemoryStatus(ref MemInfo);
            return string.Format("{0:0.00}", MemInfo.dwMemoryLoad.ToString());
        }

        /// <summary>
        /// 取得磁盘利用率
        /// </summary>
        /// <returns></returns>
        private static string getDiskRate()
        {
            string result = string.Empty;
            string str = string.Empty;
            ManagementClass diskClass = new ManagementClass("Win32_LogicalDisk");
            ManagementObjectCollection disks = diskClass.GetInstances();
            foreach (ManagementObject disk in disks)
            {
                try
                {                
                    // 磁盘总容量，可用空间，已用空间
                    if (Convert.ToInt64(disk["Size"]) > 0)
                    {
                        long totalSpace = Convert.ToInt64(disk["Size"]);
                        long freeSpace = Convert.ToInt64(disk["FreeSpace"]);
                        long usedSpace = totalSpace - freeSpace;
                        str = string.Format("{0}={1:0.00}%", disk["Name"].ToString(), usedSpace * 100.0 / totalSpace);
                        if (result.Length == 0)
                            result = str;
                        else result = string.Format("{0}#{1}", result, str);

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return result;
        }

        [DllImport("kernel32")]
        public static extern void GetSystemDirectory(StringBuilder SysDir, int count);
        [DllImport("kernel32")]
        public static extern void GetSystemInfo(ref CPU_INFO cpuinfo);
        [DllImport("kernel32")]
        public static extern void GlobalMemoryStatus(ref MEMORY_INFO meminfo);
        [DllImport("kernel32")]
        public static extern void GetSystemTime(ref SYSTEMTIME_INFO stinfo);
        
    }
    //定义CPU的信息结构    
    [StructLayout(LayoutKind.Sequential)]
    public struct CPU_INFO
    {
        public uint dwOemId;
        public uint dwPageSize;
        public uint lpMinimumApplicationAddress;
        public uint lpMaximumApplicationAddress;
        public uint dwActiveProcessorMask;
        public uint dwNumberOfProcessors;
        public uint dwProcessorType;
        public uint dwAllocationGranularity;
        public uint dwProcessorLevel;
        public uint dwProcessorRevision;
    }
    //定义内存的信息结构    
    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_INFO
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public uint dwTotalPhys;
        public uint dwAvailPhys;
        public uint dwTotalPageFile;
        public uint dwAvailPageFile;
        public uint dwTotalVirtual;
        public uint dwAvailVirtual;
    }
    //定义系统时间的信息结构    
    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME_INFO
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;
    }



}  
