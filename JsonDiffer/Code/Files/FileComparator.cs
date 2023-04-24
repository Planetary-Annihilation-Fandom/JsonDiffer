using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Automata.IO;
using JsonDiffer.Code.Json;
using Syncfusion.Linq;

namespace JsonDiffer.Code.Files
{
    public class FileComparator
    {
        public FilePair Pair { get; set; }

        private FileIdentity? A => Pair?.A;
        private FileIdentity? B => Pair?.B;

        public FileCompareResult Result { get; private set; }

        public ObservableCollection<JsonNodeDiff>? Differences { get; private set; }

        public FileComparator(FilePair pair)
        {
            Pair = pair;
            if (A is null && B is null)
                throw new ArgumentNullException();

        }
        

        public async Task Compare()
        {
            if (A is null)
            {
                Result = FileCompareResult.New;
                return;
            }

            if (B is null)
            {
                Result = FileCompareResult.Delete;
                return;
            }

            var aJson = JsonNode.Parse(await A.File.ReadAsync());
            var bJson = JsonNode.Parse(await B.File.ReadAsync());

            if (aJson is null || bJson is null)
            {
                Result = FileCompareResult.JsonCorrupt;
                return;
            }

            var jsonComparator = new JsonComparator(aJson, bJson, -1);
            Differences = (await jsonComparator.Compare()).ToObservableCollection();

            if (Differences.Count == 0)
            {
                Result = FileCompareResult.Equal;
                return;
            }

            Result = FileCompareResult.Modify;
            return;
        }
    }

    public enum FileCompareResult
    {
        New,
        Delete,
        Modify,
        Equal,
        JsonCorrupt
    }
}
