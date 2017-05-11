using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

public partial class gamedataObject: INotifyPropertyChanged
{
    [XmlIgnore]
    public gamedata associatedRootData = null;//When comparing objects for a merge, we need to compare sub-objects, which requires querying the set of things owned by the root data.
    public gamedataObject()
    {
        uuid = Guid.NewGuid().ToString();
    }
    public Boolean Equals(gamedataObject rhs)
    {
        //First, check all the required fields.
        if (!nameField.Equals(rhs.nameField))
        {
            return false;
        }

        if (descriptionField != rhs.descriptionField)
        {
            return false;
        }

        if (commentField != rhs.commentField)
        {
            return false;
        }

        if (questPreDescriptionField != rhs.questPreDescriptionField)
        {
            return false;
        }

        if (iconField != rhs.iconField)
        {
            return false;
        }

        if (giveItemNumberField != rhs.giveItemNumberField)
        {
            return false;
        }

        if (minimumField != rhs.minimumField)
        {
            return false;
        }

        if (maximumField != rhs.maximumField)
        {
            return false;
        }

        if (equippableField != rhs.equippableField)
        {
            return false;
        }

        if (uuidField != rhs.uuidField)
        {
            return false;
        }

        if (valueField != rhs.valueField)
        {
            return false;
        }

        if (statTargetField != rhs.statTargetField)
        {
            return false;
        }

        if (alternateBackgroundField != rhs.alternateBackgroundField)
        {
            return false;
        }

        if (deletedField != rhs.deletedField)
        {
            return false;
        }

        if (displayedOnSidebarField != rhs.displayedOnSidebarField)
        {
            return false;
        }

        if (displayedOnMainStatPageField != rhs.displayedOnMainStatPageField)
        {
            return false;
        }

        if (oneTimeQuestField != rhs.oneTimeQuestField)
        {
            return false;
        }

        if (questIsForceGrantedField != rhs.questIsForceGrantedField)
        {
            return false;
        }

        if (cooldownTimerField != rhs.cooldownTimerField)
        {
            return false;
        }

        if (questEnergyCostField != rhs.questEnergyCostField)
        {
            return false;
        }

        if (sortOrderField != rhs.sortOrderField)
        {
            return false;
        }

           if (!itemField.SequenceEqual(rhs.itemField))
        {
            return false;
        }

    if (!startingCharacterInfoField.SequenceEqual(rhs.startingCharacterInfoField))
        {
            return false;
        }

    if (!stepField.SequenceEqual(rhs.stepField))
        {
            return false;
        }
   if (!firstStepField.SequenceEqual(rhs.firstStepField))
        {
            return false;
        }
   if (!failStepField.SequenceEqual(rhs.failStepField))

        {
            return false;
        }
   if(!nextStepField.SequenceEqual(rhs.nextStepField))
        {
            return false;
        }

    if (!questField.SequenceEqual(rhs.questField))
        {
            return false;
        }
    if (!questStepChoiceField.SequenceEqual(rhs.questStepChoiceField))
        {
            return false;
        }
    if(!statField.SequenceEqual(rhs.statField))
        {
            return false;
        }
    if (!xpStatField.SequenceEqual(rhs.xpStatField))
        {
            return false;
        }
    if(!henchmanField.SequenceEqual(rhs.henchmanField))
        {
            return false;
        }

    if (!itemTypeField.SequenceEqual(rhs.itemTypeField))
        {
            return false;
        }
    if (!subLocationOfField.SequenceEqual(rhs.subLocationOfField))
        {
            return false;
        }
        if (!requiredLocationField.SequenceEqual(rhs.requiredLocationField))
        {
            return false;
        }
        if (!startingLocationField.SequenceEqual(rhs.startingLocationField))
        {
            return false;
        }
        if (!associatedLocationField.SequenceEqual(rhs.associatedLocationField))
        {
            return false;
        }
        if (!unlockLocationField.SequenceEqual(rhs.unlockLocationField))
        {
            return false;
        }
        if (!moveToLocationField.SequenceEqual(rhs.moveToLocationField))
        {
            return false;
        }
        if ( !associatedNPCField.SequenceEqual(rhs.associatedNPCField))
        {
            return false;
        }
        if (!startingQuestField.SequenceEqual(rhs.startingQuestField))
        {
            return false;
        }
        if (!statGroupField.SequenceEqual(rhs.statGroupField))
        {
            return false;
        }
        //Next, for objects with many-to-one relationships with something else, check the something else 
        //Nothing relevant for ItemType, Stat, StatGroup, Location, or NPC
        if (@class == "ItemData")
        {
            //For an item to be equal, they need to have identical stat mods
            if (!doIdenticalRelatedObjectsExist(associatedRootData, rhs.associatedRootData, "ItemStatModifierData", (gamedataObject) => itemID.Equals(uuid)))
            {
                return false;
            }
        }
        if (@class == "StartingCharacterInfoData")
        {
            //For starting character to be equal, they need to have identical stats
            if (!doIdenticalRelatedObjectsExist(associatedRootData, rhs.associatedRootData, "StartingCharacterInfoStatModifierData", (gamedataObject) => startingCharacterInfoID.Equals(uuid)))
            {
                return false;
            }
        }
        if (@class == "HenchmanData")
        {
            //For a henchman to be equal, they need to have identical stats
            if (!doIdenticalRelatedObjectsExist(associatedRootData, rhs.associatedRootData, "HenchmanStatData", (gamedataObject) => henchmanID.Equals(uuid)))
            {
                return false;
            }
        }
        if (@class == "QuestData")
        {
            //For a quest to be equal, they need to have identical steps and requirements.  Note that this is recursive, so the steps need to have identical step requirements etc (see below)
            if (!doIdenticalRelatedObjectsExist(associatedRootData, rhs.associatedRootData, "QuestStepData", (gamedataObject)=>questID.Equals(uuid)))
           {
                return false;
            }
            if (!doIdenticalRelatedObjectsExist(associatedRootData, rhs.associatedRootData, "QuestStatRequirementData", (gamedataObject) => questID.Equals(uuid)))
            {
                return false;
            }
            if (!doIdenticalRelatedObjectsExist(associatedRootData, rhs.associatedRootData, "QuestItemRequirementData", (gamedataObject) => questID.Equals(uuid)))
            {
                return false;
            }
        }
        if (@class == "QuestStepData")
        {
            //For a quest to be equal, they need to have identical steps and requirements.  Note that this is recursive, so the steps need to have identical step requirements etc (see below)
            if (!doIdenticalRelatedObjectsExist(associatedRootData, rhs.associatedRootData, "QuestStepChoiceData", (gamedataObject) => stepID.Equals(uuid)))
            {
                return false;
            }
            if (!doIdenticalRelatedObjectsExist(associatedRootData, rhs.associatedRootData, "QuestStepStatGrantData", (gamedataObject) => stepID.Equals(uuid)))
            {
                return false;
            }
            if (!doIdenticalRelatedObjectsExist(associatedRootData, rhs.associatedRootData, "QuestStepItemGrantData", (gamedataObject) => stepID.Equals(uuid)))
            {
                return false;
            }
            if (!doIdenticalRelatedObjectsExist(associatedRootData, rhs.associatedRootData, "QuestStepHenchmanGrantData", (gamedataObject) => stepID.Equals(uuid)))
            {
                return false;
            }
        }
        if (@class == "QuestStepChoiceData")
        {
            //For a quest step choice to be equal, they need to have identical requirement and grants. 
            if (!doIdenticalRelatedObjectsExist(associatedRootData, rhs.associatedRootData, "QuestStepChoiceStatRequirementData", (gamedataObject) => questStepChoiceID.Equals(uuid)))
            {
                return false;
            }
            if (!doIdenticalRelatedObjectsExist(associatedRootData, rhs.associatedRootData, "QuestStepChoiceStatGrantData", (gamedataObject) => questStepChoiceID.Equals(uuid)))
            {
                return false;
            }
            if (!doIdenticalRelatedObjectsExist(associatedRootData, rhs.associatedRootData, "QuestStepChoiceItemGrantData", (gamedataObject) => questStepChoiceID.Equals(uuid)))
            {
                return false;
            }
            if (!doIdenticalRelatedObjectsExist(associatedRootData, rhs.associatedRootData, "QuestStepChoiceHenchmanGrantData", (gamedataObject) => questStepChoiceID.Equals(uuid)))
            {
                return false;
            }
        }
        return true;
    }
    delegate bool filterDelegate(gamedataObject i);
    private bool doIdenticalRelatedObjectsExist(gamedata lhsData, gamedata rhsData, string datatype, filterDelegate filter)
    {
        foreach (gamedataObject lhsObject in lhsData.Items.Where<gamedataObject>(iter => iter.@class.Equals(datatype) && iter.deleted == "False" && filter(iter)))
        {
            if (!doesIdenticalObjectExist(lhsObject, rhsData))
            {
                return false;
            }
        }
        foreach (gamedataObject rhsObject in rhsData.Items.Where<gamedataObject>(iter => iter.@class.Equals(datatype) && iter.deleted == "False" && filter(iter)))
        {
            if (!doesIdenticalObjectExist(rhsObject, lhsData))
            {
                return false;
            }
        }
        return true;
    }
    public List<gamedataObject> GetDependentObjects()
    {
        List<gamedataObject> retVal = new List<gamedataObject>();
        if (@class == "ItemData")
        {
            //For an item to be equal, they need to have identical stat mods
            foreach (gamedataObject obj in associatedRootData.Items.Where<gamedataObject>(iter=>iter.@class.Equals("ItemStatModifierData")&& iter.deleted=="False" && iter.itemID.Equals(uuid)))
               
            {
                retVal.Add(obj);
            }
        }
        if (@class == "StartingCharacterInfoData")
        {
            //For starting character to be equal, they need to have identical stats
            foreach (gamedataObject obj in associatedRootData.Items.Where<gamedataObject>(iter => iter.@class.Equals("StartingCharacterInfoStatModifierData") && iter.deleted == "False" && iter.startingCharacterInfoID.Equals(uuid)))
            {

                retVal.Add(obj);
            }
        }
        if (@class == "HenchmanData")
        {
            //For a henchman to be equal, they need to have identical stats
            foreach (gamedataObject obj in associatedRootData.Items.Where<gamedataObject>(iter => iter.@class.Equals("HenchmanStatData") && iter.deleted == "False" && iter.henchmanID.Equals(uuid)))
            {
                retVal.Add(obj);
            }
        }
        if (@class == "QuestData")
        {
            //For a quest to be equal, they need to have identical steps and requirements.  Note that this is recursive, so the steps need to have identical step requirements etc (see below)
            foreach (gamedataObject obj in associatedRootData.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepData") && iter.deleted == "False" && iter.questID.Equals(uuid)))
            {
                retVal.Add(obj);
            }
            foreach (gamedataObject obj in associatedRootData.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStatRequirementData") && iter.deleted == "False" && iter.questID.Equals(uuid)))
            {
                retVal.Add(obj);
            }
            foreach (gamedataObject obj in associatedRootData.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestItemRequirementData") && iter.deleted == "False" && iter.questID.Equals(uuid)))
            {
                retVal.Add(obj);
            }
        }
        if (@class == "QuestStepData")
        {
            //For a quest to be equal, they need to have identical steps and requirements.  Note that this is recursive, so the steps need to have identical step requirements etc (see below)
            foreach (gamedataObject obj in associatedRootData.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceData") && iter.deleted == "False" && iter.stepID.Equals(uuid)))
            {
                retVal.Add(obj);
            }
            foreach (gamedataObject obj in associatedRootData.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepStatGrantData") && iter.deleted == "False" && iter.stepID.Equals(uuid)))
            {
                retVal.Add(obj);
            }
            foreach (gamedataObject obj in associatedRootData.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepItemGrantData") && iter.deleted == "False" && iter.stepID.Equals(uuid)))
            {
                retVal.Add(obj);
            }
            foreach (gamedataObject obj in associatedRootData.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepHenchmanGrantData") && iter.deleted == "False" && iter.stepID.Equals(uuid)))
            {
                retVal.Add(obj);
            }
        }
        if (@class == "QuestStepChoiceData")
        {
            //For a quest step choice to be equal, they need to have identical requirement and grants. 
            foreach (gamedataObject obj in associatedRootData.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceStatRequirementData") && iter.deleted == "False" && iter.questStepChoiceID.Equals(uuid)))
            {
                retVal.Add(obj);
            }
            foreach (gamedataObject obj in associatedRootData.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceStatGrantData") && iter.deleted == "False" && iter.questStepChoiceID.Equals(uuid)))
            {
                retVal.Add(obj);
            }
            foreach (gamedataObject obj in associatedRootData.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceItemGrantData") && iter.deleted == "False" && iter.questStepChoiceID.Equals(uuid)))
            {
                retVal.Add(obj);
            }
            foreach (gamedataObject obj in associatedRootData.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceHenchmanGrantData") && iter.deleted == "False" && iter.questStepChoiceID.Equals(uuid)))
            {
                retVal.Add(obj);
            }
        }
        return retVal;
    }
    private bool doesIdenticalObjectExist(gamedataObject obj,gamedata rhsData)
    {
        gamedataObject otherObject = rhsData.getItemByID(obj.uuid);
        if (otherObject == null)
        {
            return false;
        }
        if (!obj.Equals(otherObject))
        {
            return false;
        }
        return true;
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
        get { if (item == null || item.Count==0 || item[0].Value == null) return ""; return item[0].Value;}
		set { 
            if (item == null)
			{
                item = new List<gamedataObjectItem>();
				item.Add(new gamedataObjectItem());
			}
			item[0].Value = value; }
    }
    [XmlIgnore]
    public string startingCharacterInfoID
    {
        get { if (startingCharacterInfo == null || startingCharacterInfo.Count == 0 || startingCharacterInfo[0].Value==null) return ""; return startingCharacterInfo[0].Value; }
        set
        {
            if (startingCharacterInfo == null)
            {
                startingCharacterInfo = new List<gamedataObjectStartingCharacterInfo>();
                startingCharacterInfo.Add(new gamedataObjectStartingCharacterInfo());
            }
            startingCharacterInfo[0].Value = value;
        }
    }
    [XmlIgnore]
    public string henchmanID
    {
        get { if (henchman == null || henchman.Count == 0 || henchman[0].Value==null) return ""; return henchman[0].Value; }
        set
        {
            if (henchman == null)
            {
                henchman = new List<gamedataObjectHenchman>();
                henchman.Add(new gamedataObjectHenchman());
            }
            henchman[0].Value = value;
        }
    }
    [XmlIgnore]
    public string statID
    {
        get { if (stat == null || stat.Count == 0 || stat[0].Value==null) return ""; return stat[0].Value; }
        set
        {
            if (stat == null)
            {
                stat = new List<gamedataObjectStat>();
                stat.Add(new gamedataObjectStat());
            }
            stat[0].Value = value;
        }
    }
    [XmlIgnore]
    public string xpStatID
    {
        get { if (xpStat == null || xpStat.Count == 0 || xpStat[0].Value == null) return ""; return xpStat[0].Value; }
        set
        {
            if (xpStat == null)
            {
                xpStat = new List<gamedataObjectStat>();
                xpStat.Add(new gamedataObjectStat());
            }
            xpStat[0].Value = value;
        }
    }
    [XmlIgnore]
    public string subLocationOfID
    {
        get { if (subLocationOf == null || subLocationOf.Count == 0 || subLocationOf[0].Value == null) return ""; return subLocationOf[0].Value; }
        set
        {
            if (subLocationOf == null)
            {
                subLocationOf = new List<gamedataObjectLocation>();
                subLocationOf[0] = new gamedataObjectLocation();
            }
            subLocationOf[0].Value = value;
        }
    }
    [XmlIgnore]
    public string requiredLocationID
    {
        get { if (requiredLocation == null || requiredLocation.Count == 0 || requiredLocation[0].Value==null) return ""; return requiredLocation[0].Value; }
        set
        {
            if (requiredLocation == null)
            {
                requiredLocation = new List<gamedataObjectLocation>();
                requiredLocation.Add(new gamedataObjectLocation());
            }
            requiredLocation[0].Value = value;
        }
    }
    [XmlIgnore]
    public string questStepChoiceID
    {
        get { if (questStepChoice == null || questStepChoice.Count == 0 || questStepChoice[0].Value==null) return ""; return questStepChoice[0].Value; }
        set
        {
            if (questStepChoice == null)
            {
                questStepChoice = new List<gamedataObjectQuestStepChoice>();
                questStepChoice.Add(new gamedataObjectQuestStepChoice());
            }
            questStepChoice[0].Value = value;
        }
    }
    [XmlIgnore]
    public string questID
    {
        get { if (quest == null || quest.Count == 0 || quest[0].Value == null) return ""; return quest[0].Value; }
        set
        {
            if (quest == null)
            {
                quest = new List<gamedataObjectQuest>();
                quest.Add(new gamedataObjectQuest());
            }
            quest[0].Value = value;
        }
    }
    [XmlIgnore]
    public string stepID
    {
        get { if (step == null || step.Count == 0 || step[0].Value==null) return ""; return step[0].Value; }
        set
        {
            if (step == null)
            {
                step = new List<gamedataObjectStep>();
                step.Add(new gamedataObjectStep());
            }
            step[0].Value = value;
        }
    }
    [XmlIgnore]
    public string failStepID
    {
        get { if (failStep == null || failStep.Count == 0 || failStep[0].Value == null) return ""; return failStep[0].Value; }
        set
        {
            if (value == null)  //When you repopulate the step pulldowns in the grid view because you've changed quests, all the choices suddenly point to something not valid, so they are force-changed to nulls!  This isn't good...
                return;
            if (failStep == null)
            {
                failStep = new List<gamedataObjectStep>();
                failStep.Add(new gamedataObjectStep());
            }
            failStep[0].Value = value;
        }
    }
    [XmlIgnore]
    public string firstStepID
    {
        get { if (firstStep == null || firstStep.Count == 0 || firstStep[0].Value==null) return ""; return firstStep[0].Value; }
        set
        {
            if (firstStep == null)
            {
                firstStep = new List<gamedataObjectStep>();
                firstStep.Add(new gamedataObjectStep());
            }
            firstStep[0].Value = value;
        }
    }
    [XmlIgnore]
    public string nextStepID
	{
		get { if (nextStep == null || nextStep.Count == 0 || nextStep[0].Value==null) return ""; return nextStep[0].Value; }
		set
		{
            if (value == null)  //When you repopulate the step pulldowns in the grid view because you've changed quests, all the choices suddenly point to something not valid, so they are force-changed to nulls!  This isn't good...
                return;
			if (nextStep == null)
			{
				nextStep = new List<gamedataObjectStep>();
				nextStep.Add(new gamedataObjectStep());
			}
			nextStep[0].Value = value;
			NotifyPropertyChanged("nextStepID");
		}
    }
    [XmlIgnore]
    public string itemTypeID
    {
        get { if (itemType == null || itemType.Count == 0 || itemType[0].Value==null) return ""; return itemType[0].Value; }
        set
        {
            if (itemType == null)
            {
                itemType = new List<gamedataObjectItemType>();
                itemType.Add(new gamedataObjectItemType());
            }
            itemType[0].Value = value;
            NotifyPropertyChanged("itemTypeID");
        }
    }
    [XmlIgnore]
    public string startingQuestID
    {
        get { if (startingQuest == null || startingQuest.Count == 0 || startingQuest[0].Value==null) return ""; return startingQuest[0].Value; }
        set
        {
            if (startingQuest == null)
            {
                startingQuest = new List<gamedataObjectQuest>();
                startingQuest.Add(new gamedataObjectQuest());
            }
            startingQuest[0].Value = value;
            NotifyPropertyChanged("startingQuestID");
        }
    }
    [XmlIgnore]
    public string statGroupID
    {
        get { if (statGroup == null || statGroup.Count == 0 || statGroup[0].Value== null) return ""; return statGroup[0].Value; }
        set
        {
            if (statGroup == null)
            {
                statGroup = new List<gamedataObjectStatGroup>();
                statGroup.Add(new gamedataObjectStatGroup());
            }
            statGroup[0].Value = value;
            NotifyPropertyChanged("statGroupID");
        }
    }
    [XmlIgnore]
    public string startingLocationID
    {
        get { if (startingLocation == null || startingLocation.Count == 0 || startingLocation[0].Value==null) return ""; return startingLocation[0].Value; }
        set
        {
            if (startingLocation == null)
            {
                startingLocation = new List<gamedataObjectLocation>();
                startingLocation.Add(new gamedataObjectLocation());
            }
            startingLocation[0].Value = value;
            NotifyPropertyChanged("startingLocationID");
        }
    }
    [XmlIgnore]
    public string associatedLocationID
    {
        get { if (associatedLocation == null || associatedLocation.Count == 0 || associatedLocation[0].Value==null) return ""; return associatedLocation[0].Value; }
        set
        {
            if (associatedLocation == null)
            {
                associatedLocation = new List<gamedataObjectLocation>();
                associatedLocation.Add(new gamedataObjectLocation());
            }
            associatedLocation[0].Value = value;
            NotifyPropertyChanged("associatedLocationID");
        }
    }
    [XmlIgnore]
    public string unlockLocationID
    {
        get { if (unlockLocation == null || unlockLocation.Count == 0 || unlockLocation[0].Value==null) return ""; return unlockLocation[0].Value; }
        set
        {
            if (unlockLocation == null)
            {
                unlockLocation = new List<gamedataObjectLocation>();
                unlockLocation.Add(new gamedataObjectLocation());
            }
            unlockLocation[0].Value = value;
            NotifyPropertyChanged("unlockLocationID");
        }
    }
    [XmlIgnore]
    public string moveToLocationID
    {
        get { if (moveToLocation == null || moveToLocation.Count == 0 || moveToLocation[0].Value==null) return ""; return moveToLocation[0].Value; }
        set
        {
            if (moveToLocation == null)
            {
                moveToLocation = new List<gamedataObjectLocation>();
                moveToLocation.Add(new gamedataObjectLocation());
            }
            moveToLocation[0].Value = value;
            NotifyPropertyChanged("moveToLocationID");
        }
    }
    [XmlIgnore]
    public string associatedNPCID
    {
        get { if (associatedNPC == null || associatedNPC.Count == 0 || associatedNPC[0].Value==null) return ""; return associatedNPC[0].Value; }
        set
        {
            if (associatedNPC == null)
            {
                associatedNPC = new List<gamedataObjectNPC>();
                associatedNPC.Add(new gamedataObjectNPC());
            }
            associatedNPC[0].Value = value;
            NotifyPropertyChanged("associatedNPCID");
        }
    }
    [XmlIgnore]
    public int integerValue
    {
        get
        {
                return value;
        }
        set
        {
            this.value = value;
        }
    }
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public int statTarget
    {
        get
        {
                return statTargetField;
        }
        set
        {
                   this.statTargetField = value;
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