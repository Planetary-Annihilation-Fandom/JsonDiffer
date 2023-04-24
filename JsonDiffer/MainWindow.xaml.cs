using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Automata.IO;
using JsonDiffer.Code;
using JsonDiffer.Code.Json;
using Syncfusion.Linq;
using Syncfusion.ProjIO;
using Task = System.Threading.Tasks.Task;

namespace JsonDiffer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Setup _setup;
        private Comparator _comparator;

        public Setup Setup
        {
            get => _setup;
            set
            {
                if (Equals(value, _setup)) return;
                _setup = value;
                OnPropertyChanged();
            }
        }

        public Comparator Comparator
        {
            get => _comparator;
            set
            {
                if (Equals(value, _comparator)) return;
                _comparator = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            //var node = new JsonObject()
            //{
            //    ["val"] = "aaaa",
            //    ["intval"] = 1,
            //    ["arrr"] = new JsonArray(1,2,4,5,new JsonObject()
            //    {
            //        ["innerval"] = "aaaa"
            //    })
            //};

            //C:/WorkRoot/Development/Testing/PaJsonDiffer/mla/
            //C:/WorkRoot/Development/Testing/PaJsonDiffer/mla2/

            //TestJsonComparator();

            InitializeComponent();

            Setup = new Setup();
            SetProgressText("Select folders");
            SetProgressBar(1);

            //Setup.Original = new Directory("C:/WorkRoot/Development/Testing/PaJsonDiffer/mla/");
            //Setup.Modified = new Directory("C:/WorkRoot/Development/Testing/PaJsonDiffer/mla2/");

            //StartAnalyzeClick(StartAnalyzeButton, null);
        }

        private async Task TestJsonComparator()
        {
            var root = (new Directory(AppDomain.CurrentDomain.BaseDirectory)).Directory("root");
            var testJsons = root.Directory("test", "test.jsons");
            var a = testJsons.File("a.json");
            var b = testJsons.File("b.json");
            
            testJsons.Create();
            if (!a.Exist() || !b.Exist())
                throw new InvalidOperationException();

            var aJson = JsonNode.Parse(await a.ReadAsync());
            var bJson = JsonNode.Parse(await b.ReadAsync());

            if (aJson is null || bJson is null)
                throw new ArgumentNullException();

            var comparator = new JsonComparator(aJson, bJson, -1);
            var diffs = await comparator.Compare();
        }

        private void SelectOriginalFolderClick(object sender, RoutedEventArgs e)
        {
            var folder = FolderDialog.AskDirectory();
            if (folder is null)
                return;

            if (folder.Compare(Setup.Modified))
            {
                Dialogs.Warn("Selected folder same as Modified folder!");
                return;
            }

            if (folder.HasInheritance(Setup.Modified))
            {
                Dialogs.Warn("Selected folder has inheritance with Modified folder!");
                return;
            }

            Setup.Original = folder;
        }

        private void SelectModifiedFolderClick(object sender, RoutedEventArgs e)
        {
            var folder = FolderDialog.AskDirectory();
            if (folder is null)
                return;

            if (folder.Compare(Setup.Original))
            {
                Dialogs.Warn("Selected folder same as Original folder!");
                return;
            }

            if (folder.HasInheritance(Setup.Original))
            {
                Dialogs.Warn("Selected folder has inheritance with Original folder!");
                return;
            }

            Setup.Modified = folder;
        }

        public void SetProgressBar(double value)
        {
            var i = (int)Math.Floor(value);
            SetupProgressBar.Progress = i;
        }

        public void SetProgressText(string text)
        {
            SetupStatusText.Text = text;
        }

        private async void StartAnalyzeClick(object sender, RoutedEventArgs e)
        {
            StartAnalyzeButton.IsEnabled = false;

            if (Setup.Original is null || Setup.Modified is null)
            {
                Dialogs.Warn("Select Original and Modified folders!");
                StartAnalyzeButton.IsEnabled = true;
                return;
            }

            
            var comparator = new Comparator(Setup.Original, Setup.Modified);
            await comparator.Compare();

            Comparator = comparator;
            SetProgressText("Analyze Complete");
            SetProgressBar(100);
            StartAnalyzeButton.IsEnabled = true;
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

        private void SelectedModifiedChanged(object sender, Syncfusion.UI.Xaml.TreeView.ItemSelectionChangedEventArgs e)
        {
            //var group = ModifiedFilesTree.SelectedItem as FileGroup;
            //if (group?.FileComparator is null)
            //    return;

            //var comp = group.FileComparator;

            //var observableCollection = comp.Differences.ToObservableCollection();

            //ModifiedView.ItemsSource = observableCollection;
            
        }
    }
}
