using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Automata.IO;

namespace JsonDiffer.Code.Files
{
    public class FileIdentity
    {
        public readonly FileIdentifier Identifier;

        public readonly IFile File;

        public FileIdentity(IFile file)
        {
            File = file;

            Identifier = new FileIdentifier(
                file.Directory.Parent()?.Name() ?? "drive",
                file.Directory.Name(),
                file.Name);
        }
    }

    public class FileIdentifier
    {
        public FileIdentifier(string l0, string l1, string l2)
        {
            L0 = l0 ?? throw new ArgumentNullException(nameof(l0));
            L1 = l1 ?? throw new ArgumentNullException(nameof(l1));
            L2 = l2 ?? throw new ArgumentNullException(nameof(l2));
        }

        public string L0 { get; set; }
        public string L1 { get; set; }
        public string L2 { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is not FileIdentifier fi) return false;
            
            return L0 == fi.L0 && L1 == fi.L1 && L2 == fi.L2;
        }

        protected bool Equals(FileIdentifier other)
        {
            return L0 == other.L0 && L1 == other.L1 && L2 == other.L2;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(L0, L1, L2);
        }

        public static bool operator ==(FileIdentifier? a, FileIdentifier? b)
        {
            if (a is null)
                return false;
            if (b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(FileIdentifier? a, FileIdentifier? b)
        {
            return !(a == b);
        }
    }

    public static class FileIdentityExtensions
    {
        public static FileIdentity ToIdentity(this IFile file) => new FileIdentity(file);
    }
}
