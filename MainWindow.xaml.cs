using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;
using System.Xml.Serialization;

namespace EvilWindowsEditor
{
    public class TreeViewEx : TreeView
    {
        public TreeViewEx()
        {
            this.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(TreeViewEx_SelectedItemChanged);
        }

        void TreeViewEx_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.SelectedItem = e.NewValue;
        }

        #region SelectedItem

        /// <summary>
        /// Gets or Sets the SelectedItem possible Value of the TreeViewItem object.
        /// </summary>
        public new object SelectedItem
        {
            get { return this.GetValue(TreeViewEx.SelectedItemProperty); }
            set { this.SetValue(TreeViewEx.SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public new static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(TreeViewEx),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedItemProperty_Changed));

        static void SelectedItemProperty_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            TreeViewEx targetObject = dependencyObject as TreeViewEx;
            if (targetObject != null)
            {
                TreeViewItem tvi = targetObject.FindItemNode(targetObject.SelectedItem) as TreeViewItem;
                if (tvi != null)
                    tvi.IsSelected = true;
            }
        }
        #endregion SelectedItem   

        public TreeViewItem FindItemNode(object item)
        {
            TreeViewItem node = null;
            foreach (object data in this.Items)
            {
                node = this.ItemContainerGenerator.ContainerFromItem(data) as TreeViewItem;
                if (node != null)
                {
                    if (data == item)
                        break;
                    node = FindItemNodeInChildren(node, item);
                    if (node != null)
                        break;
                }
            }
            return node;
        }

        protected TreeViewItem FindItemNodeInChildren(TreeViewItem parent, object item)
        {
            TreeViewItem node = null;
            bool isExpanded = parent.IsExpanded;
            if (!isExpanded) //Can't find child container unless the parent node is Expanded once
            {
                parent.IsExpanded = true;
                parent.UpdateLayout();
            }
            foreach (object data in parent.Items)
            {
                node = parent.ItemContainerGenerator.ContainerFromItem(data) as TreeViewItem;
                if (data == item && node != null)
                    break;
                node = FindItemNodeInChildren(node, item);
                if (node != null)
                    break;
            }
            if (node == null && parent.IsExpanded != isExpanded)
                parent.IsExpanded = isExpanded;
            if (node != null)
                parent.IsExpanded = true;
            return node;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            GameDataViewObject.PropertyChanged += GameDataViewObject_PropertyChanged;
        }
        
        private void MenuItem_FindQuestByStat(object sender, RoutedEventArgs e)
        {
            // Instantiate the dialog box
            FindQuestByStat dlg = new FindQuestByStat() { DataContext = this.DataContext, Owner=this};
            // Open the dialog box modally 
            dlg.ShowDialog();
        }
        private void GameDataViewObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //This feels dirty, as it kind of violates the proper separation of data from UI
            if (e.PropertyName != null && e.PropertyName.Equals("SelectedObject"))
            {
                if ((sender as gameDataView).gameData != null && (sender as gameDataView).gameData.@class.Equals("QuestData"))
                {
                    (FindName("FlowchartPanel") as QuestFlowchart).BuildFlowchart();
                }
            }
        }

        private void MenuItem_About(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this, "This is where conspiracies are made...");
        }
        private void MenuItem_Quit(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void MenuItem_SaveFile(object sender, RoutedEventArgs e)
        {
            var filterString = new string[] { "*.xml" };
            var sfDialog = new SaveFileDialog()
            {
                DefaultExt = "xml",
                Title = "Select game data file to save"
            };
            if (sfDialog.ShowDialog(this) == true)
            {
                FileStream saveFileStream = null;
                if (File.Exists(sfDialog.FileName))
                {
                    saveFileStream = File.Open(sfDialog.FileName, FileMode.Truncate);
                }
                else
                {
                    saveFileStream = File.Open(sfDialog.FileName, FileMode.OpenOrCreate);
                }
                if (saveFileStream != null)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(gamedata));
                    serializer.Serialize(saveFileStream, GameDataViewObject.root);
                    saveFileStream.Dispose();
                }
            }
            return;
        }
        private void MenuItem_MergeFile(object sender, RoutedEventArgs e)
        {
            var ofDialog = new OpenFileDialog()
            {
                Title = "Select game data file to merge into this file",
                DefaultExt = ".xml",
            };
            bool? dialogResult = ofDialog.ShowDialog(this);
            if (dialogResult == true)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(gamedata));
                XmlReader reader = XmlReader.Create(ofDialog.FileName);
                gamedata fileToMerge = (gamedata)serializer.Deserialize(reader);
                reader.Dispose();
                MergeDialog inputDialog = new MergeDialog(GameDataViewObject.root,fileToMerge);
                if (inputDialog.ShowDialog() == true)
                {
                    inputDialog.commitMerge();
                    GameDataViewObject.root = GameDataViewObject.root;  //Retriggers the setter FIXME make this cleaner
                }
            }
        }
        private void MenuItem_OpenFile(object sender, RoutedEventArgs e)
        {
            var ofDialog = new OpenFileDialog()
            {
                Title = "Select game data file to Load",
                DefaultExt = ".xml",
            };
            bool? dialogResult = ofDialog.ShowDialog(this);
            if (dialogResult == true)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(gamedata));
                XmlReader reader = XmlReader.Create(ofDialog.FileName);
                GameDataViewObject.root = (gamedata)serializer.Deserialize(reader);
                reader.Dispose();
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
            GameDataViewObject.addNewQuest();
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
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.deleteSelectedObject();
        }
        private void DeleteSelectedQuestStepButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.deleteObject(GameDataViewObject.SelectedQuestStep);
        }
        private void AddHenchmanStatButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewHenchmanStat();
        }
        private void DeleteSelectedHenchmanStatButton_Click(object sender, RoutedEventArgs e)
        {
            object dgr = (sender as Button).FindName("HenchmanStatsGridRoot");
            object selected = (dgr as DataGrid).SelectedItem;
            if (selected != null)
            {
                GameDataViewObject.deleteObject(selected as gamedataObject);
            }
        }
        private void AddItemStatModifierButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewItemStatModifier();
        }
        private void DeleteSelectedItemStatModifierButton_Click(object sender, RoutedEventArgs e)
        {
            object dgr = (sender as Button).FindName("ItemStatModifierGridRoot");
            object selected = (dgr as DataGrid).SelectedItem;
            if (selected != null)
            {
                GameDataViewObject.deleteObject(selected as gamedataObject);
            }
        }
        private void AddCharacterStartingStatModifierButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewStartingCharacterInfoStatModifier();
        }
        private void DeleteSelectedCharacterStartingStatModifierButton_Click(object sender, RoutedEventArgs e)
        {
            object dgr = (sender as Button).FindName("CharacterStartingStatModifierGridRoot");
            object selected = (dgr as DataGrid).SelectedItem;
            if (selected != null)
            {
                GameDataViewObject.deleteObject(selected as gamedataObject);
            }
        }
        private void AddQuestStatRequirementButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewQuestStatRequirement();
        }
        private void DeleteSelectedQuestStatRequirementButton_Click(object sender, RoutedEventArgs e)
        {
            object dgr = (sender as Button).FindName("QuestStatRequirementGridRoot");
            object selected = (dgr as DataGrid).SelectedItem;
            if (selected != null)
            {
                GameDataViewObject.deleteObject(selected as gamedataObject);
            }
        }
        private void AddQuestItemRequirementButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewQuestItemRequirement();
        }
        private void DeleteSelectedQuestItemRequirementButton_Click(object sender, RoutedEventArgs e)
        {
            object dgr = (sender as Button).FindName("QuestItemRequirementGridRoot");
            object selected = (dgr as DataGrid).SelectedItem;
            if (selected != null)
            {
                GameDataViewObject.deleteObject(selected as gamedataObject);
            }
        }
        private void AddQuestStepItemGrantButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewQuestStepItemGrant();
        }
        private void DeleteSelectedQuestStepItemGrantButton_Click(object sender, RoutedEventArgs e)
        {
            object dgr = (sender as Button).FindName("QuestStepItemGrantGridRoot");
            object selected = (dgr as DataGrid).SelectedItem;
            if (selected != null)
            {
                GameDataViewObject.deleteObject(selected as gamedataObject);
            }
        }

        private void AddQuestStepStatGrantButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewQuestStepStatGrant();
        }
        private void DeleteSelectedQuestStepStatGrantButton_Click(object sender, RoutedEventArgs e)
        {
            object dgr = (sender as Button).FindName("QuestStepStatGrantGridRoot");
            object selected = (dgr as DataGrid).SelectedItem;
            if (selected != null)
            {
                GameDataViewObject.deleteObject(selected as gamedataObject);
            }
        }

        private void AddQuestStepHenchmanGrantButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewQuestStepHenchmanGrant();
        }
        private void DeleteSelectedQuestStepHenchmanGrantButton_Click(object sender, RoutedEventArgs e)
        {
            object dgr = (sender as Button).FindName("QuestStepHenchmanGrantGridRoot");
            object selected = (dgr as DataGrid).SelectedItem;
            if (selected != null)
            {
                GameDataViewObject.deleteObject(selected as gamedataObject);
            }
        }
        private void AddQuestStepChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewQuestStepChoice();
        }
        private void AddNewQuestStepToChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            object dgr = (sender as Button).FindName("QuestStepChoiceGridRoot");
            object selected = (dgr as DataGrid).SelectedItem;
            if (selected != null)
            {
                GameDataViewObject.addNewQuestStepToChoice(selected as gamedataObject);
            }
        }
        private void DeleteSelectedQuestStepChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            object dgr = (sender as Button).FindName("QuestStepChoiceGridRoot");
            object selected = (dgr as DataGrid).SelectedItem;
            if (selected != null)
            {
                GameDataViewObject.deleteObject(selected as gamedataObject);
            }
        }

        private void MoveSelectedQuestStepChoiceUpButton_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dgr = (sender as Button).FindName("QuestStepChoiceGridRoot") as DataGrid;
            gamedataObject selected = (dgr as DataGrid).SelectedItem as gamedataObject;
            if (selected != null)
            {
                //Find the largest sort index less than the selected one, that's who we want to swap numbers with.
                int sortIndex = 0;
                gamedataObject objectToSwap = null;
                foreach (gamedataObject otherChoice in GameDataViewObject.selectedQuestStepChoicesObservable)
                {
                    if (otherChoice.sortOrder > sortIndex && otherChoice.sortOrder < selected.sortOrder)
                    {
                        objectToSwap = otherChoice;
                        sortIndex = otherChoice.sortOrder;
                    }
                }
                if (objectToSwap != null) //If it is still null, there must be nothing above the selected in the list
                {
                    objectToSwap.sortOrder = selected.sortOrder;
                    selected.sortOrder = sortIndex;
                }
            }
            CollectionViewSource source = (CollectionViewSource)(this.Resources["selectedQuestStepChoicesViewSource"]);
            source.View.SortDescriptions.Clear();
            source.View.SortDescriptions.Add(new System.ComponentModel.SortDescription("sortOrder", System.ComponentModel.ListSortDirection.Ascending));
        }
        private void MoveSelectedQuestStepChoiceDownButton_Click(object sender, RoutedEventArgs e)
        {
            object dgr = (sender as Button).FindName("QuestStepChoiceGridRoot");
            gamedataObject selected = (dgr as DataGrid).SelectedItem as gamedataObject;
            if (selected != null)
            {
                //Find the largest sort index less than the selected one, that's who we want to swap numbers with.
                int sortIndex = Int32.MaxValue;
                gamedataObject objectToSwap = null;
                foreach (gamedataObject otherChoice in GameDataViewObject.selectedQuestStepChoicesObservable)
                {
                    if (otherChoice.sortOrder < sortIndex && otherChoice.sortOrder > selected.sortOrder)
                    {
                        objectToSwap = otherChoice;
                        sortIndex = otherChoice.sortOrder;
                    }
                }
                if (objectToSwap != null) //If it is still null, there must be nothing above the selected in the list
                {
                    objectToSwap.sortOrder = selected.sortOrder;
                    selected.sortOrder = sortIndex;
                }
            }
            CollectionViewSource source = (CollectionViewSource)(this.Resources["selectedQuestStepChoicesViewSource"]);
            source.View.SortDescriptions.Clear();
            source.View.SortDescriptions.Add(new System.ComponentModel.SortDescription("sortOrder", System.ComponentModel.ListSortDirection.Ascending));

        }
        private void AddQuestStepChoiceStatRequirementButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewQuestStepChoiceStatRequirement();
        }
        private void DeleteSelectedQuestStepChoiceStatRequirementButton_Click(object sender, RoutedEventArgs e)
        {
            object dgr = (sender as Button).FindName("QuestStepChoiceStatRequirementGridRoot");
            object selected = (dgr as DataGrid).SelectedItem;
            if (selected != null)
            {
                GameDataViewObject.deleteObject(selected as gamedataObject);
            }
        }
        private void AddQuestStepChoiceStatGrantButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewQuestStepChoiceStatGrant();
        }
        private void DeleteSelectedQuestStepChoiceStatGrantButton_Click(object sender, RoutedEventArgs e)
        {
            object dgr = (sender as Button).FindName("QuestStepChoiceStatGrantGridRoot");
            object selected = (dgr as DataGrid).SelectedItem;
            if (selected != null)
            {
                GameDataViewObject.deleteObject(selected as gamedataObject);
            }
        }
        private void AddQuestStepChoiceItemGrantButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewQuestStepChoiceItemGrant();
        }
        private void DeleteSelectedQuestStepChoiceItemGrantButton_Click(object sender, RoutedEventArgs e)
        {
            object dgr = (sender as Button).FindName("QuestStepChoiceItemGrantGridRoot");
            object selected = (dgr as DataGrid).SelectedItem;
            if (selected != null)
            {
                GameDataViewObject.deleteObject(selected as gamedataObject);
            }
        }
        private void AddQuestStepChoiceHenchmanGrantButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.addNewQuestStepChoiceHenchmanGrant();
        }
        private void DeleteSelectedQuestStepChoiceHenchmanGrantButton_Click(object sender, RoutedEventArgs e)
        {
            object dgr = (sender as Button).FindName("QuestStepChoiceHenchmanGrantGridRoot");
            object selected = (dgr as DataGrid).SelectedItem;
            if (selected != null)
            {
                GameDataViewObject.deleteObject(selected as gamedataObject);
            }
        }
        private void ReturnToFlowchartButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.ReturnToFlowchartView();
        }
        private void FlowchartVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            QuestFlowchart qf = sender as QuestFlowchart;
            if (e.NewValue as bool? == true)
            {
                qf.BuildFlowchart();
            }
        }
        
        private void QuestStepChoiceGridRoot_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {

            CollectionViewSource source = (CollectionViewSource)(this.Resources["selectedQuestStepChoicesViewSource"]);
            source.View.SortDescriptions.Clear();
            source.View.SortDescriptions.Add(new System.ComponentModel.SortDescription("sortOrder", System.ComponentModel.ListSortDirection.Ascending));
        }

        private void QuestStepChoiceGridRoot_Initialized(object sender, EventArgs e)
        {


            CollectionViewSource source = (CollectionViewSource)(this.Resources["selectedQuestStepChoicesViewSource"]);
            source.View.SortDescriptions.Clear();
            source.View.SortDescriptions.Add(new System.ComponentModel.SortDescription("sortOrder", System.ComponentModel.ListSortDirection.Ascending));

        }

        private void QuestStepChoiceGridRoot_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            CollectionViewSource source = (CollectionViewSource)(this.Resources["selectedQuestStepChoicesViewSource"]);
            source.View.SortDescriptions.Clear();
            source.View.SortDescriptions.Add(new System.ComponentModel.SortDescription("sortOrder", System.ComponentModel.ListSortDirection.Ascending));

        }

        private void ImportQuestStepChoices_Click(object sender, RoutedEventArgs e)
        {
            ChoiceImportDialog inputDialog = new ChoiceImportDialog();
            if (inputDialog.ShowDialog() == true)
            {
                using (System.IO.StringReader reader = new System.IO.StringReader(inputDialog.ChoicesText))
                {
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        GameDataViewObject.addNewQuestStepChoiceWithData(line);
                        line = reader.ReadLine();
                    }
                }
            }
        }
    }

    class MyExtensions
    {
        public static string GetHTML(DependencyObject obj)
        {
            return (string)obj.GetValue(HTMLProperty);
        }


        public static void SetHTML(DependencyObject obj, string value)
        {
            obj.SetValue(HTMLProperty, value);
        }


        // Using a DependencyProperty as the backing store for HTML.  This enables animation, styling, binding, etc...  
        public static readonly DependencyProperty HTMLProperty =
            DependencyProperty.RegisterAttached("HTML", typeof(string), typeof(MyExtensions), new PropertyMetadata(0));

        private static void OnHTMLChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {


            WebBrowser wv = d as WebBrowser;
            if (wv != null)
            {
                wv.NavigateToString((string)e.NewValue);
            }
        }
    }
    public static class BrowserBehavior
    {
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
            "Html",
            typeof(string),
            typeof(BrowserBehavior),
            new FrameworkPropertyMetadata(OnHtmlChanged));

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static string GetHtml(WebBrowser d)
        {
            return (string)d.GetValue(HtmlProperty);
        }

        public static void SetHtml(WebBrowser d, string value)
        {
            d.SetValue(HtmlProperty, value);
        }

        static void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser wb = d as WebBrowser;
            if (wb != null)
            {
                if (e.NewValue == null)
                {
                    wb.NavigateToString("&nbsp;");
                }
                else if (e.NewValue as string == "")
                {
                    wb.NavigateToString("&nbsp;");
                }
                else
                {
                    wb.NavigateToString(e.NewValue as string);
                }
            }
        }
    }
}
