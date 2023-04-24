using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
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
using JsonDiffer.Code.Json;

namespace JsonDiffer.Controls
{
    /// <summary>
    /// Логика взаимодействия для DiffControl.xaml
    /// </summary>
    public partial class DiffControl : UserControl
    {
        public DiffControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty JsonDiffProperty = DependencyProperty.Register(
            nameof(JsonDiff), typeof(JsonNodeDiff), typeof(DiffControl), new PropertyMetadata(default(JsonNodeDiff)));

        public JsonNodeDiff JsonDiff
        {
            get => (JsonNodeDiff)GetValue(JsonDiffProperty);
            set => SetValue(JsonDiffProperty, value);
        }
    }
}
