using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automata.IO;

namespace JsonDiffer.Code
{
    public class FileExplorer
    {
        public List<string> Extensions { get; private set; }

        public FileExplorer()
        {
            Extensions = new List<string>();
        }

        public async Task<LinkedList<IFile>> Explore(IDirectory root)
        {
            var col = new LinkedList<IFile>();
            await foreach (var file in root.EnumerateFilesDeep())
            {
                if(!IsAllowed(file))
                    continue;

                col.AddLast(file);
            }

            return col;
        }

        public void AddExtension(string extension)
        {
            if (!extension.StartsWith('.'))
            {
                extension = extension.Insert(0, ".");
            }

            if (Extensions.Contains(extension))
                return;

            Extensions.Add(extension);
        }

        private bool IsAllowed(IFile file)
        {
            var ext = file.Extension();
            if (Extensions.Contains(ext))
                return true;
            return false;
        }
    }
}
