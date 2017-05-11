using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace EvilWindowsEditor
{
    public class MergeTreeItem : INotifyPropertyChanged
    {
        //This is the class for stuff in the tree view; Name is what gets displayed.  Since this can either be an actual 
        //object or a category of objects, you can set a ref to an object, in which case the name comes from that, or you
        //can have a null object and set name manually for a category.
        public MergeTreeItem()
        {
            Children = new ObservableCollection<MergeTreeItem>();
        }
        private string id = "";
        public string Id { get { if (OriginalObjectRef != null) { return OriginalObjectRef.uuid; } else { if (MergeObjectRef != null) { return MergeObjectRef.uuid; } else { return null; } } }
            set { id = value; } }
        public gamedataObject OriginalObjectRef { get; set; }
        public gamedataObject MergeObjectRef { get; set; }
        private string _name;
        public string Name
        {
            get
            {
                if (OriginalObjectRef != null)
                {
                    if (MergeObjectRef != null)
                    {
                        if (MergeObjectRef.Name.Equals(OriginalObjectRef.Name))
                        {
                            return OriginalObjectRef.Name;
                        }
                        else
                        {
                            return OriginalObjectRef.Name + " (renamed to " + MergeObjectRef.Name + " in the merge file)";
                        }
                    }
                    else
                    {
                        return OriginalObjectRef.Name + " (does not exist in merge file, using original version)";
                    }
                }
                else
                {
                    if (MergeObjectRef != null)
                    {
                        return MergeObjectRef.Name + " (does not exist in original file, using version from merge file)";
                    }
                    else
                    {
                        //Exists in neither, this must be a header field, so just use the name
                        return _name;
                    }
                }
            }
            set
            {
                _name = value; NotifyPropertyChanged("Name");
            }
        }
        public ObservableCollection<MergeTreeItem> Children { get; set; }
        private Boolean _selectOriginal = false;
        private Boolean _selectMerge = false;
        public Boolean SelectOriginal
        {
            get { return _selectOriginal; }
            set { _selectOriginal = value; _selectMerge = !value; NotifyPropertyChanged("SelectMerge"); NotifyPropertyChanged("ForegroundColor"); }
        }
        public Boolean SelectMerge
        {
            get { return _selectMerge; }
            set { _selectMerge = value; _selectOriginal = !value; NotifyPropertyChanged("SelectOriginal"); NotifyPropertyChanged("ForegroundColor"); }
        }
        public Boolean ObjectsDiffer
        {
            //Note: one object exists and the other does not returns true for this, because user doesn't need to make a choice
            get
            {
                if (OriginalObjectRef != null)
                {
                    if (MergeObjectRef != null)
                    {
                        return !OriginalObjectRef.Equals(MergeObjectRef);
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
        }
        public System.Windows.Media.Brush ForegroundColor
        {
            get
            {
                if (ObjectsDiffer)
                {
                    if (_selectMerge || _selectOriginal)
                    {
                        return System.Windows.Media.Brushes.Green;
                    }
                    else
                    {

                        return System.Windows.Media.Brushes.Red;
                    }
                }
                if (MergeObjectRef == null && OriginalObjectRef!=null)
                {
                    return System.Windows.Media.Brushes.Blue;
                }
                if (OriginalObjectRef == null && MergeObjectRef != null)
                {
                    return System.Windows.Media.Brushes.Blue;
                }
                return System.Windows.Media.Brushes.Black;
            }

            }
        //Implementing INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }

    class MergeProcessorView : INotifyPropertyChanged
    {
        public MergeProcessorView()
        { }
        public void setData(gamedata originalData, gamedata mergeData)
        {
            _originalData = originalData;
            _mergeData = mergeData;
            foreach (gamedataObject originalObject in originalData.Items)
            {
                originalObject.associatedRootData = originalData;
            }
            foreach (gamedataObject mergeObject in mergeData.Items)
            {
                mergeObject.associatedRootData = mergeData;
            }
            //Create the merge tree here.
            _mergeTree = new ObservableCollection<MergeTreeItem>();
            MergeTree.Clear();
            var elementList = new List<string> { "ItemTypeData", "ItemData", "StatData", "QuestData", "StatGroupData", "HenchmanData", "LocationData", "NPCData", "StartingCharacterInfoData" };
            foreach (String elementClass in elementList)
            {
                int substringOffset = elementClass.LastIndexOf("Data");
                string elementClassShortName = elementClass.Substring(0, substringOffset);
                MergeTreeItem classNode = new MergeTreeItem() { Name = elementClassShortName, OriginalObjectRef = null, MergeObjectRef = null };
                Dictionary<string, MergeTreeItem> itemsByID = new Dictionary<string, MergeTreeItem>();
                foreach (gamedataObject originalObject in originalData.Items.Where<gamedataObject>(iter => iter.@class.Equals(elementClass) && iter.deleted == "False"))
                {
                    MergeTreeItem childNode = new MergeTreeItem() { OriginalObjectRef = originalObject };
                    itemsByID.Add(originalObject.uuid, childNode);
                }
                foreach (gamedataObject mergeObject in mergeData.Items.Where<gamedataObject>(iter => iter.@class.Equals(elementClass) && iter.deleted == "False"))
                {
                    if (itemsByID.ContainsKey(mergeObject.uuid))
                    {
                        itemsByID[mergeObject.uuid].MergeObjectRef = mergeObject;
                        if (itemsByID[mergeObject.uuid].MergeObjectRef.Equals(itemsByID[mergeObject.uuid].OriginalObjectRef))
                        {
                            //The items are identical, so remove the listing before displaying the merge tree.
                            itemsByID.Remove(mergeObject.uuid);
                        }
                    }
                    else
                    {
                        MergeTreeItem childNode = new MergeTreeItem() { MergeObjectRef = mergeObject };
                        itemsByID.Add(mergeObject.uuid, childNode);
                    }
                }
                foreach (MergeTreeItem childNode in itemsByID.Values)
                {
                    childNode.PropertyChanged += ChildNode_PropertyChanged;
                    classNode.Children.Add(childNode);
                }
                MergeTree.Add(classNode);
            }

            NotifyPropertyChanged("MergeTree");
        }
        public void commitMerge()
        {
            //At this point, everything that differs should have SelectOriginal or SelectMerge set; take the relevant things from
            //_mergeData and put them into _originalData (those that have SelectMerge or where original wasn't present)
            //For items where we selected the original, or were present in the original but not the merge, or were identical, we just leave
            //it in the original data.
            foreach(MergeTreeItem categoryMTI in MergeTree)
            {
                foreach (MergeTreeItem objectMTI in categoryMTI.Children)
                {
                    if (objectMTI.SelectMerge || objectMTI.OriginalObjectRef == null)
                    {
                        if (objectMTI.OriginalObjectRef != null)
                        {
                            _originalData.Items.Remove(objectMTI.OriginalObjectRef);
                            foreach (gamedataObject originalDependentObj in objectMTI.OriginalObjectRef.GetDependentObjects())
                            {

                                _originalData.Items.Remove(originalDependentObj);
                            }
                        }
                        if (_originalData.getDeletedItemByID(objectMTI.MergeObjectRef.uuid)!=null)
                        {
                            //Deleted items weren't added to the merge tree, so it is possible that the original is present but deleted, in which case we need to remove it.
                            gamedataObject deletedOriginalObj = _originalData.getDeletedItemByID(objectMTI.MergeObjectRef.uuid);
                            _originalData.Items.Remove(deletedOriginalObj);
                            foreach (gamedataObject originalDependentObj in deletedOriginalObj.GetDependentObjects())
                            {

                                _originalData.Items.Remove(originalDependentObj);
                            }
                        }
                        _originalData.Items.Add(objectMTI.MergeObjectRef);
                        foreach (gamedataObject mergeDependentObj in objectMTI.MergeObjectRef.GetDependentObjects())
                        {

                            _originalData.Items.Add(mergeDependentObj);
                        }
                    }
                }
            }
        }
        private void ChildNode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("SelectOriginal") || e.PropertyName.Equals("SelectMerge"))
                NotifyPropertyChanged("MergeComplete");
        }
        
        private MergeTreeItem _selectedMergeTreeItem = null;
        public MergeTreeItem SelectedMergeTreeItem
        {
            get { return _selectedMergeTreeItem; }
            set { _selectedMergeTreeItem = value; }
        }
        private gamedata _originalData;
        private gamedata _mergeData;

        private ObservableCollection<MergeTreeItem> _mergeTree = null;
        public ObservableCollection<MergeTreeItem> MergeTree
        {
            get { return _mergeTree; }

        }
        public bool MergeComplete
        {
            get
            {
                if (MergeTree == null)
                {
                    return false; //When first invoked, before MergeTree is defined, we need to default it to false.
                }
                foreach (MergeTreeItem mti in MergeTree)
                {
                    foreach (MergeTreeItem childMTI in mti.Children)
                    {
                        if (childMTI.ObjectsDiffer && !childMTI.SelectMerge && !childMTI.SelectOriginal)
                            return false;
                    }
                }
                return true;
            }
        }

        //Implementing INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
