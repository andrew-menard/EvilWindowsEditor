using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EvilWindowsEditor
{
    /// <summary>
    /// Interaction logic for MergeDialog.xaml
    /// </summary>
    public partial class MergeDialog : Window
    {
        public MergeDialog(gamedata original, gamedata merge)
        {
            InitializeComponent();
            MergeProcessorObject.setData(original, merge);
        }
        public void commitMerge()
        {
            //When the data has been accepted, this is called, and performs the actual merge.
            MergeProcessorObject.commitMerge();
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
