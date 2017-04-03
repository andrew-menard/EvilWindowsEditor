using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;

namespace EvilWindowsEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_OpenFile(object sender, RoutedEventArgs e)
        {
           
            var ofDialog = new OpenFileDialog()
            {
                Title = "Select game data file to Load",
                DefaultExt = ".xml",
//                 Filter = ,
//                Filters = { new FileDialogFilter("XML (.xml)", ".xml") }
            };
            bool? dialogResult = ofDialog.ShowDialog(this);
            if (dialogResult == true)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(gamedata));
                XmlReader reader = XmlReader.Create(ofDialog.FileName);
                //objectTree.SuspendLayout(); //When the number of objects gets large, redrawing the object tree once per item will get slow, so we turn off redraws when loading things.
                GameDataViewObject.root = (gamedata)serializer.Deserialize(reader);
                reader.Dispose();
                //objectTree.RefreshData(); //Required to make the object tree recognize child nodes when a top level node is added.
                //objectTree.ResumeLayout(); //Turn redraws back on.
                //afterLoad = true;
            }
        }

        private void MenuItem_CreateItem(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewObject("Item");
        }
        private void MenuItem_CreateItemType(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewObject("ItemType");
        }
        private void MenuItem_CreateStat(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewObject("Stat");
        }
        private void MenuItem_CreateStatGroup(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewObject("StatGroup");
        }
        private void MenuItem_CreateQuest(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewObject("Quest");
        }
        private void MenuItem_CreateHenchman(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewObject("Henchman");
        }
        private void MenuItem_CreateLocation(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewObject("Location");
        }
        private void MenuItem_CreateNPC(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewObject("NPC");
        }
        private void MenuItem_CreateStartingCharacterInfo(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewObject("StartingCharacterInfo");
        }
    }
}
