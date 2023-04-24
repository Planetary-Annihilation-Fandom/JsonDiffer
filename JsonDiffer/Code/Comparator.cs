using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Automata.IO;
using JsonDiffer.Code.Files;

namespace JsonDiffer.Code
{
    public class Comparator: INotifyPropertyChanged
    {
        private FileGroup _newFilesGroup;
        private FileGroup _deleteFilesGroup;
        private FileGroup _modifyFilesGroup;
        private FileGroup _equalFilesGroup;
        private FileGroup _jsonCorruptFilesGroup;
        public IDirectory ARoot { get; set; }
        public IDirectory BRoot { get; set; }

        public Comparator(IDirectory aRoot, IDirectory bRoot)
        {
            ARoot = aRoot ?? throw new ArgumentNullException(nameof(aRoot));
            BRoot = bRoot ?? throw new ArgumentNullException(nameof(bRoot));

            NewFiles = new();
            DeleteFiles = new();
            ModifyFiles = new();
            EqualFiles = new();
            JsonCorruptFiles = new();
        }
        // New,
        // Delete,
        // Modify,
        // Equal,
        // JsonCorrupt
        public List<FileComparator> NewFiles { get; set; }
        public List<FileComparator> DeleteFiles { get; set; }
        public List<FileComparator> ModifyFiles { get; set; }
        public List<FileComparator> EqualFiles { get; set; }
        public List<FileComparator> JsonCorruptFiles { get; set; }

        public FileGroup NewFilesGroup
        {
            get => _newFilesGroup;
            set
            {
                if (Equals(value, _newFilesGroup)) return;
                _newFilesGroup = value;
                OnPropertyChanged();
            }
        }

        public FileGroup DeleteFilesGroup
        {
            get => _deleteFilesGroup;
            set
            {
                if (Equals(value, _deleteFilesGroup)) return;
                _deleteFilesGroup = value;
                OnPropertyChanged();
            }
        }

        public FileGroup ModifyFilesGroup
        {
            get => _modifyFilesGroup;
            set
            {
                if (Equals(value, _modifyFilesGroup)) return;
                _modifyFilesGroup = value;
                OnPropertyChanged();
            }
        }

        public FileGroup EqualFilesGroup
        {
            get => _equalFilesGroup;
            set
            {
                if (Equals(value, _equalFilesGroup)) return;
                _equalFilesGroup = value;
                OnPropertyChanged();
            }
        }

        public FileGroup JsonCorruptFilesGroup
        {
            get => _jsonCorruptFilesGroup;
            set
            {
                if (Equals(value, _jsonCorruptFilesGroup)) return;
                _jsonCorruptFilesGroup = value;
                OnPropertyChanged();
            }
        }

        public async Task Compare()
        {
            var explorer = new FileExplorer();
            explorer.AddExtension(".json");

            var aFiles = await explorer.Explore(ARoot);
            var bFiles = await explorer.Explore(BRoot);

            var pairs = new List<FilePair>();

            // Создаем пары на основе ARoot
            foreach (var aFile in aFiles)
            {
                var identity = new FileIdentity(aFile);
                pairs.Add(new FilePair(identity, null));
            }

            // Сопоставляем файлы и пары BRoot
            foreach (var bFile in bFiles)
            {
                var identity = new FileIdentity(bFile);

                var pair = pairs.FirstOrDefault(x => x.A?.Identifier == identity.Identifier);
                if (pair is null)
                {
                    pair = new FilePair(null, identity);
                    pairs.Add(pair);
                    continue;
                }

                pair.B = identity;
            }

            // Сравниваем файлы и распределяем их по коллекциям
            foreach (var pair in pairs)
            {
                var comparator = new FileComparator(pair);
                await comparator.Compare();

                switch (comparator.Result)
                {
                    case FileCompareResult.New:
                        NewFiles.Add(comparator);
                        break;
                    case FileCompareResult.Delete:
                        DeleteFiles.Add(comparator);
                        break;
                    case FileCompareResult.Modify:
                        ModifyFiles.Add(comparator);
                        break;
                    case FileCompareResult.Equal:
                        EqualFiles.Add(comparator);
                        break;
                    case FileCompareResult.JsonCorrupt:
                        JsonCorruptFiles.Add(comparator);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            NewFilesGroup = Group(NewFiles, "New", IdentifierForNew);
            DeleteFilesGroup = Group(DeleteFiles, "Delete", IdentifierForDelete);
            ModifyFilesGroup = Group(ModifyFiles, "Modify", IdentifierForDefault);
            EqualFilesGroup = Group(EqualFiles, "Modify", IdentifierForDefault);
            JsonCorruptFilesGroup = Group(JsonCorruptFiles, "Modify", IdentifierForDefault);
        }

        private (string, string, string) IdentifierForNew(FileComparator fc)
        {
            var l0 = fc.Pair.B!.Identifier.L0;
            var l1 = fc.Pair.B!.Identifier.L1;
            var l2 = fc.Pair.B!.Identifier.L2;

            return (l0, l1, l2);
        }

        private (string, string, string) IdentifierForDelete(FileComparator fc)
        {
            var l0 = fc.Pair.A!.Identifier.L0;
            var l1 = fc.Pair.A!.Identifier.L1;
            var l2 = fc.Pair.A!.Identifier.L2;

            return (l0, l1, l2);
        }

        private (string, string, string) IdentifierForDefault(FileComparator fc)
        {
            var l0 = fc.Pair.A!.Identifier.L0;
            var l1 = fc.Pair.A!.Identifier.L1;
            var l2 = fc.Pair.A!.Identifier.L2;

            return (l0, l1, l2);
        }

        public FileGroup Group(List<FileComparator> fileComparators,string rootName,Func<FileComparator, (string,string,string)> identifierSelector)
        {
            var groups = new List<FileGroup>();

            foreach (var fc in fileComparators)
            {
                var triple = identifierSelector(fc);
                var l0 = triple.Item1;
                var l1 = triple.Item2;
                var l2 = triple.Item3;

                var l0G = Search(l0);
                if (l0G is null)
                {
                    l0G = new FileGroup(l0, null);
                    groups.Add(l0G);
                }

                var l1G = DeepSearch(l0G, l1);
                if (l1G is null)
                {
                    l1G = new FileGroup(l1, null);
                    l0G.Nodes.Add(l1G);
                }

                var l2G = DeepSearch(l1G, l2);
                if (l2G is null)
                {
                    l2G = new FileGroup(l2, fc);
                    l1G.Nodes.Add(l2G);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            var rootGroup = new FileGroup(rootName, null);
            foreach (var fileGroup in groups)
            {
                rootGroup.Nodes.Add(fileGroup);
            }

            return rootGroup;

            FileGroup? Search(string name)
            {
                foreach (var group in groups)
                {
                    var res = DeepSearch(group, name);
                    if (res is not null)
                        return res;
                }

                return null;
            }
        }

        public FileGroup? DeepSearch(FileGroup group, string name)
        {
            if (group.Name == name)
                return group;

            // Проверяем имена только детей
            foreach (var groupNode in group.Nodes)
            {
                if (groupNode.Name == name)
                    return groupNode;
            }

            foreach (var groupNode in group.Nodes)
            {
                var result = DeepSearch(groupNode, name);
                if (result is not null)
                    return result;
            }

            return null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class FileGroup : INotifyPropertyChanged
    {
        private string _name;
        private FileComparator? _fileComparator;
        private ObservableCollection<FileGroup> _nodes;

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public FileComparator? FileComparator
        {
            get => _fileComparator;
            set
            {
                if (Equals(value, _fileComparator)) return;
                _fileComparator = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FileGroup> Nodes
        {
            get => _nodes;
            set
            {
                if (Equals(value, _nodes)) return;
                _nodes = value;
                OnPropertyChanged();
            }
        }

        public FileGroup(string name, FileComparator? fileComparator)
        {
            Name = name;
            FileComparator = fileComparator;
            Nodes = new();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
