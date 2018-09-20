using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sprung
{
    public static class MethodTimeLogger
    {
        public static void Log(MethodBase methodBase, long milliseconds, string message)
        {
            using (StreamWriter w = File.AppendText("log.txt"))
            {
                w.WriteLine($"{methodBase.Name} {milliseconds}");
            }
        }
    }
}
