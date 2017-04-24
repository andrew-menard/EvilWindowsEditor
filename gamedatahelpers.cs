using System;
using System.ComponentModel;
using System.Xml.Serialization;

public partial class gamedataObject: INotifyPropertyChanged
{
    public gamedataObject()
    {
        uuid = Guid.NewGuid().ToString();
    }
    
    [XmlIgnore]
    public string Name
	{
		get { return name; }
		set { name=value; NotifyPropertyChanged("Name"); }
    }
    [XmlIgnore]
    public string itemID
	{
        get { if (item == null || item.Length==0 || item[0].Value == null) return ""; return item[0].Value;}
		set { 
            if (item == null)
			{
				item = new gamedataObjectItem[1];
				item[0] = new gamedataObjectItem();
				item[0].@class = "UUID";
			}
			item[0].Value = value; }
    }
    [XmlIgnore]
    public string startingCharacterInfoID
    {
        get { if (startingCharacterInfo == null || startingCharacterInfo.Length == 0 || startingCharacterInfo[0].Value==null) return ""; return startingCharacterInfo[0].Value; }
        set
        {
            if (startingCharacterInfo == null)
            {
                startingCharacterInfo = new gamedataObjectStartingCharacterInfo[1];
                startingCharacterInfo[0] = new gamedataObjectStartingCharacterInfo();
                startingCharacterInfo[0].@class = "UUID";
            }
            startingCharacterInfo[0].Value = value;
        }
    }
    [XmlIgnore]
    public string henchmanID
    {
        get { if (henchman == null || henchman.Length == 0 || henchman[0].Value==null) return ""; return henchman[0].Value; }
        set
        {
            if (henchman == null)
            {
                henchman = new gamedataObjectHenchman[1];
                henchman[0] = new gamedataObjectHenchman();
                henchman[0].@class = "UUID";
            }
            henchman[0].Value = value;
        }
    }
    [XmlIgnore]
    public string statID
    {
        get { if (stat == null || stat.Length == 0 || stat[0].Value==null) return ""; return stat[0].Value; }
        set
        {
            if (stat == null)
            {
                stat = new gamedataObjectStat[1];
                stat[0] = new gamedataObjectStat();
                stat[0].@class = "UUID";
            }
            stat[0].Value = value;
        }
    }
    [XmlIgnore]
    public string requiredLocationID
    {
        get { if (requiredLocation == null || requiredLocation.Length == 0 || requiredLocation[0].Value==null) return ""; return requiredLocation[0].Value; }
        set
        {
            if (requiredLocation == null)
            {
                requiredLocation = new gamedataObjectRequiredLocation[1];
                requiredLocation[0] = new gamedataObjectRequiredLocation();
                requiredLocation[0].@class = "UUID";
            }
            requiredLocation[0].Value = value;
        }
    }
    [XmlIgnore]
    public string questStepChoiceID
    {
        get { if (questStepChoice == null || questStepChoice.Length == 0 || questStepChoice[0].Value==null) return ""; return questStepChoice[0].Value; }
        set
        {
            if (questStepChoice == null)
            {
                questStepChoice = new gamedataObjectQuestStepChoice[1];
                questStepChoice[0] = new gamedataObjectQuestStepChoice();
                questStepChoice[0].@class = "UUID";
            }
            questStepChoice[0].Value = value;
        }
    }
    [XmlIgnore]
    public string questID
    {
        get { if (quest == null || quest.Length == 0 || quest[0].Value == null) return ""; return quest[0].Value; }
        set
        {
            if (quest == null)
            {
                quest = new gamedataObjectQuest[1];
                quest[0] = new gamedataObjectQuest();
                quest[0].@class = "UUID";
            }
            quest[0].Value = value;
        }
    }
    [XmlIgnore]
    public string stepID
    {
        get { if (step == null || step.Length == 0 || step[0].Value==null) return ""; return step[0].Value; }
        set
        {
            if (step == null)
            {
                step = new gamedataObjectStep[1];
                step[0] = new gamedataObjectStep();
                step[0].@class = "UUID";
            }
            step[0].Value = value;
        }
    }
    [XmlIgnore]
    public string firstStepID
    {
        get { if (firstStep == null || firstStep.Length == 0 || firstStep[0].Value==null) return ""; return firstStep[0].Value; }
        set
        {
            if (firstStep == null)
            {
                firstStep = new gamedataObjectFirstStep[1];
                firstStep[0] = new gamedataObjectFirstStep();
                firstStep[0].@class = "UUID";
            }
            firstStep[0].Value = value;
        }
    }
    [XmlIgnore]
    public string nextStepID
	{
		get { if (nextStep == null || nextStep.Length == 0 || nextStep[0].Value==null) return ""; return nextStep[0].Value; }
		set
		{
            if (value == null)  //When you repopulate the step pulldowns in the grid view because you've changed quests, all the choices suddenly point to something not valid, so they are force-changed to nulls!  This isn't good...
                return;
			if (nextStep == null)
			{
				nextStep = new gamedataObjectNextStep[1];
				nextStep[0] = new gamedataObjectNextStep();
				nextStep[0].@class = "UUID";
			}
			nextStep[0].Value = value;
			NotifyPropertyChanged("nextStepID");
		}
    }
    [XmlIgnore]
    public string itemTypeID
    {
        get { if (itemType == null || itemType.Length == 0 || itemType[0].Value==null) return ""; return itemType[0].Value; }
        set
        {
            if (itemType == null)
            {
                itemType = new gamedataObjectItemType[1];
                itemType[0] = new gamedataObjectItemType();
                itemType[0].@class = "UUID";
            }
            itemType[0].Value = value;
            NotifyPropertyChanged("itemTypeID");
        }
    }
    [XmlIgnore]
    public string startingQuestID
    {
        get { if (startingQuest == null || startingQuest.Length == 0 || startingQuest[0].Value==null) return ""; return startingQuest[0].Value; }
        set
        {
            if (startingQuest == null)
            {
                startingQuest = new gamedataObjectStartingQuest[1];
                startingQuest[0] = new gamedataObjectStartingQuest();
                startingQuest[0].@class = "UUID";
            }
            startingQuest[0].Value = value;
            NotifyPropertyChanged("startingQuestID");
        }
    }
    [XmlIgnore]
    public string statGroupID
    {
        get { if (statGroup == null || statGroup.Length == 0 || statGroup[0].Value== null) return ""; return statGroup[0].Value; }
        set
        {
            if (statGroup == null)
            {
                statGroup = new gamedataObjectStatGroup[1];
                statGroup[0] = new gamedataObjectStatGroup();
                statGroup[0].@class = "UUID";
            }
            statGroup[0].Value = value;
            NotifyPropertyChanged("statGroupID");
        }
    }
    [XmlIgnore]
    public string startingLocationID
    {
        get { if (startingLocation == null || startingLocation.Length == 0 || startingLocation[0].Value==null) return ""; return startingLocation[0].Value; }
        set
        {
            if (startingLocation == null)
            {
                startingLocation = new gamedataObjectStartingLocation[1];
                startingLocation[0] = new gamedataObjectStartingLocation();
                startingLocation[0].@class = "UUID";
            }
            startingLocation[0].Value = value;
            NotifyPropertyChanged("startingLocationID");
        }
    }
    [XmlIgnore]
    public string associatedLocationID
    {
        get { if (associatedLocation == null || associatedLocation.Length == 0 || associatedLocation[0].Value==null) return ""; return associatedLocation[0].Value; }
        set
        {
            if (associatedLocation == null)
            {
                associatedLocation = new gamedataObjectAssociatedLocation[1];
                associatedLocation[0] = new gamedataObjectAssociatedLocation();
                associatedLocation[0].@class = "UUID";
            }
            associatedLocation[0].Value = value;
            NotifyPropertyChanged("associatedLocationID");
        }
    }
    [XmlIgnore]
    public string unlockLocationID
    {
        get { if (unlockLocation == null || unlockLocation.Length == 0 || unlockLocation[0].Value==null) return ""; return unlockLocation[0].Value; }
        set
        {
            if (unlockLocation == null)
            {
                unlockLocation = new gamedataObjectUnlockLocation[1];
                unlockLocation[0] = new gamedataObjectUnlockLocation();
                unlockLocation[0].@class = "UUID";
            }
            unlockLocation[0].Value = value;
            NotifyPropertyChanged("unlockLocationID");
        }
    }
    [XmlIgnore]
    public string moveToLocationID
    {
        get { if (moveToLocation == null || moveToLocation.Length == 0 || moveToLocation[0].Value==null) return ""; return moveToLocation[0].Value; }
        set
        {
            if (moveToLocation == null)
            {
                moveToLocation = new gamedataObjectMoveToLocation[1];
                moveToLocation[0] = new gamedataObjectMoveToLocation();
                moveToLocation[0].@class = "UUID";
            }
            moveToLocation[0].Value = value;
            NotifyPropertyChanged("moveToLocationID");
        }
    }
    [XmlIgnore]
    public string associatedNPCID
    {
        get { if (associatedNPC == null || associatedNPC.Length == 0 || associatedNPC[0].Value==null) return ""; return associatedNPC[0].Value; }
        set
        {
            if (associatedNPC == null)
            {
                associatedNPC = new gamedataObjectAssociatedNPC[1];
                associatedNPC[0] = new gamedataObjectAssociatedNPC();
                associatedNPC[0].@class = "UUID";
            }
            associatedNPC[0].Value = value;
            NotifyPropertyChanged("associatedNPCID");
        }
    }
    [XmlIgnore]
    public string integerValue
    {
        get
        {
            if (value == null || value.Equals(""))
            {
                return "0";
            }
            else
            {
                return value;
            }
        }
        set
        {
            int temp;
            if (Int32.TryParse(value, out temp))
            {
                this.value = temp.ToString();
            }
            else
            {
                this.value = "0";
            }
            NotifyPropertyChanged("integerValue");
        }
    }
    
    //Overriding ToString makes objects show their name in the debugger, which is generally easier; not used in actual code.
    override public string ToString()
    {
        return name;
    }
	//Implementing INotifyPropertyChanged interfacee
	public event PropertyChangedEventHandler PropertyChanged;

	public void NotifyPropertyChanged(String info)
	{
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
    }
}