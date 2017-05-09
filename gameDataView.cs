using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Net;

namespace EvilWindowsEditor
{
    public class GameTreeItem : INotifyPropertyChanged
    {
        //This is the class for stuff in the tree view; Name is what gets displayed.  Since this can either be an actual 
        //object or a category of objects, you can set a ref to an object, in which case the name comes from that, or you
        //can have a null object and set name manually for a category.
        public GameTreeItem()
        {
            Children = new ObservableCollection<GameTreeItem>();
        }
        public string MinValueSorter
        {
            get
            {
                if (minStatValue > 0|| maxStatValue >0)
                {
                    //Sorts by min value, then by max value, then by name
                    return minStatValue.ToString("D8") + maxStatValue.ToString("D8") + Name;
                }
                else
                { 
                    return 99999999.ToString("D8") + 99999999.ToString() + Name;
                }
            }
        }
        private int minStatValue = 0;
        public int MinStatValue {
            get { return minStatValue; }
            set { minStatValue = value; NotifyPropertyChanged("Name"); }
        }

        private int maxStatValue = 0;
        public int MaxStatValue
        {
            get { return maxStatValue; }
            set { maxStatValue = value; NotifyPropertyChanged("Name"); }
        }
        private string id="";
        public string Id { get { if (ObjectRef != null) { return ObjectRef.uuid; } else { return id; } } set { id = value; } }
        public gamedataObject ObjectRef { get; set; }
        private string _name;
        public string Name
        {
            get {
                if (ObjectRef != null)
                {
                    if (minStatValue == 0 && maxStatValue==0)
                    { return ObjectRef.name; }
                    else
                    { return ObjectRef.name + "(requires " + minStatValue.ToString() + "-" +maxStatValue.ToString()+")"; }
                }
                else { return _name; }
            }
            set { _name = value; NotifyPropertyChanged("Name"); }
        }
        public ObservableCollection<GameTreeItem> Children { get; set; }
        //Implementing INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    //This class holds the data, and holds what object is selected etc, so that the various controls can bind to it.
    //It has a number of sadly redundant bits, as you need to have things in somewhat specific forms to bind to.
    //Uses the INotifyPropertyChanged interface to let bound form elements know when relevant things have changed.
    public class gameDataView : INotifyPropertyChanged
    {
        private GameTreeItem selectedGameTreeItem = null;
        private ObservableCollection<GameTreeItem> gameTree = null;
        private gamedataObject gameDataObj = null; //This is the currently selected object, or null if none is selected.
        private gamedataObject _selectedQuestStep = null; //If a quest is selected, this is null if in flowchart view or is the selected step that you are focusing on
        private gamedataObject _selectedQuestStepChoice = null; //If a quest is selected and you have a selected quest step, this is the selected step's choice that you are focusing on
        public Dictionary<string, gamedataObject> allObjects; //Dict for looking up by objects by uuid
        private ObservableCollection<gamedataObject> locations; //Location list, for binding combo boxes 
        public ObservableCollection<gamedataObject> Locations { get => locations; set { locations = value; NotifyPropertyChanged("Locations"); } }
        private ObservableCollection<gamedataObject> henchmen; //Henchmen list, for binding combo boxes for item grants and requirements
        public ObservableCollection<gamedataObject> Henchmen { get => henchmen; set { henchmen = value; NotifyPropertyChanged("Henchmen"); } }
        private ObservableCollection<gamedataObject> items; //Items list, for binding combo boxes for item grants and requirements
        public ObservableCollection<gamedataObject> Items { get => items; set { items = value; NotifyPropertyChanged("Item"); } }
        private ObservableCollection<gamedataObject> itemTypes; //Item types list, for binding combo box on the item screen
        public ObservableCollection<gamedataObject> ItemTypes { get => itemTypes; set { itemTypes = value; NotifyPropertyChanged("ItemTypes"); } }
        private ObservableCollection<gamedataObject> stats; //Stats list, for binding combo boxes for stat requirements
        public ObservableCollection<gamedataObject> Stats { get => stats; set { stats = value; NotifyPropertyChanged("Stats"); } }
        private ObservableCollection<gamedataObject> statGroups; //Stat Groups list, for binding combo boxes for stat group
        public ObservableCollection<gamedataObject> StatGroups { get => statGroups; set { statGroups = value; NotifyPropertyChanged("StatGroups"); } }
        public ObservableCollection<gamedataObject> questSteps; //List of steps on the current quest, for binding combo boxes for quest choices.
        private ObservableCollection<gamedataObject> quests; //List of quests, for binding combo boxes for quests.
        public ObservableCollection<gamedataObject> Quests { get => quests; set { quests = value; NotifyPropertyChanged("Quests"); } }
        private ObservableCollection<gamedataObject> npcs; //List of npcs, for binding combo boxes for npcs.
        public ObservableCollection<gamedataObject> NPCs { get => npcs; set { npcs = value; NotifyPropertyChanged("NPCs"); } }
        Dictionary<String, List<gamedataObject>> elementsByType; //For elements that show up in the tree view only.

        private bool _postLoad = false;
        public bool PostLoad { get { return _postLoad; } set { _postLoad = value; NotifyPropertyChanged("pickNewObjectVisible"); NotifyPropertyChanged("PostLoad"); NotifyPropertyChanged("PreLoad"); } }
        public bool PreLoad { get { return !_postLoad; } }
        private gamedata _root;
        public gamedata root
        {
            get
            {
                return _root;
            }
            set
            {
                //We're only actually going to set this when we have read in a new file; it sets up the various 
                //reference lists and dictionaries that we use for convenient binding to the UI.
                _root = value;
                //This should be broken out to a new function, we really don't need to do this when objects are addedd
                if (root.Items == null)
                {
                    //This covers the edge case where we read in a file but it contained no items; in that case the 
                    //reader never created root.Items, so some of the bindings will get nulls.
                    root.Items = new gamedataObject[0];
                }
                locations.Clear();
                gamedataObject nullLocation = new gamedataObject();
                nullLocation.uuid = "";
                nullLocation.name = "(none)";
                locations.Add(nullLocation); //Empty value so you can "unselect" things in the combo boxes
                foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("LocationData") && iter.deleted=="False"))
                {
                    locations.Add(gameObject);
                }
                henchmen.Clear();
                foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("HenchmanData") && iter.deleted == "False"))
                {
                    henchmen.Add(gameObject);
                }
                items.Clear();
                foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("ItemData") && iter.deleted == "False"))
                {
                    items.Add(gameObject);
                }
                itemTypes.Clear();
                foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("ItemTypeData") && iter.deleted == "False"))
                {
                    itemTypes.Add(gameObject);
                }
                stats.Clear();
                gamedataObject nullStat = new gamedataObject();
                nullStat.uuid = "";
                nullStat.name = "(none)";
                stats.Add(nullStat); //Empty value so you can "unselect" things in the combo boxes
                foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("StatData") && iter.deleted == "False"))
                {
                    stats.Add(gameObject);
                }
                statGroups.Clear();
                gamedataObject nullStatGroup = new gamedataObject();
                nullStatGroup.uuid = "";
                nullStatGroup.name = "(none)";
                statGroups.Add(nullStatGroup); //Empty value so you can "unselect" things in the combo boxes
                foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("StatGroupData") && iter.deleted == "False"))
                {
                    statGroups.Add(gameObject);
                }
                quests.Clear();
                foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestData") && iter.deleted == "False"))
                {
                    quests.Add(gameObject);
                }
                npcs.Clear();
                gamedataObject nullNPC = new gamedataObject();
                nullNPC.uuid = "";
                nullNPC.name = "(none)";
                npcs.Add(nullNPC); //Empty value so you can "unselect" things in the combo boxes
                foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("NPCData") && iter.deleted == "False"))
                {
                    npcs.Add(gameObject);
                }
                allObjects.Clear();
                foreach (gamedataObject gameObject in root.Items)
                {
                    allObjects.Add(gameObject.uuid, gameObject);
                }
                elementsByType.Clear();
                foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => !iter.@class.Equals("QuestStepData")
                                                                       && !iter.@class.Equals("QuestStepChoiceData")
                                                                       && !iter.@class.Equals("QuestStepChoiceStatRequirementData")
                                                                       && !iter.@class.Equals("QuestStatRequirementData")
                                                                       && !iter.@class.Equals("ItemStatModifierData")))
                {
                    if (!elementsByType.ContainsKey(gameObject.@class))
                    {
                        elementsByType.Add(gameObject.@class, new List<gamedataObject>());
                    }
                    elementsByType[gameObject.@class].Add(gameObject);
                }
                GameTree.Clear();
                var elementList = new List<string> { "ItemTypeData", "ItemData", "StatData", "QuestData", "StatGroupData", "HenchmanData", "LocationData", "NPCData", "StartingCharacterInfoData" };
                foreach (String elementClass in elementList)
                {
                    int substringOffset = elementClass.LastIndexOf("Data");
                    string elementClassShortName = elementClass.Substring(0, substringOffset);
                    GameTreeItem classNode = new GameTreeItem() { Name = elementClassShortName, ObjectRef = null };
                    if (elementsByType.ContainsKey(elementClass))
                    {
                        if (elementClass == "StatData")
                        {

                            foreach (gamedataObject element in elementsByType["StatGroupData"])
                            {
                                if (element.deleted != null && element.deleted != "True")
                                {
                                    GameTreeItem statGroupNode = new GameTreeItem() { Name = element.Name, ObjectRef = null, Id=element.uuid };
                                    foreach (gamedataObject statElement in elementsByType["StatData"])
                                    {
                                        if (statElement.deleted != null && statElement.deleted != "True" && statElement.statGroupID == element.uuid)
                                        {
                                            GameTreeItem statNode = new GameTreeItem() { ObjectRef = statElement };
                                            statGroupNode.Children.Add(statNode);
                                        }
                                    }
                                    classNode.Children.Add(statGroupNode);
                                }
                            }
                            {

                                GameTreeItem statGroupNode = new GameTreeItem() { Name = "(none)", ObjectRef = null };
                                foreach (gamedataObject statElement in elementsByType["StatData"])
                                {
                                    if (statElement.deleted != null && statElement.deleted != "True" && statElement.statGroupID == "")
                                    {
                                        GameTreeItem statNode = new GameTreeItem() { ObjectRef = statElement };
                                        statGroupNode.Children.Add(statNode);
                                    }
                                }
                                classNode.Children.Add(statGroupNode);
                            }
                        }
                        else
                        {
                            foreach (gamedataObject element in elementsByType[elementClass])
                            {
                                if (element.deleted != null && element.deleted != "True")
                                {
                                    GameTreeItem childNode = new GameTreeItem() { ObjectRef = element };
                                    classNode.Children.Add(childNode);
                                }
                            }
                        }
                    }
                    GameTree.Add(classNode);
                }
                PostLoad = true;
                NotifyPropertyChanged("gameTree");
            }
        }
        public void ReturnToFlowchartView()
        {
            SelectedQuestStep = null;
        }
        public gamedataObject SelectedQuestStep
        {
            get
            {
                if (_selectedQuestStep != null && gameDataObj != null && gameDataObj.@class.Equals("QuestData"))
                {
                    return _selectedQuestStep;
                }
                else { return null; }
            }
            set
            {
                if (_selectedQuestStep != value)
                {
                    _selectedQuestStep = value;
                    selectedQuestStepChoicesObservable.Clear();
                    selectedQuestStepStatGrantsObservable.Clear();
                    selectedQuestStepItemGrantsObservable.Clear();
                    selectedQuestStepHenchmanGrantsObservable.Clear();
                    NotifyPropertyChanged("selectedQuestStepChoicesObservable");
                    NotifyPropertyChanged("selectedQuestStepStatGrantsObservable");
                    NotifyPropertyChanged("selectedQuestStepItemGrantsObservable");
                    NotifyPropertyChanged("selectedQuestStepHenchmanGrantsObservable");
                    selectedQuestStepChoicesObservable = new ObservableCollection<gamedataObject>();
                    selectedQuestStepStatGrantsObservable = new ObservableCollection<gamedataObject>();
                    selectedQuestStepItemGrantsObservable = new ObservableCollection<gamedataObject>();
                    selectedQuestStepHenchmanGrantsObservable = new ObservableCollection<gamedataObject>();
                    if (value != null)
                    {
                        //Populate the list of quest stepchoices.  Comes after the above which might be fixing a quest step so it gets found here.
                        int sortIndex = 1;
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceData")
                                                                                               && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                               && iter.stepID.Equals(_selectedQuestStep.uuid)))
                        {
                            if (gameObject.sortOrder==0)
                            {//This is a patch for datafiles saved prior to the sort order existing -- add it in if necessary.
                                gameObject.sortOrder = sortIndex;
                                sortIndex++;
                            }
                            selectedQuestStepChoicesObservable.Add(gameObject);

                        }
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepStatGrantData")
                                                                                               && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                               && iter.stepID.Equals(_selectedQuestStep.uuid)))
                        {
                            selectedQuestStepStatGrantsObservable.Add(gameObject);
                        }
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepItemGrantData")
                                                                                               && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                               && iter.stepID.Equals(_selectedQuestStep.uuid)))
                        {
                            selectedQuestStepItemGrantsObservable.Add(gameObject);
                        }
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepHenchmanGrantData")
                                                                                               && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                               && iter.stepID.Equals(_selectedQuestStep.uuid)))
                        {
                            selectedQuestStepHenchmanGrantsObservable.Add(gameObject);
                        }
                    }
                    NotifyPropertyChanged("questFlowchartVisible");
                    NotifyPropertyChanged("questStepPanelVisible");
                    NotifyPropertyChanged("stepName");
                    NotifyPropertyChanged("stepDescription");
                    NotifyPropertyChanged("associatedLocation");
                    NotifyPropertyChanged("moveToLocation");
                    NotifyPropertyChanged("unlockLocation");
                    NotifyPropertyChanged("associatedNPC");
                    NotifyPropertyChanged("alternateBackground");
                    NotifyPropertyChanged("SelectedQuestStep");
                }
            }
        }
        private string findQuestByStat;
        public string FindQuestByStat
        {
            get
            { return findQuestByStat; }
            set
            {
                findQuestByStat = value;
         
                foreach (GameTreeItem iter in GameTree)
                {
                    if (iter.Name.Equals("Quest"))
                    {
                        foreach (GameTreeItem inneriter in iter.Children)
                        {
                            if (value=="")
                            {
                                inneriter.MinStatValue = 0;
                                inneriter.MaxStatValue = 0;
                            }
                            else
                            {
                                inneriter.MinStatValue = 0;
                                inneriter.MaxStatValue = 0;
                                foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(x => x.@class.Equals("QuestStatRequirementData")
                                                                                                       && x.deleted.Equals("False")
                                                                                                       && x.questID.Equals(inneriter.ObjectRef.uuid)
                                                                                                       && x.statID.Equals(value)))
                                {
                                    Int32 temp;
                                    if (Int32.TryParse(gameObject.minimum, out temp))
                                    {
                                        inneriter.MinStatValue = temp;
                                    }
                                    else { inneriter.MinStatValue = 0; }
                                    if (Int32.TryParse(gameObject.maximum, out temp))
                                    {
                                        inneriter.MaxStatValue = temp;
                                    }
                                    else { inneriter.MaxStatValue = 0; }
                                }
                            }

                        }
                        if (value == "")
                        {
                            List<GameTreeItem> temp = iter.Children.OrderBy(g => g.Name).ToList<GameTreeItem>();
                            ObservableCollection<GameTreeItem> sortedList = new ObservableCollection<GameTreeItem>();
                            temp.ForEach(x => sortedList.Add(x));
                            iter.Children = sortedList;
                            iter.NotifyPropertyChanged("Children");
                        }
                        else
                        {
                            List<GameTreeItem> temp = iter.Children.OrderBy(g => g.MinValueSorter ).ToList<GameTreeItem>();
                            ObservableCollection<GameTreeItem> sortedList = new ObservableCollection<GameTreeItem>();
                            temp.ForEach(x => sortedList.Add(x));
                            iter.Children=sortedList;
                            iter.NotifyPropertyChanged("Children");
                        }
                        break;
                    }
                }

            }
        }
        public bool questStepChoiceDataVisible
        {
            get
            {
                if (_selectedQuestStepChoice != null && _selectedQuestStep != null && gameDataObj != null && gameDataObj.@class.Equals("QuestData"))
                {
                    return true;
                }
                return false;
            }
        }
        public gamedataObject SelectedQuestStepChoice
        {
            get
            {
                if (_selectedQuestStepChoice != null && gameDataObj != null && gameDataObj.@class == "QuestData")
                {
                    return _selectedQuestStepChoice;
                }
                else { return null; }
            }
            set
            {
                if (_selectedQuestStepChoice != value)
                {
                    _selectedQuestStepChoice = value;
                    if (value != null)
                    {
                        //Update the various observables that depend on this
                        selectedQuestStepChoiceStatRequirementsObservable.Clear();
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceStatRequirementData")
                                                                                               && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                               && iter.questStepChoiceID.Equals(_selectedQuestStepChoice.uuid)))
                        {
                            selectedQuestStepChoiceStatRequirementsObservable.Add(gameObject);
                        }
                        selectedQuestStepChoiceStatGrantsObservable.Clear();
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceStatGrantData")
                                                                                               && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                               && iter.questStepChoiceID.Equals(_selectedQuestStepChoice.uuid)))
                        {
                            selectedQuestStepChoiceStatGrantsObservable.Add(gameObject);
                        }
                        selectedQuestStepChoiceItemGrantsObservable.Clear();
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceItemGrantData")
                                                                                               && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                               && iter.questStepChoiceID.Equals(_selectedQuestStepChoice.uuid)))
                        {
                            selectedQuestStepChoiceItemGrantsObservable.Add(gameObject);
                        }
                        selectedQuestStepChoiceHenchmanGrantsObservable.Clear();
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceHenchmanGrantData")
                                                                                               && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                               && iter.questStepChoiceID.Equals(_selectedQuestStepChoice.uuid)))
                        {
                            selectedQuestStepChoiceHenchmanGrantsObservable.Add(gameObject);
                        }
                    }
                    NotifyPropertyChanged("selectedQuestStepChoiceHenchmanGrantsObservable");
                    NotifyPropertyChanged("selectedQuestStepChoiceStatRequirementsObservable");
                    NotifyPropertyChanged("selectedQuestStepChoiceStatGrantsObservable");
                    NotifyPropertyChanged("selectedQuestStepChoiceItemGrantsObservable");
                    NotifyPropertyChanged("questStepChoiceDataVisible");
                }
            }
        }
        public gamedataObject GetItemByID(string id)
        {
            if (id == null)
            {
                return null;
            }
            if (allObjects.ContainsKey(id))
            {
                var retVal = allObjects[id];
                return retVal;
            }
            else return null;
        }

        public gamedataObject gameData
        {
            get { return gameDataObj; }
            set
            {
                if (gameDataObj != value)
                {

                    SelectedQuestStepChoice = null;
                    SelectedQuestStep = null;
                    gameDataObj = value;
                    if (gameDataObj != null && gameDataObj.@class.Equals("ItemData"))
                    {
                        //If this is an item, populate the list of relevant item stat modifiers.
                        itemStatModifiersObservable.Clear();
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("ItemStatModifierData") && iter.itemID.Equals(gameDataObj.uuid) && iter.deleted.Equals("False")))
                        {
                            itemStatModifiersObservable.Add(gameObject);
                        }
                    }
                    if (gameDataObj != null && gameDataObj.@class.Equals("StartingCharacterInfoData"))
                    {
                        startingCharacterStatModifiersObservable.Clear();
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("StartingCharacterInfoStatModifierData") && iter.startingCharacterInfoID.Equals(gameDataObj.uuid) && iter.deleted.Equals("False")))
                        {
                            startingCharacterStatModifiersObservable.Add(gameObject);
                        }
                    }
                    if (gameDataObj != null && gameDataObj.@class.Equals("HenchmanData"))
                    {
                        //If this is an henchman, populate the list of relevant henchman stats.
                        henchmanStats.Clear();
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("HenchmanStatData") && iter.henchmanID.Equals(gameDataObj.uuid) && iter.deleted.Equals("False")))
                        {
                            henchmanStats.Add(gameObject);
                        }
                    }
                    if (gameDataObj != null && gameDataObj.@class.Equals("QuestData"))
                    {
                        //If this is an quest, populate the list of relevant quest stat requirements.
                        questStatRequirementsObservable.Clear();
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStatRequirementData")
                                                                                               && iter.deleted.Equals("False")
                                                                                               && iter.questID.Equals(gameDataObj.uuid)))
                        {
                            questStatRequirementsObservable.Add(gameObject);
                        }
                        questItemRequirementsObservable.Clear();
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestItemRequirementData")
                                                                                               && iter.deleted.Equals("False")
                                                                                               && iter.questID.Equals(gameDataObj.uuid)))
                        {
                            questItemRequirementsObservable.Add(gameObject);
                        }
                        _selectedQuestStep = null; //Upon selecting a quest you should initially go to flowchart view.

                        //This is here to fix up quests from an older version of the editor where the first step might not have gotten a quest uuid properly: if there is a quest step equal to our first step but it doesn't realize we are its quest, let it know.
                        foreach (gamedataObject questStepObj in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepData") && iter.uuid.Equals(gameDataObj.firstStepID)))
                        {
                            if (questStepObj.quest == null)
                            {
                                questStepObj.questID = gameDataObj.uuid;
                            }
                        }
                        //Populate the list of quest steps.  Comes after the above which might be fixing a quest step so it gets found here.
                        selectedQuestStepsObservable.Clear();
                        gamedataObject nullStep = new gamedataObject();
                        nullStep.uuid = "";
                        nullStep.name = "(End Quest)";
                        selectedQuestStepsObservable.Add(nullStep); //Empty value so you can "unselect" things in the combo boxes
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepData") && iter.questID.Equals(gameDataObj.uuid) && iter.deleted.Equals("False")))
                        {
                            selectedQuestStepsObservable.Add(gameObject);
                        }
                    }
                    OnGameDataChanged(EventArgs.Empty);
                }
            }
        }

        protected void OnGameDataChanged(EventArgs e)
        {
            NotifyPropertyChanged("SelectedObject");
            NotifyPropertyChanged("pickNewObjectVisible");
            NotifyPropertyChanged("name");
            NotifyPropertyChanged("nameVisible");
            NotifyPropertyChanged("icon");
            NotifyPropertyChanged("iconVisible");
            NotifyPropertyChanged("comment");
            NotifyPropertyChanged("description");
            NotifyPropertyChanged("descriptionVisible");
            NotifyPropertyChanged("equippable");
            NotifyPropertyChanged("equippableVisible");
            NotifyPropertyChanged("itemType");
            NotifyPropertyChanged("subLocationOf");
            NotifyPropertyChanged("requiredLocation");
            NotifyPropertyChanged("startingLocation");
            NotifyPropertyChanged("associatedLocation");
            NotifyPropertyChanged("moveToLocation");
            NotifyPropertyChanged("alternateBackground");
            NotifyPropertyChanged("unlockLocation");
            NotifyPropertyChanged("associatedNPC");
            NotifyPropertyChanged("startingQuest");
            NotifyPropertyChanged("locationVisible");
            NotifyPropertyChanged("itemTypeVisible");
            NotifyPropertyChanged("itemStatModifiers");
            NotifyPropertyChanged("itemStatModifiersObservable");
            NotifyPropertyChanged("itemStatModifiersVisible");
            NotifyPropertyChanged("questStatRequirements");
            NotifyPropertyChanged("questStatRequirementsObservable");
            NotifyPropertyChanged("questStatRequirementsVisible");
            NotifyPropertyChanged("questFlowchartVisible");
            NotifyPropertyChanged("questStepPanelVisible");
            NotifyPropertyChanged("statGroupVisible");
            NotifyPropertyChanged("henchmanVisible");
            NotifyPropertyChanged("questVisible");
            NotifyPropertyChanged("characterStartingInfoVisible");
            NotifyPropertyChanged("deleteObjectButtonVisible");
            NotifyPropertyChanged("deleteObjectButtonTooltip");
            NotifyPropertyChanged("deleteObjectButtonEnabled");
            NotifyPropertyChanged("deleteObjectButtonTextColor");
            NotifyPropertyChanged("oneTimeQuest");
            NotifyPropertyChanged("cooldownTimer");
            NotifyPropertyChanged("questStepChoiceDataVisible");
            NotifyPropertyChanged(null); //This supposedly forces a notify on all properties, but appears not to work, possibly due to old version of .net?
        }


        public bool canCreateStartingCharacterInfo
        {
            get
            {
                if (root == null)
                {
                    return false;
                }
                foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => (iter.@class.Equals("StartingCharacterInfoData"))))
                {
                    return false;
                }
                return true;
            }
        }

        public bool pickNewObjectVisible { get { return (gameDataObj == null) && PostLoad; } }

        public string name
        {
            get
            {

                if (gameDataObj == null)

                {
                    return "";
                }
                else
                {
                    return gameDataObj.name;
                }

            }
            set
            {
                if (gameDataObj != null)
                {
                    gameDataObj.name = value;
                    gameDataObj.NotifyPropertyChanged("name");
                    SelectedGameTreeItem.NotifyPropertyChanged("Name");
                    if (gameDataObj.@class=="StatGroupData")
                    {
                        foreach (GameTreeItem outerItem in gameTree)
                        {
                            if (outerItem.Name=="Stat")
                            {
                                foreach (GameTreeItem innerItem in outerItem.Children)
                                {
                                    if (innerItem.Id == gameDataObj.uuid)
                                    {
                                        innerItem.Name = gameDataObj.Name;
                                    }
                                }
                            }
                        }
                    }
                    //updateObjectTreeForNameChange();
                }
            }
        }
        public string stepName
        {
            get
            {
                if (gameDataObj == null || SelectedQuestStep == null)
                {
                    return "";
                }
                else
                {
                    return SelectedQuestStep.Name;
                };
            }
            set
            {
                SelectedQuestStep.Name = value;
                //It's supposed to be automatic, but actually you have to press this button.
                //...The "Next Step" combo box doesn't refresh on name-changed events of individual objects, but does 
                //on objects being added/removed from the set.  So when you change the step name, the combo box only
                //shows that if you remove the step and re-add it.  Sigh.
                selectedQuestStepsObservable.Remove(SelectedQuestStep);
                selectedQuestStepsObservable.Add(SelectedQuestStep);
            }
        }
        public bool nameVisible
        {
            get
            {
                if (gameDataObj == null) { return false; }
                //For the moment, all object types have names.
                return true;
            }
        }

        public bool iconVisible
        {
            get
            {
                if (gameDataObj == null) { return false; }
                if (gameDataObj.@class.Equals("ItemData") || gameDataObj.@class.Equals("LocationData") || gameDataObj.@class.Equals("HenchmanData") || gameDataObj.@class.Equals("NPCData"))
                { return true; }
                else
                { return false; }
            }
        }
        public string comment { get { if (gameDataObj == null) { return ""; } else return gameDataObj.comment; } set { if (gameDataObj != null) gameDataObj.comment = value; } }
        public string icon { get { if (gameDataObj == null) { return ""; } else return gameDataObj.icon; } set { if (gameDataObj != null) gameDataObj.icon = value; } }
        public string alternateBackground { get { if (gameDataObj == null || SelectedQuestStep == null) { return ""; } else return SelectedQuestStep.alternateBackground; } set { if (SelectedQuestStep != null) SelectedQuestStep.alternateBackground = value; NotifyPropertyChanged("alternateBackground"); } }

         public string Description { get { if (gameDataObj == null || gameDataObj.description == null) { return ""; } else return WebUtility.HtmlDecode(gameDataObj.description); } set { if (gameDataObj != null) gameDataObj.description = WebUtility.HtmlEncode(value); NotifyPropertyChanged("Description"); } }

        public string questPreDescription { get { if (gameDataObj == null) { return ""; } else return WebUtility.HtmlDecode(gameDataObj.questPreDescription); } set { if (gameDataObj != null) gameDataObj.questPreDescription = WebUtility.HtmlEncode(value); NotifyPropertyChanged("questPreDescription"); } }
        public string stepDescription
        {
            get { if (gameDataObj == null || SelectedQuestStep == null) { return ""; } else return WebUtility.HtmlDecode(SelectedQuestStep.description); }
            set { if (SelectedQuestStep != null) SelectedQuestStep.description = WebUtility.HtmlEncode(value); NotifyPropertyChanged("stepDescription"); }
        }
        public bool descriptionVisible
        {
            get
            {
                if (gameDataObj == null)
                { return false; }
                else
                { if (gameDataObj.@class.Equals("StartingCharacterInfoData")) { return false; } return true; }
            }
        }
        public bool? equippable
        {
            get
            {
                if (gameDataObj == null)
                {
                    return false;
                }
                else
                {
                    return gameDataObj.equippable.Equals("True");
                }
            }
            set
            {
                if (gameDataObj != null)
                {
                    if (value == true)
                    { gameDataObj.equippable.Equals("True"); }
                    else
                    { gameDataObj.equippable.Equals("False"); }
                }
            }
        }
        public bool? oneTimeQuest
        {
            get
            {
                if (gameDataObj == null)
                {
                    return false;
                }
                else
                {
                    return gameDataObj.oneTimeQuest.Equals("True");
                }
            }
            set
            {
                if (gameDataObj != null)
                {
                    if (value == true)
                    { gameDataObj.oneTimeQuest = "True"; }
                    else
                    { gameDataObj.oneTimeQuest = "False"; }
                }
            }
        }
        public bool? questIsForceGranted
        {
            get
            {
                if (gameDataObj == null)
                {
                    return false;
                }
                else
                {
                    return gameDataObj.questIsForceGranted.Equals("True");
                }
            }
            set
            {
                if (gameDataObj != null)
                {
                    if (value == true)
                    { gameDataObj.questIsForceGranted = "True"; }
                    else
                    { gameDataObj.questIsForceGranted = "False"; }
                }
            }
        }
        public bool? displayedOnSidebar
        {
            get
            {
                if (gameDataObj == null)
                {
                    return false;
                }
                else
                {
                    return gameDataObj.displayedOnSidebar.Equals("True");
                }
            }
            set
            {
                if (gameDataObj != null)
                {
                    if (value == true)
                    { gameDataObj.displayedOnSidebar = "True"; }
                    else
                    { gameDataObj.displayedOnSidebar = "False"; }
                }
            }
        }
        public bool? displayedOnMainStatPage
        {
            get
            {
                if (gameDataObj == null)
                {
                    return false;
                }
                else
                {
                    return gameDataObj.displayedOnMainStatPage.Equals("True");
                }
            }
            set
            {
                if (gameDataObj != null)
                {
                    if (value == true)
                    { gameDataObj.displayedOnMainStatPage = "True"; }
                    else
                    { gameDataObj.displayedOnMainStatPage = "False"; }
                }
            }
        }
        public string questEnergyCost
        {
            get
            {
                if (gameDataObj == null)
                {
                    return "0";
                }
                else
                {
                    return gameDataObj.questEnergyCost;
                }
            }
            set
            {
                if (gameDataObj != null)
                {
                    int temp;
                    if (Int32.TryParse(value, out temp))
                    {
                        gameDataObj.questEnergyCost = Math.Max(temp, 0).ToString();
                    }
                    else
                    {
                        gameDataObj.questEnergyCost = "0";
                    }
                }
            }
        }
        public string cooldownTimer
        {
            get
            {
                if (gameDataObj == null)
                {
                    return "0";
                }
                else
                {
                    return gameDataObj.cooldownTimer;
                }
            }
            set
            {
                if (gameDataObj != null)
                {
                    int temp;
                    if (Int32.TryParse(value, out temp))
                    {
                        gameDataObj.cooldownTimer = Math.Max(temp, 0).ToString();
                    }
                    else
                    {
                        gameDataObj.cooldownTimer = "0";
                    }
                }
            }
        }
        public bool equippableVisible
        {
            get
            {
                if (gameDataObj == null) { return false; }
                if (gameDataObj.@class.Equals("ItemTypeData"))
                { return true; }
                else
                { return false; }
            }
        }
        public bool questVisible
        {
            get
            {
                if (gameDataObj == null) { return false; }
                if (gameDataObj.@class.Equals("QuestData"))
                { return true; }
                else
                { return false; }
            }
        }
        public bool characterStartingInfoVisible
        {
            get
            {
                if (gameDataObj == null) { return false; }
                if (gameDataObj.@class.Equals("StartingCharacterInfoData"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool henchmanVisible
        {
            get
            {
                if (gameDataObj == null) { return false; }
                if (gameDataObj.@class.Equals("HenchmanData"))
                { return true; }
                else
                { return false; }
            }
        }
        public string subLocationOf
        {
            get { if (gameDataObj == null || gameDataObj.subLocationOfID == null) { return ""; } else return gameDataObj.subLocationOfID; }
            set
            {
                if (value == null)
                    gameDataObj.subLocationOfID = "";
                else
                    gameDataObj.subLocationOfID = value;
            }
        }
        public string requiredLocation
        {
            get { if (gameDataObj == null || gameDataObj.requiredLocationID == null) { return ""; } else return gameDataObj.requiredLocationID; }
            set
            {
                if (value == null)
                    gameDataObj.requiredLocationID = "";
                else
                    gameDataObj.requiredLocationID = value;
            }
        }
        public string startingLocation
        {
            get { if (gameDataObj == null || gameDataObj.startingLocationID == null) { return ""; } else return gameDataObj.startingLocationID; }
            set
            {
                if (gameDataObj != null)
                {
                    gameDataObj.startingLocationID = value;
                }
            }
        }
        public string associatedLocation
        {
            get { if (_selectedQuestStep == null || _selectedQuestStep.associatedLocationID==null) { return ""; } else return _selectedQuestStep.associatedLocationID; }
            set
            {
                _selectedQuestStep.associatedLocationID = value;
            }
        }
        public string moveToLocation
        {
            get { if (_selectedQuestStep == null || _selectedQuestStep.moveToLocationID==null) { return ""; } else return _selectedQuestStep.moveToLocationID; }
            set
            {
                _selectedQuestStep.moveToLocationID = value;
            }
        }
        public string unlockLocation
        {
            get { if (_selectedQuestStep == null || _selectedQuestStep.unlockLocationID==null) { return ""; } else return _selectedQuestStep.unlockLocationID; }
            set
            {
                _selectedQuestStep.unlockLocationID = value;
            }
        }
        public string associatedNPC
        {
            get { if (_selectedQuestStep == null || _selectedQuestStep.associatedNPCID==null) { return ""; } else return _selectedQuestStep.associatedNPCID; }
            set
            {
                _selectedQuestStep.associatedNPCID = value;
            }
        }
        public string startingQuest
        {
            get { if (gameDataObj == null || gameDataObj.startingQuestID == null) { return ""; } else return gameDataObj.startingQuestID; }
            set
            {
                if (gameDataObj != null)
                {
                    gameDataObj.startingQuestID = value;
                }
            }
        }
        public string itemType
        {
            get { if (gameDataObj == null) { return ""; } else return gameDataObj.itemTypeID; }
            set
            {
                gameDataObj.itemTypeID = value;
            }
        }
        public string xpStat
        {
            get { if (gameDataObj == null) { return ""; } else return gameDataObj.xpStatID; }
            set
            {
                gameDataObj.xpStatID = value;
            }
        }
        public string statGroup
        {
            get { if (gameDataObj == null) { return ""; } else return gameDataObj.statGroupID; }
            set
            {
                gameDataObj.statGroupID = value;
                //On stat group changing, the selected stat needs to jump to a different stat group category.
                GameTreeItem localGameTreeItem = selectedGameTreeItem;
                foreach (GameTreeItem iter in GameTree)
                {
                    if (iter.Name.Equals("Stat"))
                    {
                        foreach (GameTreeItem statGroupIter in iter.Children)
                        {
                            if (statGroupIter.Children.Contains(selectedGameTreeItem))
                            {
                                statGroupIter.Children.Remove(selectedGameTreeItem);
                            }
                        }
                        foreach (GameTreeItem statGroupIter in iter.Children)
                        {
                            if (statGroupIter.Id.Equals(value))
                            {
                                statGroupIter.Children.Add(localGameTreeItem);
                            }
                        }
                        selectedGameTreeItem = localGameTreeItem;//We need to reselect the item, because pulling it out of the tree un-selected it!
                    }
                }
            }
        }
        public bool locationVisible
        {
            get
            {
                if (gameDataObj == null) { return false; }
                if (gameDataObj.@class.Equals("LocationData"))
                { return true; }
                else
                { return false; }
            }
        }
        public bool itemTypeVisible
        {
            get
            {
                if (gameDataObj == null) { return false; }
                if (gameDataObj.@class.Equals("ItemData"))
                { return true; }
                else
                { return false; }
            }
        }
        public bool statVisible
        {
            get
            {
                if (gameDataObj == null) { return false; }
                if (gameDataObj.@class.Equals("StatData"))
                { return true; }
                else
                { return false; }
            }
        }
        public bool statGroupVisible
        {
            get
            {
                if (gameDataObj == null) { return false; }
                if (gameDataObj.@class.Equals("StatGroupData"))
                { return true; }
                else
                { return false; }
            }
        }
        private ObservableCollection<gamedataObject> _henchmanStats = new ObservableCollection<gamedataObject>();
        public ObservableCollection<gamedataObject> henchmanStats
        {
            get { return _henchmanStats; }
            set
            {
                _henchmanStats = value;
                NotifyPropertyChanged("henchmanStats");
            }
        }
        private ObservableCollection<gamedataObject> _selectedQuestStepChoicesObservable;
        public ObservableCollection<gamedataObject> selectedQuestStepChoicesObservable
        {
            get { return _selectedQuestStepChoicesObservable; }
            set
            {
                _selectedQuestStepChoicesObservable = value;
                NotifyPropertyChanged("selectedQuestStepChoicesObservable");
            }
        }
        private ObservableCollection<gamedataObject> _selectedQuestStepChoiceStatRequirementsObservable;
        public ObservableCollection<gamedataObject> selectedQuestStepChoiceStatRequirementsObservable
        {
            get { return _selectedQuestStepChoiceStatRequirementsObservable; }
            set
            {
                _selectedQuestStepChoiceStatRequirementsObservable = value;
                NotifyPropertyChanged("selectedQuestStepChoiceStatRequirementsObservable");
            }
        }
        private ObservableCollection<gamedataObject> _selectedQuestStepChoiceItemGrantsObservable;
        public ObservableCollection<gamedataObject> selectedQuestStepChoiceItemGrantsObservable
        {
            get { return _selectedQuestStepChoiceItemGrantsObservable; }
            set
            {
                _selectedQuestStepChoiceItemGrantsObservable = value;
                NotifyPropertyChanged("selectedQuestStepChoiceItemGrantsObservable");
            }
        }
        private ObservableCollection<gamedataObject> _selectedQuestStepChoiceStatGrantsObservable;
        public ObservableCollection<gamedataObject> selectedQuestStepChoiceStatGrantsObservable
        {
            get { return _selectedQuestStepChoiceStatGrantsObservable; }
            set
            {
                _selectedQuestStepChoiceStatGrantsObservable = value;
                NotifyPropertyChanged("selectedQuestStepChoiceStatGrantsObservable");
            }
        }
        private ObservableCollection<gamedataObject> _selectedQuestStepChoiceHenchmanGrantsObservable;
        public ObservableCollection<gamedataObject> selectedQuestStepChoiceHenchmanGrantsObservable
        {
            get { return _selectedQuestStepChoiceHenchmanGrantsObservable; }
            set
            {
                _selectedQuestStepChoiceHenchmanGrantsObservable = value;
                NotifyPropertyChanged("selectedQuestStepChoiceHenchmanGrantsObservable");
            }
        }
        private ObservableCollection<gamedataObject> _selectedQuestStepItemGrantsObservable;
        public ObservableCollection<gamedataObject> selectedQuestStepItemGrantsObservable
        {
            get { return _selectedQuestStepItemGrantsObservable; }
            set
            {
                _selectedQuestStepItemGrantsObservable = value;
                NotifyPropertyChanged("selectedQuestStepItemGrantsObservable");
            }
        }
        private ObservableCollection<gamedataObject> _selectedQuestStepStatGrantsObservable;
        public ObservableCollection<gamedataObject> selectedQuestStepStatGrantsObservable
        {
            get { return _selectedQuestStepStatGrantsObservable; }
            set
            {
                _selectedQuestStepStatGrantsObservable = value;
                NotifyPropertyChanged("selectedQuestStepStatGrantsObservable");
            }
        }
        private ObservableCollection<gamedataObject> _selectedQuestStepHenchmanGrantsObservable;
        public ObservableCollection<gamedataObject> selectedQuestStepHenchmanGrantsObservable
        {
            get { return _selectedQuestStepHenchmanGrantsObservable; }
            set
            {
                _selectedQuestStepHenchmanGrantsObservable = value;
                NotifyPropertyChanged("selectedQuestStepHenchmanGrantsObservable");
            }
        }
        private ObservableCollection<gamedataObject> _questStatRequirementsObservable;
        public ObservableCollection<gamedataObject> questStatRequirementsObservable
        {
            get { return _questStatRequirementsObservable; }
            set
            {
                _questStatRequirementsObservable = value;
                NotifyPropertyChanged("questStatRequirementsObservable");
            }
        }
        private ObservableCollection<gamedataObject> _selectedQuestStepsObservable;
        public ObservableCollection<gamedataObject> selectedQuestStepsObservable
        {
            get { return _selectedQuestStepsObservable; }
            set
            {
                _selectedQuestStepsObservable = value;
                NotifyPropertyChanged("questStepsObservable");
            }
        }
        private ObservableCollection<gamedataObject> _questItemRequirementsObservable;
        public ObservableCollection<gamedataObject> questItemRequirementsObservable
        {
            get { return _questItemRequirementsObservable; }
            set
            {
                _questItemRequirementsObservable = value;
                NotifyPropertyChanged("questItemRequirementsObservable");
            }
        }
        private ObservableCollection<gamedataObject> _itemStatModifiersObservable;
        public ObservableCollection<gamedataObject> itemStatModifiersObservable
        {
            get { return _itemStatModifiersObservable; }
            set
            {
                _itemStatModifiersObservable = value;
                NotifyPropertyChanged("itemStatModifiersObservable");
            }
        }
        private ObservableCollection<gamedataObject> _startingCharacterStatModifiersObservable;
        public ObservableCollection<gamedataObject> startingCharacterStatModifiersObservable
        {
            get { return _startingCharacterStatModifiersObservable; }
            set
            {
                _startingCharacterStatModifiersObservable = value;
                NotifyPropertyChanged("startingCharacterStatModifiersObservable");
            }
        }
        public bool itemStatModifiersVisible
        {
            get
            {
                if (gameDataObj == null) { return false; }
                if (gameDataObj.@class.Equals("ItemData"))
                { return true; }
                else
                { return false; }
            }
        }
        public bool questStatRequirementsVisible
        {
            get
            {
                if (gameDataObj == null) { return false; }
                if (gameDataObj.@class.Equals("QuestData"))
                { return true; }
                else
                { return false; }
            }
        }
        public bool questFlowchartVisible
        {
            get
            {
                if (gameDataObj == null) { return false; }
                if (gameDataObj.@class.Equals("QuestData"))
                {
                    if (_selectedQuestStep == null)
                    { return true; }
                    else
                    { return false; }
                }
                else
                { return false; }
            }

        }
        public bool questStepPanelVisible
        {
            get
            {
                if (gameDataObj == null) { return false; }
                if (gameDataObj.@class.Equals("QuestData"))
                {
                    if (_selectedQuestStep == null)
                    { return false; }
                    else
                    { return true; }
                }
                else
                { return false; }
            }

        }
        public void addNewObject(string objectType)
        {
            gamedataObject newObj = new gamedataObject();
            newObj.@class = objectType + "Data";
            newObj.name = "New " + objectType;
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(newObj);
            root.Items = newRootItems.ToArray();
            allObjects[newObj.uuid] = newObj;
            if (objectType.Equals("Location"))
            {
                locations.Add(newObj);
            }
            if (objectType.Equals("Henchman"))
            {
                henchmen.Add(newObj);
            }
            if (objectType.Equals("Item"))
            {
                items.Add(newObj);
            }
            if (objectType.Equals("ItemType"))
            {
                itemTypes.Add(newObj);
            }
            if (objectType.Equals("Stat"))
            {
                stats.Add(newObj);
            }
            if (objectType.Equals("StatGroup"))
            {
                statGroups.Add(newObj);
                //Also add category to stats on the tree
                foreach (GameTreeItem headernode in gameTree)
                {
                    if (headernode.Name == "Stat")
                    {
                        GameTreeItem statGroupNode = new GameTreeItem() { Name = newObj.Name, ObjectRef = null, Id = newObj.uuid };
                        headernode.Children.Add(statGroupNode);
                    }
                }

            }
            if (objectType.Equals("Quest"))
            {
                quests.Add(newObj);
            }
            if (objectType.Equals("NPC"))
            {
                npcs.Add(newObj);
            }
            if (objectType.Equals("StartingCharacterInfo"))
            {
                NotifyPropertyChanged("canCreateStartingCharacterInfo");
            }
            SelectNewObject(newObj, objectType);
        }
        private void SelectNewObject(gamedataObject newObj, string objectType)
        {
            //This sets the newly created object as the selected object in the tree view
            bool foundTreeItem = false;
            GameTreeItem childNode = new GameTreeItem() { ObjectRef = newObj };
            foreach (GameTreeItem iter in GameTree)
            {
                if (objectType.Equals("Stat"))
                {
                    if (iter.Name.Equals("Stat"))
                    {
                        foreach (GameTreeItem statGroupIter in iter.Children)
                        {
                            if (statGroupIter.Id.Equals(""))
                            {
                                foundTreeItem = true;
                                statGroupIter.Children.Add(childNode);
                                break;
                            }
                        }
                        if (!foundTreeItem)
                        {
                            GameTreeItem statGroupNode = new GameTreeItem() { Name = "(none)", ObjectRef = null, Id = null };
                            statGroupNode.Children.Add(childNode);
                            iter.Children.Add(statGroupNode);
                            foundTreeItem = true;
                        }
                    }
                }
                else
                {
                    if (iter.Name.Equals(objectType))
                    {
                        foundTreeItem = true;
                        iter.Children.Add(childNode);
                        break;
                    }
                }
            }
            if (!foundTreeItem)
            {
                GameTreeItem classNode = new GameTreeItem() { Name = objectType, Children = { childNode } };
                GameTree.Add(classNode);
            }
            SelectedGameTreeItem = childNode;
        }
        public void addNewHenchmanStat()
        {
            gamedataObject newHenchmanStat = new gamedataObject();
            newHenchmanStat.@class = "HenchmanStatData";
            newHenchmanStat.henchmanID = gameData.uuid;
            newHenchmanStat.statID = stats.First().uuid;
            newHenchmanStat.value = "0";
            _henchmanStats.Add(newHenchmanStat);
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(newHenchmanStat);
            root.Items = newRootItems.ToArray();
            allObjects[newHenchmanStat.uuid] = newHenchmanStat;
        }
        public void addNewItemStatModifier()
        {
            gamedataObject newStatMod = new gamedataObject();
            newStatMod.@class = "ItemStatModifierData";
            newStatMod.itemID = gameData.uuid;
            addStatModifier(newStatMod);
            _itemStatModifiersObservable.Add(newStatMod);
        }
        public void addNewStartingCharacterInfoStatModifier()
        {
            gamedataObject newStatMod = new gamedataObject();
            newStatMod.@class = "StartingCharacterInfoStatModifierData";
            newStatMod.startingCharacterInfoID = gameData.uuid;
            addStatModifier(newStatMod);
            _startingCharacterStatModifiersObservable.Add(newStatMod);
        }
        private void addStatModifier(gamedataObject newStatMod)
        {
            newStatMod.itemID = gameData.uuid;
            newStatMod.statID = stats.First().uuid;
            newStatMod.value = "0";
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(newStatMod);
            root.Items = newRootItems.ToArray();
            allObjects[newStatMod.uuid] = newStatMod;
        }
        public void addNewQuestStatRequirement()
        {
            gamedataObject newStatReq = new gamedataObject();
            newStatReq.@class = "QuestStatRequirementData";
            newStatReq.questID = gameData.uuid;
            newStatReq.statID = stats.First().uuid;
            newStatReq.minimum = "0";
            newStatReq.maximum = "0";
            _questStatRequirementsObservable.Add(newStatReq);
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(newStatReq);
            root.Items = newRootItems.ToArray();
            allObjects[newStatReq.uuid] = newStatReq;
        }
        public void addNewQuestItemRequirement()
        {
            gamedataObject newStatReq = new gamedataObject();
            newStatReq.@class = "QuestItemRequirementData";
            newStatReq.questID = gameData.uuid;
            newStatReq.itemID = items.First().uuid;
            newStatReq.value = "0";
            _questItemRequirementsObservable.Add(newStatReq);
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(newStatReq);
            root.Items = newRootItems.ToArray();
            allObjects[newStatReq.uuid] = newStatReq;
        }
        public void addNewQuestStepChoice()
        {
            gamedataObject newQuestStepChoice = new gamedataObject();
            newQuestStepChoice.@class = "QuestStepChoiceData";
            newQuestStepChoice.stepID = SelectedQuestStep.uuid;
            newQuestStepChoice.name = "New choice";
            newQuestStepChoice.description = "Pick Me!!!";
            int sortIndex = 0;
            foreach (gamedataObject existingChoice in _selectedQuestStepChoicesObservable)
            {
                if (existingChoice.sortOrder >= sortIndex)
                {
                    sortIndex = existingChoice.sortOrder + 1;
                }
            }
            newQuestStepChoice.sortOrder = sortIndex;
            _selectedQuestStepChoicesObservable.Add(newQuestStepChoice);
            ObservableCollection<gamedataObject> temp = _selectedQuestStepChoicesObservable;
            _selectedQuestStepChoicesObservable = temp;
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(newQuestStepChoice);
            root.Items = newRootItems.ToArray();
            allObjects[newQuestStepChoice.uuid] = newQuestStepChoice;
        }

        public void addNewQuestStepChoiceWithData(string inputLine)
        {
            string[] inputData = inputLine.Split(',');
            gamedataObject newQuestStepChoice = new gamedataObject();
            newQuestStepChoice.@class = "QuestStepChoiceData";
            newQuestStepChoice.stepID = SelectedQuestStep.uuid;
            if (inputData.Count() > 0)
            {

                newQuestStepChoice.name = inputData[0];
            }
            else
            {
                newQuestStepChoice.name = "New choice";
            }
            if (inputData.Count() > 1)
            {

                newQuestStepChoice.description = inputData[1];
            }
            else
            {
                newQuestStepChoice.description = "Pick Me!!!";
            }

            if (inputData.Count() > 2)
            {

                foreach (gamedataObject questStepObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepData")
                && iter.questID.Equals(gameDataObj.uuid)
                && iter.deleted != "True"
                && iter.name.Equals(inputData[2])))
                {
                    newQuestStepChoice.nextStepID = questStepObject.uuid;
                }
            }

            if (inputData.Count() > 3)
            {

                foreach (gamedataObject statObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("StatData")
                && iter.deleted != "True"
                && iter.name.Equals(inputData[3])))
                {
                    newQuestStepChoice.statID = statObject.uuid;
                }
            }
            if (inputData.Count() > 4)
            {
                
                    newQuestStepChoice.statTarget = inputData[4];
            }
            if (inputData.Count() > 5)
            {

                foreach (gamedataObject questStepObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepData")
                && iter.questID.Equals(gameDataObj.uuid)
                && iter.deleted != "True"
                && iter.name.Equals(inputData[5])))
                {
                    newQuestStepChoice.failStepID = questStepObject.uuid;
                }
            }
            int sortIndex = 0;
            foreach (gamedataObject existingChoice in _selectedQuestStepChoicesObservable)
            {
                if (existingChoice.sortOrder >= sortIndex)
                {
                    sortIndex = existingChoice.sortOrder + 1;
                }
            }

            newQuestStepChoice.sortOrder = sortIndex;
            _selectedQuestStepChoicesObservable.Add(newQuestStepChoice);
            ObservableCollection<gamedataObject> temp = _selectedQuestStepChoicesObservable;
            _selectedQuestStepChoicesObservable = temp;
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(newQuestStepChoice);
            root.Items = newRootItems.ToArray();
            allObjects[newQuestStepChoice.uuid] = newQuestStepChoice;
        }
        public void addNewQuestStepItemGrant()
        {
            gamedataObject newQuestStepItemGrant = new gamedataObject();
            newQuestStepItemGrant.@class = "QuestStepItemGrantData";
            newQuestStepItemGrant.stepID = SelectedQuestStep.uuid;
            newQuestStepItemGrant.value = "0";
            newQuestStepItemGrant.name = "";
            newQuestStepItemGrant.description = "";
            _selectedQuestStepItemGrantsObservable.Add(newQuestStepItemGrant);
            ObservableCollection<gamedataObject> temp = _selectedQuestStepItemGrantsObservable;
            _selectedQuestStepItemGrantsObservable = temp;
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(newQuestStepItemGrant);
            root.Items = newRootItems.ToArray();
            allObjects[newQuestStepItemGrant.uuid] = newQuestStepItemGrant;
        }
        public void addNewQuestStepStatGrant()
        {
            gamedataObject newQuestStepStatGrant = new gamedataObject();
            newQuestStepStatGrant.@class = "QuestStepStatGrantData";
            newQuestStepStatGrant.stepID = SelectedQuestStep.uuid;
            newQuestStepStatGrant.value = "0";
            _selectedQuestStepStatGrantsObservable.Add(newQuestStepStatGrant);
            ObservableCollection<gamedataObject> temp = _selectedQuestStepStatGrantsObservable;
            _selectedQuestStepStatGrantsObservable = temp;
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(newQuestStepStatGrant);
            root.Items = newRootItems.ToArray();
            allObjects[newQuestStepStatGrant.uuid] = newQuestStepStatGrant;
        }
        public void addNewQuestStepHenchmanGrant()
        {
            gamedataObject newQuestStepHenchmanGrant = new gamedataObject();
            newQuestStepHenchmanGrant.@class = "QuestStepHenchmanGrantData";
            newQuestStepHenchmanGrant.stepID = SelectedQuestStep.uuid;
            _selectedQuestStepHenchmanGrantsObservable.Add(newQuestStepHenchmanGrant);
            ObservableCollection<gamedataObject> temp = _selectedQuestStepHenchmanGrantsObservable;
            _selectedQuestStepHenchmanGrantsObservable = temp;
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(newQuestStepHenchmanGrant);
            root.Items = newRootItems.ToArray();
            allObjects[newQuestStepHenchmanGrant.uuid] = newQuestStepHenchmanGrant;
        }
        public void addNewQuestStepChoiceItemGrant()
        {
            gamedataObject newQuestStepChoiceItemGrant = new gamedataObject();
            newQuestStepChoiceItemGrant.@class = "QuestStepChoiceItemGrantData";
            newQuestStepChoiceItemGrant.questStepChoiceID = SelectedQuestStepChoice.uuid;
            newQuestStepChoiceItemGrant.value = "0";
            newQuestStepChoiceItemGrant.name = "";
            newQuestStepChoiceItemGrant.description = "";
            _selectedQuestStepChoiceItemGrantsObservable.Add(newQuestStepChoiceItemGrant);
            ObservableCollection<gamedataObject> temp = _selectedQuestStepChoiceItemGrantsObservable;
            _selectedQuestStepChoiceItemGrantsObservable = temp;
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(newQuestStepChoiceItemGrant);
            root.Items = newRootItems.ToArray();
            allObjects[newQuestStepChoiceItemGrant.uuid] = newQuestStepChoiceItemGrant;
        }
        public void addNewQuestStepChoiceStatGrant()
        {
            gamedataObject newQuestStepChoiceStatGrant = new gamedataObject();
            newQuestStepChoiceStatGrant.@class = "QuestStepChoiceStatGrantData";
            newQuestStepChoiceStatGrant.questStepChoiceID = SelectedQuestStepChoice.uuid;
            newQuestStepChoiceStatGrant.value = "0";
            _selectedQuestStepChoiceStatGrantsObservable.Add(newQuestStepChoiceStatGrant);
            ObservableCollection<gamedataObject> temp = _selectedQuestStepChoiceStatGrantsObservable;
            _selectedQuestStepChoiceStatGrantsObservable = temp;
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(newQuestStepChoiceStatGrant);
            root.Items = newRootItems.ToArray();
            allObjects[newQuestStepChoiceStatGrant.uuid] = newQuestStepChoiceStatGrant;
        }
        public void addNewQuestStepChoiceStatRequirement()
        {
            gamedataObject newQuestStepChoiceStatRequirement = new gamedataObject();
            newQuestStepChoiceStatRequirement.@class = "QuestStepChoiceStatRequirementData";
            newQuestStepChoiceStatRequirement.questStepChoiceID = SelectedQuestStepChoice.uuid;
            newQuestStepChoiceStatRequirement.value = "0";
            _selectedQuestStepChoiceStatRequirementsObservable.Add(newQuestStepChoiceStatRequirement);
            ObservableCollection<gamedataObject> temp = _selectedQuestStepChoiceStatRequirementsObservable;
            _selectedQuestStepChoiceStatRequirementsObservable = temp;
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(newQuestStepChoiceStatRequirement);
            root.Items = newRootItems.ToArray();
            allObjects[newQuestStepChoiceStatRequirement.uuid] = newQuestStepChoiceStatRequirement;
        }
        public void addNewQuestStepChoiceHenchmanGrant()
        {
            gamedataObject newQuestStepChoiceHenchmanGrant = new gamedataObject();
            newQuestStepChoiceHenchmanGrant.@class = "QuestStepChoiceHenchmanGrantData";
            newQuestStepChoiceHenchmanGrant.questStepChoiceID = SelectedQuestStepChoice.uuid;
            _selectedQuestStepChoiceHenchmanGrantsObservable.Add(newQuestStepChoiceHenchmanGrant);
            ObservableCollection<gamedataObject> temp = _selectedQuestStepChoiceHenchmanGrantsObservable;
            _selectedQuestStepChoiceHenchmanGrantsObservable = temp;
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(newQuestStepChoiceHenchmanGrant);
            root.Items = newRootItems.ToArray();
            allObjects[newQuestStepChoiceHenchmanGrant.uuid] = newQuestStepChoiceHenchmanGrant;
        }
        public void addNewQuest()
        {
            gamedataObject questStepItem = new gamedataObject();
            questStepItem.name = "First step";
            questStepItem.description = "First step description";
            questStepItem.@class = "QuestStepData";
            gamedataObject questItem = new gamedataObject();
            questStepItem.questID = questItem.uuid;
            questItem.@class = "QuestData";
            questItem.name = "New quest";
            questItem.description = "Description of new quest";
            questItem.firstStepID = questStepItem.uuid;
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(questItem);
            newRootItems.Add(questStepItem);
            root.Items = newRootItems.ToArray();
            allObjects[questItem.uuid] = questItem;
            allObjects[questStepItem.uuid] = questStepItem;
            SelectNewObject(questItem, "Quest");
        }
        public void addNewQuestStepToChoice(gamedataObject choice)
        {
            int suffix = 0;
            bool found = true;
            while (found)
            {
                suffix++;
                found = false;
                foreach (gamedataObject step in selectedQuestStepsObservable)
                {
                    if (step.name.Equals("New step " + suffix.ToString()))

                    {
                        found = true;
                        break;
                    };
                }
            }
            gamedataObject questStepItem = new gamedataObject();
            questStepItem.name = "New step " + suffix.ToString();
            questStepItem.description = "New step description";
            questStepItem.@class = "QuestStepData";
            questStepItem.questID = gameDataObj.uuid;
            _selectedQuestStepsObservable.Add(questStepItem);
            NotifyPropertyChanged("selectedQuestStepsObservable");
            //Re-bind the choices grid, because that forces a rebuild of the grid; the next-choice doesn't auto-detect changes the way it should.
            choice.nextStepID = questStepItem.uuid;
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(questStepItem);
            root.Items = newRootItems.ToArray();
            allObjects[questStepItem.uuid] = questStepItem;
        }
        public Boolean deleteObjectButtonVisible
        {
            get { return (gameDataObj != null); }
        }
        public Boolean deleteObjectButtonEnabled
        {
            get
            {
                return deleteObjectButtonTooltip.Equals("Delete this object");
            }
        }
        public Color deleteObjectButtonTextColor
        {
            get
            {
                if (deleteObjectButtonTooltip.Equals("Delete this object"))
                {
                    return Colors.Black;
                }
                else
                {
                    return Colors.Red;
                }

            }
        }
        public String deleteObjectButtonTooltip
        {
            get
            {
                if (gameDataObj == null)
                    return "";
                Boolean canDelete = true;
                List<string> blockingObjects = new List<string>();
                if (gameDataObj.@class.Equals("ItemTypeData"))
                {
                    foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => (iter.deleted == null || iter.deleted != "True") && iter.@class.Equals("ItemData") && iter.itemTypeID.Equals(gameDataObj.uuid)))
                    {
                        canDelete = false;
                        blockingObjects.Add(gameObject.name);
                    }
                }
                else if (gameDataObj.@class.Equals("StatGroupData"))
                {
                    foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => (iter.deleted == null || iter.deleted != "True") && iter.@class.Equals("StatData") && iter.statGroupID.Equals(gameDataObj.uuid)))
                    {
                        canDelete = false;
                        blockingObjects.Add(gameObject.name);
                    }
                }
                else if (gameDataObj.@class.Equals("LocationData"))
                {
                    //Nothing currently blocks locations, will likely add quests that can move you to a location soon.
                }
                else if (gameDataObj.@class.Equals("HenchmanData"))
                {
                    //Nothing currently blocks henchmen, will likely add quests that grant you a henchman or level the henchman soon
                }
                else if (gameDataObj.@class.Equals("QuestData"))
                {
                    //Nothing currently blocks quests; perhaps in the future there will be items that grant quests
                }
                else if (gameDataObj.@class.Equals("NPCData"))
                {
                    //Nothing currently blocks npcs
                }
                else if (gameDataObj.@class.Equals("StatData"))
                {
                    //Lots of things reference stats, so instead of a long type check, just look for stat != null.
                    foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => (iter.deleted==null || iter.deleted!="True") && iter.statID.Equals(gameDataObj.uuid)))
                    {
                        canDelete = false;
                        blockingObjects.Add(gameObject.name);
                    }
                }
                else if (gameDataObj.@class.Equals("ItemData"))
                {
                    //Lots of things reference items, so instead of a long type check, just look for item != null.
                    foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => (iter.deleted == null || iter.deleted != "True") && iter.itemID.Equals(gameDataObj.uuid)))
                    {
                        canDelete = false;
                        blockingObjects.Add(gameObject.name);
                    }
                }
                else
                {
                    //Item type not handled yet?
                    canDelete = false;
                }
                if (canDelete)
                {
                    return "Delete this object";
                }
                else
                {
                    if (blockingObjects.Count() == 0)
                    {
                        return "This type of object can not yet be deleted";
                    }
                    else
                    {
                        System.Text.StringBuilder retVal = new System.Text.StringBuilder();
                        retVal.Append("Cannot delete object because it is referred to by the following objects:");
                        foreach (string iter in blockingObjects)
                        {
                            //Todo: handle things like quest-stat-requirements, item-stat-modifiers, where we probably want to list the parent object as the blocker.
                            retVal.Append("\n");
                            retVal.Append(iter);
                        }
                        return retVal.ToString();
                    }
                }
            }
        }
        public ObservableCollection<GameTreeItem> GameTree { get => gameTree; set => gameTree = value; }
        public GameTreeItem SelectedGameTreeItem
        {
            get => selectedGameTreeItem;
            set {
                selectedGameTreeItem = value;
                if (value == null)
                {
                    gameData = null;
                }
                else
                {
                    gameData = value.ObjectRef;
                }
            }
        }

        private Boolean canDeleteSelectedObject()
        {
            var canDelete = false;
            if (deleteObjectButtonTooltip.Equals("Delete this object"))
            {
                canDelete = true;
            }
            return canDelete;
        }
        public void deleteSelectedObject()
        {
            if (gameDataObj != null)
            {
                if (gameDataObj.@class.Equals("HenchmanData"))
                {
                    foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("HenchmanStatData") && iter.henchmanID.Equals(gameDataObj.uuid)))
                    {
                        gameObject.deleted = "True";
                    }
                }
                else if (gameDataObj.@class.Equals("QuestData"))
                {
                    //This takes care of quest stat requirements, quest steps
                    foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.questID.Equals(gameDataObj.uuid)))
                    {
                        gameObject.deleted = "True";
                    }
                    foreach (gamedataObject questStepObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepData") && iter.questID.Equals(gameDataObj.uuid)))
                    {
                        if (questStepObject.questStepChoice != null)
                        {
                            foreach (gamedataObjectQuestStepChoice choice in questStepObject.questStepChoice)
                            {
                                var gameObjectChoice = GetItemByID(choice.Value);
                                if (gameObjectChoice != null)
                                {
                                    gameObjectChoice.deleted = "True";
                                }
                            }
                        }
                    }

                }
                if (canDeleteSelectedObject())
                {
                    deleteObject(gameDataObj);
                    gameDataObj = null;
                    OnGameDataChanged(EventArgs.Empty);
                }
            }
        }
        public void deleteObject(gamedataObject objToDelete)
        {

            if (objToDelete.@class.Equals("QuestStepData"))
            {
                //Ensure that you cannot delete a quest step which is the first step of a quest.  All quests must have a step.
                //TODO: turn off the button when this condition is true, so we don't even get here
                foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestData")
                                                                                               && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                               && iter.firstStepID.Equals(objToDelete.uuid)))
                {
                    return;
                }
            }
            //If selected in the tree view, unselect it
            if (SelectedGameTreeItem.ObjectRef.Equals(objToDelete))
            {
                SelectedGameTreeItem = null;
            }
            //Deletion process: flag object deleted
            objToDelete.deleted = "True";
            //Remove object from tree view (if relevant)
            foreach (GameTreeItem node in GameTree)
            {
                foreach (GameTreeItem childnode in node.Children)
                {
                    if (objToDelete.@class == "StatData")
                    {
                        //Stats are a second level down, because they are grouped by statgroup
                        foreach (GameTreeItem grandchildnode in childnode.Children)
                        {
                            if ((grandchildnode.ObjectRef).Equals(objToDelete))
                            {
                                childnode.Children.Remove(grandchildnode);
                                break;
                            }
                        }
                    }
                    else if (objToDelete.@class == "StatGroupData")
                    {
                        //Delete both the stat group element, and the stat group category under stats
                        if (childnode.ObjectRef != null && (childnode.ObjectRef).Equals(objToDelete))
                        {
                            node.Children.Remove(childnode);
                            break;
                        }
                        if (childnode.Id.Equals(objToDelete.uuid))
                        {
                            node.Children.Remove(childnode);
                            break;
                        }
                    }
                    else
                    {
                        if (childnode.ObjectRef !=null && (childnode.ObjectRef).Equals(objToDelete))
                        {
                            node.Children.Remove(childnode);
                            break;
                        }
                    }
                }
                //Remove object from any relevant combo boxes
                if (objToDelete.@class.Equals("ItemTypeData"))
                {
                    itemTypes.Remove(objToDelete);
                }
                if (objToDelete.@class.Equals("StatGroupData"))
                {
                    StatGroups.Remove(objToDelete);
                }
                //Remove object from relevant sub-displays
                if (objToDelete.@class.Equals("HenchmanStatData"))
                {
                    henchmanStats.Remove(objToDelete);
                }
                if (objToDelete.@class.Equals("ItemStatModifierData"))
                {
                    itemStatModifiersObservable.Remove(objToDelete);
                }
                if (objToDelete.@class.Equals("StartingCharacterInfoStatModifierData"))
                {
                    startingCharacterStatModifiersObservable.Remove(objToDelete);
                }
                if (objToDelete.@class.Equals("QuestStatRequirementData"))
                {
                    questStatRequirementsObservable.Remove(objToDelete);
                }
                if (objToDelete.@class.Equals("QuestItemRequirementData"))
                {
                    questItemRequirementsObservable.Remove(objToDelete);
                }
                if (objToDelete.@class.Equals("QuestStepChoiceData"))
                {
                    selectedQuestStepChoicesObservable.Remove(objToDelete);
                }
                if (objToDelete.@class.Equals("QuestStepStatGrantData"))
                {
                    selectedQuestStepStatGrantsObservable.Remove(objToDelete);
                }
                if (objToDelete.@class.Equals("QuestStepItemGrantData"))
                {
                    selectedQuestStepItemGrantsObservable.Remove(objToDelete);
                }
                if (objToDelete.@class.Equals("QuestStepHenchmanGrantData"))
                {
                    selectedQuestStepHenchmanGrantsObservable.Remove(objToDelete);
                }
                if (objToDelete.@class.Equals("QuestStepChoiceStatGrantData"))
                {
                    selectedQuestStepChoiceStatGrantsObservable.Remove(objToDelete);
                }
                if (objToDelete.@class.Equals("QuestStepChoiceStatRequirementData"))
                {
                    selectedQuestStepChoiceStatRequirementsObservable.Remove(objToDelete);
                }
                if (objToDelete.@class.Equals("QuestStepChoiceItemGrantData"))
                {
                    selectedQuestStepChoiceItemGrantsObservable.Remove(objToDelete);
                }
                if (objToDelete.@class.Equals("QuestStepChoiceHenchmanGrantData"))
                {
                    selectedQuestStepChoiceHenchmanGrantsObservable.Remove(objToDelete);
                }

                if (objToDelete.@class.Equals("QuestStepData"))
                {
                    if (selectedQuestStepsObservable.Contains(objToDelete))
                    {
                        selectedQuestStepsObservable.Remove(objToDelete);
                    }
                    SelectedQuestStep = null; //Unselect this quest step, moving you back to the flowchart; must come after removing it from the selected quest steps, or it will still be on the flowchart.
                                              //If you delete a quest step, auto-delete all the dependent stuff.  
                    foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceData")
                                                                                                   && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                                   && iter.nextStepID.Equals(objToDelete.uuid)))
                    {
                        gameObject.nextStepID = "";
                    }
                    foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceData")
                                                                                                  && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                                  && iter.failStepID.Equals(objToDelete.uuid)))
                    {
                        gameObject.failStepID = "";
                    }
                    foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceData")
                                                                                                   && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                                   && iter.stepID.Equals(objToDelete.uuid)))
                    {
                        gameObject.deleted = "True";

                        foreach (gamedataObject innerGameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceStatRequirementData")
                                                                                                       && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                                       && iter.stepID.Equals(objToDelete.uuid)))
                        {
                            innerGameObject.deleted = "True";
                        }
                        foreach (gamedataObject innerGameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceItemGrantData")
                                                                                                       && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                                       && iter.stepID.Equals(objToDelete.uuid)))
                        {
                            innerGameObject.deleted = "True";
                        }

                        foreach (gamedataObject innerGameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceStatGrantData")
                                                                                                       && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                                       && iter.stepID.Equals(objToDelete.uuid)))
                        {
                            innerGameObject.deleted = "True";
                        }
                        foreach (gamedataObject innerGameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceHenchmanGrantData")
                                                                                                       && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                                       && iter.stepID.Equals(objToDelete.uuid)))
                        {
                            innerGameObject.deleted = "True";
                        }
                    }
                    foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepItemGrantData")
                                                                                                   && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                                   && iter.stepID.Equals(objToDelete.uuid)))
                    {
                        gameObject.deleted = "True";
                    }

                    foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepStatGrantData")
                                                                                                   && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                                   && iter.stepID.Equals(objToDelete.uuid)))
                    {
                        gameObject.deleted = "True";
                    }
                    foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepHenchmanGrantData")
                                                                                                   && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                                   && iter.stepID.Equals(objToDelete.uuid)))
                    {
                        gameObject.deleted = "True";
                    }
                }
            }
        }

    public void deleteQuestStepChoice(gamedataObject choice)
		{
		}
		public gameDataView()
		{
            PostLoad = false;
            GameTree = new ObservableCollection<GameTreeItem>();
			_henchmanStats = new ObservableCollection<gamedataObject>();
			_itemStatModifiersObservable = new ObservableCollection<gamedataObject>();
            _startingCharacterStatModifiersObservable = new ObservableCollection<gamedataObject>();
			_questStatRequirementsObservable = new ObservableCollection<gamedataObject>();
            _questItemRequirementsObservable = new ObservableCollection<gamedataObject>();
            _selectedQuestStepChoicesObservable = new ObservableCollection<gamedataObject>();
            _selectedQuestStepItemGrantsObservable = new ObservableCollection<gamedataObject>();
            _selectedQuestStepStatGrantsObservable = new ObservableCollection<gamedataObject>();
            _selectedQuestStepHenchmanGrantsObservable = new ObservableCollection<gamedataObject>();
            _selectedQuestStepChoiceItemGrantsObservable = new ObservableCollection<gamedataObject>();
            _selectedQuestStepChoiceStatGrantsObservable = new ObservableCollection<gamedataObject>();
            _selectedQuestStepChoiceStatRequirementsObservable = new ObservableCollection<gamedataObject>();
            _selectedQuestStepChoiceHenchmanGrantsObservable = new ObservableCollection<gamedataObject>();
            _selectedQuestStepsObservable = new ObservableCollection<gamedataObject>();
			allObjects = new Dictionary<string, gamedataObject>();
			ItemTypes = new ObservableCollection<gamedataObject>();
            itemTypes.OrderBy(g => g.name);
            stats = new ObservableCollection<gamedataObject>();
            stats.OrderBy(g => g.name);
            locations = new ObservableCollection<gamedataObject>();
            locations.OrderBy(g => g.name);
            henchmen = new ObservableCollection<gamedataObject>();
            henchmen.OrderBy(g => g.name);
            items = new ObservableCollection<gamedataObject>();
            items.OrderBy(g => g.name);
            StatGroups = new ObservableCollection<gamedataObject>();
            statGroups.OrderBy(g => g.name);
            quests = new ObservableCollection<gamedataObject>();
            quests.OrderBy(g => g.name);
            npcs = new ObservableCollection<gamedataObject>();
            npcs.OrderBy(g => g.name);
            elementsByType = new Dictionary<string, List<gamedataObject>>();
        }
        //Implementing INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }


}
