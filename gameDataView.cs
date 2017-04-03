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
        public GameTreeItem()
        {
            Children = new ObservableCollection<GameTreeItem>();
        }
        public string Id { get { if (ObjectRef != null) { return ObjectRef.uuid; } else { return ""; } } }
        public gamedataObject ObjectRef { get; set; }
        private string _name;
        public string Name { get { if (ObjectRef != null) { return ObjectRef.name; } else { return _name; } } set { _name = value; } }
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
        public ObservableCollection<gamedataObject> henchmen; //Henchmen list, for binding combo boxes for item grants and requirements
        public ObservableCollection<gamedataObject> items; //Items list, for binding combo boxes for item grants and requirements
        private ObservableCollection<gamedataObject> itemTypes; //Item types list, for binding combo box on the item screen
        public ObservableCollection<gamedataObject> ItemTypes { get => itemTypes; set { itemTypes = value; NotifyPropertyChanged("ItemTypes"); } }
        public ObservableCollection<gamedataObject> stats; //Stats list, for binding combo boxes for stat requirements
        private ObservableCollection<gamedataObject> statGroups; //Stat Groups list, for binding combo boxes for stat group
        public ObservableCollection<gamedataObject> StatGroups { get => statGroups; set { statGroups = value; NotifyPropertyChanged("StatGroups"); } }
        public ObservableCollection<gamedataObject> questSteps; //List of steps on the current quest, for binding combo boxes for quest choices.
        public ObservableCollection<gamedataObject> quests; //List of quests, for binding combo boxes for quests.
        public ObservableCollection<gamedataObject> npcs; //List of npcs, for binding combo boxes for npcs.
        Dictionary<String, List<gamedataObject>> elementsByType; //For elements that show up in the tree view only.
        Dictionary<String, List<gamedataObject>> allElementsByType; //General set of everything

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
                locations.Add(null); //Empty value so you can "unselect" things in the combo boxes
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
                foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("StatData") && iter.deleted == "False"))
                {
                    stats.Add(gameObject);
                }
                statGroups.Clear();
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
                allElementsByType.Clear();
                foreach (gamedataObject gameObject in root.Items)
                {
                    if (!allElementsByType.ContainsKey(gameObject.@class))
                    {
                        allElementsByType.Add(gameObject.@class, new List<gamedataObject>());
                    }
                    allElementsByType[gameObject.@class].Add(gameObject);
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
                        foreach (gamedataObject element in elementsByType[elementClass])
                        {
                            if (element.deleted != null && element.deleted != "True")
                            {
                                GameTreeItem childNode = new GameTreeItem() { ObjectRef = element };
                                classNode.Children.Add(childNode);
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
                if (_selectedQuestStep != null && gameDataObj != null && gameDataObj.@class == "QuestData")
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
                    if (value == null)
                    {
                        //If you were looking at a step and now you aren't, you now need to rebuild the flowchart 
                        //because you may have changed the step name, added a step, or added a choice.
                        //We don't need to update any of the Observables below, because the UI elements driven from them will all be hidden anyway.
                        //theForm.updateFlowchartPanel();
                    }
                    else
                    {
                        //While we're at it, tell the controls bound to the selected step's name and description to update..
                        _selectedQuestStep.NotifyPropertyChanged("name");

                        _selectedQuestStep.NotifyPropertyChanged("description");
                        //Populate the list of quest stepchoices.  Comes after the above which might be fixing a quest step so it gets found here.
                        selectedQuestStepChoicesObservable.Clear();
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceData")
                                                                                               && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                               && iter.stepID.Equals(_selectedQuestStep.uuid)))
                        {
                            selectedQuestStepChoicesObservable.Add(gameObject);
                        }
                        selectedQuestStepStatGrantsObservable.Clear();
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepStatGrantData")
                                                                                               && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                               && iter.stepID.Equals(_selectedQuestStep.uuid)))
                        {
                            selectedQuestStepStatGrantsObservable.Add(gameObject);
                        }
                        selectedQuestStepItemGrantsObservable.Clear();
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepItemGrantData")
                                                                                               && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                               && iter.stepID.Equals(_selectedQuestStep.uuid)))
                        {
                            selectedQuestStepItemGrantsObservable.Add(gameObject);
                        }
                        selectedQuestStepHenchmanGrantsObservable.Clear();
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
                    //theForm.updateDescriptionAndWebView(this.stepDescription, DescriptionSelector.QuestStepDescription);
                }
            }
        }
        public bool questStepChoiceDataVisible
        {
            get
            {
                if (_selectedQuestStepChoice != null && _selectedQuestStep != null && gameDataObj != null && gameDataObj.@class == "QuestData")
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
                    gameDataObj = value;
                    if (gameDataObj != null && gameDataObj.@class == "ItemData")
                    {
                        //If this is an item, populate the list of relevant item stat modifiers.
                        itemStatModifiersObservable.Clear();
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("ItemStatModifierData") && iter.itemID.Equals(gameDataObj.uuid)))
                        {
                            itemStatModifiersObservable.Add(gameObject);
                        }
                    }
                    if (gameDataObj != null && gameDataObj.@class == "StartingCharacterInfoData")
                    {
                        itemStatModifiersObservable.Clear();
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("StartingCharacterInfoStatModifierData") && iter.startingCharacterInfoID.Equals(gameDataObj.uuid)))
                        {
                            itemStatModifiersObservable.Add(gameObject);
                        }
                    }
                    if (gameDataObj != null && gameDataObj.@class == "HenchmanData")
                    {
                        //If this is an henchman, populate the list of relevant henchman stats.
                        henchmanStats.Clear();
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("HenchmanStatData") && iter.henchmanID.Equals(gameDataObj.uuid)))
                        {
                            henchmanStats.Add(gameObject);
                        }
                    }
                    if (gameDataObj != null && gameDataObj.@class == "QuestData")
                    {
                        //If this is an quest, populate the list of relevant quest stat requirements.
                        questStatRequirements.Clear();
                        questStatRequirementsObservable.Clear();
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStatRequirementData")
                                                                                               && (iter.deleted == null || iter.deleted.Equals("False"))
                                                                                               && iter.questID.Equals(gameDataObj.uuid)))
                        {
                            questStatRequirements.Add(gameObject);
                            questStatRequirementsObservable.Add(gameObject);
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
                        foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepData") && iter.questID.Equals(gameDataObj.uuid)))
                        {
                            selectedQuestStepsObservable.Add(gameObject);
                        }
                        // theForm.updateFlowchartPanel();
                    }
                    OnGameDataChanged(EventArgs.Empty);
                }
            }
        }

        protected void OnGameDataChanged(EventArgs e)
        {
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
            NotifyPropertyChanged("requiredLocation");
            NotifyPropertyChanged("startingLocation");
            NotifyPropertyChanged("associatedLocation");
            NotifyPropertyChanged("moveToLocation");
            NotifyPropertyChanged("unlockLocation");
            NotifyPropertyChanged("associatedNPC");
            NotifyPropertyChanged("startingQuest");
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
            //theForm.updateDescriptionAndWebView(this.description, DescriptionSelector.Description);
            // theForm.updateDescriptionAndWebView(this.questPreDescription, DescriptionSelector.QuestPreDescription);
            // theForm.updateDescriptionAndWebView(this.stepDescription, DescriptionSelector.QuestStepDescription);
            NotifyPropertyChanged(null); //This supposedly forces a notify on all properties, but appears not to work, possibly due to old version of .net?
            /*if (gameDataObj != null)
            {
                theForm.updateDescriptionAndWebView(this.description, MainForm.DescriptionSelector.Description);
                theForm.updateDescriptionAndWebView(this.questPreDescription, MainForm.DescriptionSelector.QuestPreDescription);
                theForm.updateDescriptionAndWebView(this.stepDescription, MainForm.DescriptionSelector.QuestStepDescription);
            }*/
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
                    updateObjectTreeForNameChange();
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

//        public string description { get { if (gameDataObj == null || gameDataObj.description == null) { return ""; } else return gameDataObj.description; } set { if (gameDataObj != null) gameDataObj.description = value; } }
        public string Description { get { if (gameDataObj == null || gameDataObj.description == null) { return ""; } else return WebUtility.HtmlDecode(gameDataObj.description); } set { if (gameDataObj != null) gameDataObj.description = WebUtility.HtmlEncode(value); } }

        public string questPreDescription { get { if (gameDataObj == null) { return ""; } else return WebUtility.HtmlDecode(gameDataObj.questPreDescription); } set { if (gameDataObj != null) gameDataObj.questPreDescription = WebUtility.HtmlEncode(value); } }
        public string stepDescription
        {
            get { if (gameDataObj == null || SelectedQuestStep == null) { return ""; } else return WebUtility.HtmlDecode(SelectedQuestStep.description); }
            set { if (SelectedQuestStep != null) SelectedQuestStep.description = WebUtility.HtmlEncode(value); }
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
                    return gameDataObj.equippable == "True";
                }
            }
            set
            {
                if (gameDataObj != null)
                {
                    if (value == true)
                    { gameDataObj.equippable = "True"; }
                    else
                    { gameDataObj.equippable = "False"; }
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
                    return gameDataObj.oneTimeQuest == "True";
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
                    return gameDataObj.displayedOnSidebar == "True";
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
                    return gameDataObj.displayedOnMainStatPage == "True";
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
                if (gameDataObj.@class == "ItemTypeData")
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
                if (gameDataObj.@class == "QuestData")
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
                if (gameDataObj.@class == "StartingCharacterInfoData")
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
                if (gameDataObj.@class == "HenchmanData")
                { return true; }
                else
                { return false; }
            }
        }
        public string requiredLocation
        {
            get { if (gameDataObj == null) { return ""; } else return gameDataObj.requiredLocationID; }
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
            get { if (gameDataObj == null || gameDataObj.startingLocation == null || gameDataObj.startingLocation.First() == null) { return ""; } else return gameDataObj.startingLocation.First().Value; }
            set
            {
                if (gameDataObj.startingLocation == null)
                {
                    gameDataObj.startingLocation = new gamedataObjectStartingLocation[1];
                    gameDataObj.startingLocation[0] = new gamedataObjectStartingLocation();
                }
                gameDataObj.startingLocation.First().Value = value;
            }
        }
        public string associatedLocation
        {
            get { if (_selectedQuestStep == null) { return ""; } else return _selectedQuestStep.associatedLocationID; }
            set
            {
                _selectedQuestStep.associatedLocationID = value;
            }
        }
        public string moveToLocation
        {
            get { if (_selectedQuestStep == null) { return ""; } else return _selectedQuestStep.moveToLocationID; }
            set
            {
                _selectedQuestStep.moveToLocationID = value;
            }
        }
        public string unlockLocation
        {
            get { if (_selectedQuestStep == null) { return ""; } else return _selectedQuestStep.unlockLocationID; }
            set
            {
                _selectedQuestStep.unlockLocationID = value;
            }
        }
        public string associatedNPC
        {
            get { if (_selectedQuestStep == null) { return ""; } else return _selectedQuestStep.associatedNPCID; }
            set
            {
                _selectedQuestStep.associatedNPCID = value;
            }
        }
        public string startingQuest
        {
            get { if (gameDataObj == null || gameDataObj.startingQuest == null || gameDataObj.startingQuest.First() == null) { return ""; } else return gameDataObj.startingQuest.First().Value; }
            set
            {
                if (gameDataObj.startingQuest == null)
                {
                    gameDataObj.startingQuest = new gamedataObjectStartingQuest[1];
                    gameDataObj.startingQuest[0] = new gamedataObjectStartingQuest();
                }
                gameDataObj.startingQuest.First().Value = value;
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
        public string statGroup
        {
            get { if (gameDataObj == null) { return ""; } else return gameDataObj.statGroupID; }
            set
            {
                gameDataObj.statGroupID = value;
            }
        }
        public bool itemTypeVisible
        {
            get
            {
                if (gameDataObj == null) { return false; }
                if (gameDataObj.@class == "ItemData")
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
                if (gameDataObj.@class == "StatData")
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
                if (gameDataObj.@class == "StatGroupData")
                { return true; }
                else
                { return false; }
            }
        }
        private ObservableCollection<gamedataObject> _henchmanStats;
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
        private ObservableCollection<gamedataObject> _questStatRequirements;
        public ObservableCollection<gamedataObject> questStatRequirements
        {
            get { return _questStatRequirements; }
            set
            {
                _questStatRequirements = value;
                NotifyPropertyChanged("questStatRequirements");
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
        public bool itemStatModifiersVisible
        {
            get
            {
                if (gameDataObj == null) { return false; }
                if (gameDataObj.@class.Equals("ItemData") || gameDataObj.@class.Equals("StartingCharacterInfoData"))
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
                if (gameDataObj.@class == "QuestData")
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
                if (gameDataObj.@class == "QuestData")
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
                if (gameDataObj.@class == "QuestData")
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
        public void updateObjectTreeForNameChange()
        {
            //Null check here because if we had an item selected and click on a category, that both sets the object to 
            //null and causes the name to change to ""
            if (gameDataObj != null)
            {/*
                foreach (TreeItem node in treeItemCollection)
                {
                    foreach (TreeItem childnode in node.Children)
                    {
                        if ((childnode.Key).Equals(gameDataObj.uuid))
                        {
                            childnode.Text = gameDataObj.name;
                        }
                    }
                }
                treeViewPanel.RefreshData();
                treeViewPanel.Invalidate();
                if (gameDataObj.@class.Equals("ItemTypeData"))
                {
                    theForm.rebuildItemTypeBox();
                }
                if (gameDataObj.@class.Equals("StatGroupData"))
                {
                    theForm.rebuildStatGroupBox();
                }
                if (gameDataObj.@class.Equals("LocationData"))
                {
                    theForm.rebuildLocationBoxes();
                }
                if (gameDataObj.@class.Equals("QuestData"))
                {
                    theForm.rebuildQuestBox();
                }
                if (gameDataObj.@class.Equals("NPCData"))
                {
                    theForm.rebuildNPCBox();
                }*/
            }
        }
        public void SelectNewObject(gamedataObject newObj, string objectType)
        {
            //This sets the newly created object as the selected object, rebuilds the secondary items that depend on it,
            //and fires all the relevant change notifications
            bool foundTreeItem = false;
            GameTreeItem childNode = new GameTreeItem() { ObjectRef = newObj };
            foreach (GameTreeItem iter in GameTree)
            {
                if (iter.Name.Equals(objectType))
                {
                    foundTreeItem = true;
                    iter.Children.Add(childNode);
                    break;
                }
            }
            if (!foundTreeItem)
            {
                GameTreeItem classNode = new GameTreeItem() { Name = objectType, Children = { childNode } };
                GameTree.Add(classNode);
            }
            //gameData = newObj;
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
        public void addNewStatModifier()
        {
            gamedataObject newStatMod = new gamedataObject();
            if (gameDataObj.@class.Equals("ItemData"))
            {
                newStatMod.@class = "ItemStatModifierData";
                newStatMod.itemID = gameData.uuid;
            }
            else
            {
                newStatMod.@class = "StartingCharacterInfoStatModifierData";
                newStatMod.startingCharacterInfoID = gameData.uuid;
            }
            newStatMod.itemID = gameData.uuid;
            newStatMod.statID = stats.First().uuid;
            newStatMod.value = "0";
            _itemStatModifiersObservable.Add(newStatMod);
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
            _questStatRequirements.Add(newStatReq);
            _questStatRequirementsObservable.Add(newStatReq);
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
            _selectedQuestStepChoicesObservable.Add(newQuestStepChoice);
            ObservableCollection<gamedataObject> temp = _selectedQuestStepChoicesObservable;
            _selectedQuestStepChoicesObservable = temp;
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(newQuestStepChoice);
            root.Items = newRootItems.ToArray();
            allObjects[newQuestStepChoice.uuid] = newQuestStepChoice;
            //theForm.rootScrollView.Height = -1;
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
            //theForm.rootScrollView.Height = -1;
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
            //theForm.rootScrollView.Height = -1;
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
           // theForm.rootScrollView.Height = -1;
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
           // theForm.rootScrollView.Height = -1;
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
           // theForm.rootScrollView.Height = -1;
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
            //theForm.rootScrollView.Height = -1;
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
           // theForm.rebuildQuestStepChoicesGrid();
            //Re-bind the choices grid, because that forces a rebuild of the grid; the next-choice doesn't auto-detect changes the way it should.
            choice.nextStepID = questStepItem.uuid;
            var newRootItems = root.Items.ToList<gamedataObject>();
            newRootItems.Add(questStepItem);
            root.Items = newRootItems.ToArray();
            allObjects[questStepItem.uuid] = questStepItem;
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
                if (gameDataObj.@class == "ItemTypeData")
                {
                    foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => (iter.deleted == null || iter.deleted != "True") && iter.@class.Equals("ItemData") && iter.itemTypeID.Equals(gameDataObj.uuid)))
                    {
                        canDelete = false;
                        blockingObjects.Add(gameObject.name);
                    }
                }
                else if (gameDataObj.@class == "StatGroupData")
                {
                    foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => (iter.deleted == null || iter.deleted != "True") && iter.@class.Equals("StatData") && iter.statGroupID.Equals(gameDataObj.uuid)))
                    {
                        canDelete = false;
                        blockingObjects.Add(gameObject.name);
                    }
                }
                else if (gameDataObj.@class == "LocationData")
                {
                    //Nothing currently blocks locations, will likely add quests that can move you to a location soon.
                }
                else if (gameDataObj.@class == "HenchmanData")
                {
                    //Nothing currently blocks henchmen, will likely add quests that grant you a henchman or level the henchman soon
                }
                else if (gameDataObj.@class == "QuestData")
                {
                    //Nothing currently blocks quests; perhaps in the future there will be items that grant quests
                }
                else if (gameDataObj.@class == "NPCData")
                {
                    //Nothing currently blocks npcs
                }
                else if (gameDataObj.@class == "StatData")
                {
                    //Lots of things reference stats, so instead of a long type check, just look for stat != null.
                    foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => (iter.deleted==null || iter.deleted!="True") && iter.statID.Equals(gameDataObj.uuid)))
                    {
                        canDelete = false;
                        blockingObjects.Add(gameObject.name);
                    }
                }
                else if (gameDataObj.@class == "ItemData")
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
                if (gameDataObj.@class == "HenchmanData")
                {
                    foreach (gamedataObject gameObject in root.Items.Where<gamedataObject>(iter => iter.@class.Equals("HenchmanStatData") && iter.henchmanID.Equals(gameDataObj.uuid)))
                    {
                        gameObject.deleted = "True";
                    }
                }
                else if (gameDataObj.@class == "QuestData")
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
                    if ((childnode.ObjectRef).Equals(objToDelete))
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
            //Remove object from relevant sub-displays
            if (objToDelete.@class.Equals("QuestStatRequirementData"))
            {
                questStatRequirementsObservable.Remove(objToDelete);
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
            if (objToDelete.@class.Equals("QuestStepChoiceItemGrantData"))
            {
                selectedQuestStepChoiceItemGrantsObservable.Remove(objToDelete);
            }
            if (objToDelete.@class.Equals("QuestStepChoiceHenchmanGrantData"))
            {
                selectedQuestStepChoiceHenchmanGrantsObservable.Remove(objToDelete);
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
			_questStatRequirements = new ObservableCollection<gamedataObject>();
			_questStatRequirementsObservable = new ObservableCollection<gamedataObject>();
			_selectedQuestStepChoicesObservable = new ObservableCollection<gamedataObject>();
            _selectedQuestStepItemGrantsObservable = new ObservableCollection<gamedataObject>();
            _selectedQuestStepStatGrantsObservable = new ObservableCollection<gamedataObject>();
            _selectedQuestStepHenchmanGrantsObservable = new ObservableCollection<gamedataObject>();
            _selectedQuestStepChoiceItemGrantsObservable = new ObservableCollection<gamedataObject>();
            _selectedQuestStepChoiceStatGrantsObservable = new ObservableCollection<gamedataObject>();
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
			allElementsByType = new Dictionary<string, List<gamedataObject>>();
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
