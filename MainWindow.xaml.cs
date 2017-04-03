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

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            GameDataViewObject.deleteSelectedObject();
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
