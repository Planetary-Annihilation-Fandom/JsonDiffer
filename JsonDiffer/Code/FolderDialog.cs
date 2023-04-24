using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automata.IO;
using Ookii.Dialogs.Wpf;
using Syncfusion.Windows.Tools.Controls;

namespace JsonDiffer.Code
{
    public static class FolderDialog
    {
        public static IDirectory? AskDirectory()
        {
            var dialog = new VistaFolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result is not true)
                return null;

            var path = dialog.SelectedPath;
            return new Directory(path);
        }
    }
}
