using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WndTop.Setup
{
    class Program
    {
        static void Main(string[] args)
        {
            string src = @"..\..\..\WndTop\bin\Release";
            string dest = @"..\..\InnoSetup\WndTop";
            EmptyAndCopy(src, dest);


            Console.WriteLine("Done");
            Console.ReadKey();
        }

        private static void EmptyAndCopy(string srcServer, string destServer)
        {
            EmptyDir(destServer);
            CopyExeDll(srcServer, destServer);
        }

        private static void EmptyDir(string dir)
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
            Directory.CreateDirectory(dir);
        }
        private static void CopyExeDll(string srcDir, string destDir)
        {
            foreach (var item in Directory.GetFiles(srcDir, "*.*", SearchOption.AllDirectories).Where(a => !a.Contains(".vshost.") && a.EndsWith(".exe") || a.EndsWith(".dll")))
            {
                var destfn = destDir + item.Substring(srcDir.Length);
                var dir = new FileInfo(destfn).Directory;
                if (!dir.Exists)
                {
                    dir.Create();
                }
                File.Copy(item, destDir + item.Substring(srcDir.Length));
            }
        }
    }
}
