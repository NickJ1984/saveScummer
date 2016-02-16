using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IOlib.FileSystem.Objects;
using IOlib.FileSystem.Service;

namespace testConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            fsFolder folder = new fsFolder(@"E:\Games\RogueLike\Cataclysm\BUILDS");
            folder.read();
            string firstID = folder.ID;
            Console.WriteLine("First object ID: {0}", firstID);
            var array = serializator.serialize(folder);
            Console.WriteLine("Files:");
            for (int i = 0; i < folder.fileCount; i++)
                Console.WriteLine("{0}. {1}", i, folder[i].name);
            Console.WriteLine(new string('*', 50));

            folder = null;
            folder = serializator.deserialize<fsFolder>(array);

            string secondID = folder.ID;
            Console.WriteLine("Second object ID: {0}", secondID);
            Console.WriteLine("Files:");
            for (int i = 0; i < folder.fileCount; i++)
                Console.WriteLine("{0}. {1}", i, folder[i].name);
            Console.WriteLine(new string('*', 50));
            Console.WriteLine("Objects equality: {0}", firstID == secondID);
            #region break
            Console.WriteLine("End.");
            Console.ReadLine();
            #endregion
        }
    }
}
