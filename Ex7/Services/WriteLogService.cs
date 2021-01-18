using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ex7.Services
{
    public class WriteLogService : IWriteService
    {

        private string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public void Write(string[] _data)
        {
            string[] data = _data;

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "requestsLog.txt"), append: true))
            {
                foreach (string line in data)
                    outputFile.WriteLine(line);
            }
        }
    }
}
