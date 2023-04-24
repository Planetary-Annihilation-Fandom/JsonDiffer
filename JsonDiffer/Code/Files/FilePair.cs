using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonDiffer.Code.Files
{
    public class FilePair
    {
        public FileIdentity? A { get; set; }
        public FileIdentity? B { get; set; }

        public FilePair(FileIdentity? a, FileIdentity? b)
        {
            A = a;
            B = b;
        }
    }
}
