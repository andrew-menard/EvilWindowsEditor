//------------------------------------------------------------------------------
// Originally built using auto-generation tool, but then went through serious changes and simplifications.
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
/// <remarks/>
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
public partial class gamedata {

    [XmlIgnore]
    private List<gamedataObject> itemsField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("object", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public List<gamedataObject> Items {
        get {
            return this.itemsField;
        }
        set {
            this.itemsField = value;
        }
    }

    public gamedataObject getItemByID(string uuid)
    {
        foreach (gamedataObject obj in Items.Where<gamedataObject>(iter => iter.uuid.Equals(uuid) && iter.deleted == "False"))
        {
            return obj;
        }
        return null;
    }
    public gamedataObject getDeletedItemByID(string uuid)
    {
        foreach (gamedataObject obj in Items.Where<gamedataObject>(iter => iter.uuid.Equals(uuid) && iter.deleted == "True"))
        {
            return obj;
        }
        return null;
    }
}

/// <remarks/>
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class gamedataObject {
    
    //*********************IMPORTANT: If you add a field here, make sure to add it to the .Equals method in gamedatahelpers!
    //*********************Similarly, if you add a new many-to-one relationship, make sure it is also added in gamedatahelpers!

    private string nameField;
    
    private string descriptionField;

    private string commentField;

    private string questPreDescriptionField;

    private string iconField;
    
    private int giveItemNumberField=0;
    
    private int minimumField=0;
    
    private int maximumField=0;
    
    private string equippableField;
    
    private string uuidField;
    
    private int valueField=0;
    private int statTargetField=0;
    private string stringValueField = "";
    private string alternateBackgroundField="";

    private string deletedField; //Effectively a boolean

    private string displayedOnSidebarField; //Effectively a boolean
    private string displayedOnMainStatPageField; //Effectively a boolean
    private string oneTimeQuestField; //Effectively a bool
    private string questIsForceGrantedField; //Effectively a bool
    private string cooldownTimerField; //Minutes until quest available again for timed quests
    private string questEnergyCostField;
    private int sortOrderField=0; //For things like quest choices when the designer wants to specify a custom sort.
    private List<gamedataObjectItem> itemField;

    private List<gamedataObjectStartingCharacterInfo> startingCharacterInfoField;

    private List<gamedataObjectStep> stepField;
    private List<gamedataObjectStep> firstStepField;
    private List<gamedataObjectStep> failStepField;
    private List<gamedataObjectStep> nextStepField;
    
    private List<gamedataObjectQuest> questField;
    
    private List<gamedataObjectQuestStepChoice> questStepChoiceField;
    
    private List<gamedataObjectStat> statField;
    private List<gamedataObjectStat> xpStatField;
    private List<gamedataObjectHenchman> henchmanField;
    
    
    private List<gamedataObjectItemType> itemTypeField;

    private List<gamedataObjectLocation> subLocationOfField;
    private List<gamedataObjectLocation> requiredLocationField;
    private List<gamedataObjectLocation> startingLocationField;

    private List<gamedataObjectLocation> associatedLocationField;
    private List<gamedataObjectLocation> unlockLocationField;
    private List<gamedataObjectLocation> moveToLocationField;
    private List<gamedataObjectNPC> associatedNPCField;
    private List<gamedataObjectQuest> startingQuestField;

    private List<gamedataObjectStatGroup> statGroupField;

    private string classField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string name {
        get {
            return this.nameField;
        }
        set {
            this.nameField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string description {
        get {
			if (this.descriptionField == null)
				return "";
			else
	            return this.descriptionField;
        }
        set {
            this.descriptionField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string comment
    {
        get
        {
            if (this.commentField == null)
                return "";
            else
                return this.commentField;
        }
        set
        {
            this.commentField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string questPreDescription
    {
        get
        {
            return this.questPreDescriptionField;
        }
        set
        {
            this.questPreDescriptionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string icon {
        get {
            return this.iconField;
        }
        set {
            this.iconField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string stringValue
    {
        get
        {
            return this.stringValueField;
        }
        set
        {
            this.stringValueField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string alternateBackground
    {
        get
        {
            return this.alternateBackgroundField;
        }
        set
        {
            this.alternateBackgroundField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public int giveItemNumber {
        get {
            return this.giveItemNumberField;
        }
        set {
            this.giveItemNumberField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public int minimum
    {
        get
        {
            //            if (this.minimumField == null || this.minimumField=="")
            //           { return "0"; }
            //          else
            //         {
            return this.minimumField;
            //       }
        }
        set
        {
            this.minimumField = value;

        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
public int maximum
{
    get
    {
        //if (this.maximumField == null || this.maximumField=="")
        //{
        //    return "0";
        //}
        //else
        //{
        return this.maximumField;
        //}
    }
    set
    {
        this.maximumField = value;

    }
}

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string questEnergyCost
    {
        get
        {
            if (questEnergyCostField == null || questEnergyCostField.Equals(""))
            {
                return "0";
            }
            else
            {
                return questEnergyCostField;
            }
        }
        set
        {
            int temp;
            if (Int32.TryParse(value, out temp))
            {
                this.questEnergyCostField = temp.ToString();
            }
            else
            {
                this.questEnergyCostField = "0";
            }
            NotifyPropertyChanged("questEnergyCost");
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public int sortOrder
    {
        get
        {
            return sortOrderField;
        }
        set
        {
            sortOrderField = value;
            NotifyPropertyChanged("sortOrder");
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string equippable {
        get
        {
            if (equippableField == null || equippableField.Equals("false"))
            {
                return "False";
            }
            else if (equippableField.Equals("true"))
            {
                return "True";
            }
            return equippableField;
        }
        set {
            this.equippableField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string uuid {
        get {
            return this.uuidField;
        }
        set {
            this.uuidField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public int value {
        get {
            return this.valueField;
        }
        set {
            this.valueField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string deleted
    {
        get
        {
            if (deletedField == null || deletedField.Equals("false"))
            {
                return "False";
            }
            else if (deletedField.Equals("true"))
            {
                return "True";
            }
            return this.deletedField;
        }
        set
        {
            this.deletedField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string displayedOnSidebar
    {
        get
        {
            if (displayedOnSidebarField == null || displayedOnSidebarField.Equals("false"))
            {
                return "False";
            }
            else if (displayedOnSidebarField.Equals("true"))
            {
                return "True";
            }
            return this.displayedOnSidebarField;
        }
        set
        {
            this.displayedOnSidebarField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string displayedOnMainStatPage
    {
        get
        {
            if (displayedOnMainStatPageField == null || displayedOnMainStatPageField.Equals("false"))
            {
                return "False";
            }
            else if (displayedOnMainStatPageField.Equals("true"))
            {
                return "True";
            }
            return this.displayedOnMainStatPageField;
        }
        set
        {
            this.displayedOnMainStatPageField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string oneTimeQuest
    {
        get
        {
            if (oneTimeQuestField == null || oneTimeQuestField.Equals("false"))
            {
                return "False";
            }
            else if (oneTimeQuestField.Equals("true"))
            {
                return "True";
            }
            return this.oneTimeQuestField;
        }
        set
        {
            this.oneTimeQuestField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string questIsForceGranted
    {
        get
        {
            if (questIsForceGrantedField == null || questIsForceGrantedField.Equals("false"))
            {
                return "False";
            }
            else if (questIsForceGrantedField.Equals("true"))
            {
                return "True";
            }
            return this.questIsForceGrantedField;
        }
        set
        {
            this.questIsForceGrantedField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string cooldownTimer
    {
        get
        {
            if (cooldownTimerField == null || cooldownTimerField.Equals(""))
            {
                return "0";
            }
            return this.cooldownTimerField;
        }
        set
        {
            this.cooldownTimerField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("item", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true)]
    public List<gamedataObjectItem> item {
        get {
            return this.itemField;
        }
        set {
            this.itemField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("startingCharacterInfo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public List<gamedataObjectStartingCharacterInfo> startingCharacterInfo
    {
        get
        {
            return this.startingCharacterInfoField;
        }
        set
        {
            this.startingCharacterInfoField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("step", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true)]
    public List<gamedataObjectStep> step {
        get {
            return this.stepField;
        }
        set {
            this.stepField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("failStep", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public List<gamedataObjectStep> failStep
    {
        get
        {
            return this.failStepField;
        }
        set
        {
            this.failStepField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("nextStep", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true)]
    public List<gamedataObjectStep> nextStep {
        get {
            return this.nextStepField;
        }
        set {
            this.nextStepField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("quest", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true)]
    public List<gamedataObjectQuest> quest {
        get {
            return this.questField;
        }
        set {
            this.questField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("questStepChoice", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true)]
    public List<gamedataObjectQuestStepChoice> questStepChoice {
        get {
            return this.questStepChoiceField;
        }
        set {
            this.questStepChoiceField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("stat", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true)]
    public List<gamedataObjectStat> stat {
        get {
            return this.statField;
        }
        set {
            this.statField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("xpStat", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public List<gamedataObjectStat> xpStat
    {
        get
        {
            return this.xpStatField;
        }
        set
        {
            this.xpStatField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("henchman", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true)]
    public List<gamedataObjectHenchman> henchman {
        get {
            return this.henchmanField;
        }
        set {
            this.henchmanField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("firstStep", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true)]
    public List<gamedataObjectStep> firstStep {
        get {
            return this.firstStepField;
        }
        set {
            this.firstStepField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("itemType", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true)]
    public List<gamedataObjectItemType> itemType {
        get {
            return this.itemTypeField;
        }
        set {
            this.itemTypeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("subLocationOf", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public List<gamedataObjectLocation> subLocationOf
    {
        get
        {
            return this.subLocationOfField;
        }
        set
        {
            this.subLocationOfField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("requiredLocation", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public List<gamedataObjectLocation> requiredLocation
    {
        get
        {
            return this.requiredLocationField;
        }
        set
        {
            this.requiredLocationField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("startingLocation", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public List<gamedataObjectLocation> startingLocation
        {
        get
        {
            return this.startingLocationField;
        }
        set
        {
            this.startingLocationField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("associatedLocation", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public List<gamedataObjectLocation> associatedLocation
    {
        get
        {
            return this.associatedLocationField;
        }
        set
        {
            this.associatedLocationField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("unlockLocation", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public List<gamedataObjectLocation> unlockLocation
    {
        get
        {
            return this.unlockLocationField;
        }
        set
        {
            this.unlockLocationField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("moveToLocation", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public List<gamedataObjectLocation> moveToLocation
    {
        get
        {
            return this.moveToLocationField;
        }
        set
        {
            this.moveToLocationField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("associatedNPC", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public List<gamedataObjectNPC> associatedNPC
    {
        get
        {
            return this.associatedNPCField;
        }
        set
        {
            this.associatedNPCField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("startingQuest", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public List<gamedataObjectQuest> startingQuest
    {
        get
        {
            return this.startingQuestField;
        }
        set
        {
            this.startingQuestField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("statGroup", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public List<gamedataObjectStatGroup> statGroup
    {
        get
        {
            return this.statGroupField;
        }
        set
        {
            this.statGroupField = value;
        }
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string @class {
        get {
            return this.classField;
        }
        set {
            this.classField = value;
        }
    }
}

[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public class objectReference: System.Object
{
    public override bool Equals( System.Object rhs)
    {
        if (rhs == null)
            return false;
        objectReference rhsOR = rhs as objectReference;
        if (rhsOR == null)
            return false;
        return (className.Equals(rhsOR.className) && Value.Equals(rhsOR.Value));
    }
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
    private readonly string className;
    protected objectReference(string cn)
    {
        className = cn;
    }

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string @class
    {
        get
        {
            return "UUID";
        }
        set
        {
            //Do nothing; we have to have a setter if it is serializable, and we actually do want it serialized out for the game server
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string classType
    {
        get
        {
            return className;
        }
        set
        {
            //Do nothing; we have to have a setter if it is serializable, and we actually do want it serialized out for the game server
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
    {
        get
        {
            if (valueField == null)
                return "";
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }
}
public partial class gamedataObjectStartingCharacterInfo: objectReference
{
    public gamedataObjectStartingCharacterInfo(): base("StartingCharacterInfoData")
    {
    }}

public partial class gamedataObjectItem : objectReference
{

    public gamedataObjectItem() : base("ItemData")
    {
    }
}

public partial class gamedataObjectStep: objectReference {

    public gamedataObjectStep() : base("QuestStepData")
    {
    }
}

public partial class gamedataObjectQuest : objectReference
{

    public gamedataObjectQuest() : base("QuestData")
    {
    }
}
   
public partial class gamedataObjectQuestStepChoice : objectReference
{

    public gamedataObjectQuestStepChoice() : base("QuestStepChoiceData")
    {
    }
} 

public partial class gamedataObjectStat : objectReference
{

    public gamedataObjectStat() : base("StatData")
    {
    }
}
public partial class gamedataObjectHenchman : objectReference
{

    public gamedataObjectHenchman() : base("HenchmanData")
    {
    }
} 
public partial class gamedataObjectItemType : objectReference
{

    public gamedataObjectItemType() : base("ItemTypeData")
    {
    }
}
public partial class gamedataObjectLocation : objectReference
{

    public gamedataObjectLocation() : base("LocationData")
    {
    }
}
public partial class gamedataObjectNPC : objectReference
{

    public gamedataObjectNPC() : base("NPCData")
    {
    }
}
public partial class gamedataObjectStatGroup : objectReference
{

    public gamedataObjectStatGroup() : base("StatGroupData")
    {
    }
}

