//------------------------------------------------------------------------------
// Originally built using auto-generation tool, but then went through serious changes and simplifications.
//------------------------------------------------------------------------------

using System;
/// <remarks/>
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
public partial class gamedata {
    
    private gamedataObject[] itemsField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("object", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public gamedataObject[] Items {
        get {
            return this.itemsField;
        }
        set {
            this.itemsField = value;
        }
    }
}

/// <remarks/>
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class gamedataObject {
    
    private string nameField;
    
    private string descriptionField;

    private string commentField;

    private string questPreDescriptionField;

    private string iconField;
    
    private string giveItemNumberField;
    
    private string minimumField;
    
    private string maximumField;
    
    private string equippableField;
    
    private string uuidField;
    
    private string valueField;
    private string statTargetField;
    private string alternateBackgroundField;

    private string deletedField; //Effectively a boolean

    private string displayedOnSidebarField; //Effectively a boolean
    private string displayedOnMainStatPageField; //Effectively a boolean
    private string oneTimeQuestField; //Effectively a bool
    private string questIsForceGrantedField; //Effectively a bool
    private string cooldownTimerField; //Minutes until quest available again for timed quests
    private string questEnergyCostField;
    
    private gamedataObjectItem[] itemField;

    private gamedataObjectStartingCharacterInfo[] startingCharacterInfoField;

    private gamedataObjectStep[] stepField;

    private gamedataObjectStep[] failStepField;
    private gamedataObjectNextStep[] nextStepField;
    
    private gamedataObjectQuest[] questField;
    
    private gamedataObjectQuestStepChoice[] questStepChoiceField;
    
    private gamedataObjectStat[] statField;
    private gamedataObjectStat[] xpStatField;
    private gamedataObjectHenchman[] henchmanField;
    
    private gamedataObjectFirstStep[] firstStepField;
    
    private gamedataObjectItemType[] itemTypeField;

    private gamedataObjectRequiredLocation[] requiredLocationField;

    private gamedataObjectStartingLocation[] startingLocationField;

    private gamedataObjectAssociatedLocation[] associatedLocationField;
    private gamedataObjectUnlockLocation[] unlockLocationField;
    private gamedataObjectMoveToLocation[] moveToLocationField;
    private gamedataObjectAssociatedNPC[] associatedNPCField;
    private gamedataObjectStartingQuest[] startingQuestField;

    private gamedataObjectStatGroup[] statGroupField;

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
    public string giveItemNumber {
        get {
            return this.giveItemNumberField;
        }
        set {
            this.giveItemNumberField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string minimum {
        get {
            if (this.minimumField == null || this.minimumField=="")
            { return "0"; }
            else
            {
                return this.minimumField;
            }
        }
        set {
            this.minimumField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string maximum {
        get {
            if (this.maximumField == null || this.maximumField=="")
            {
                return "0";
            }
            else
            {
                return this.maximumField;
            }
        }
        set {
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
    public string value {
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
    public gamedataObjectItem[] item {
        get {
            return this.itemField;
        }
        set {
            this.itemField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("startingCharacterInfo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public gamedataObjectStartingCharacterInfo[] startingCharacterInfo
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
    public gamedataObjectStep[] step {
        get {
            return this.stepField;
        }
        set {
            this.stepField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("failStep", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public gamedataObjectStep[] failStep
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
    public gamedataObjectNextStep[] nextStep {
        get {
            return this.nextStepField;
        }
        set {
            this.nextStepField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("quest", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true)]
    public gamedataObjectQuest[] quest {
        get {
            return this.questField;
        }
        set {
            this.questField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("questStepChoice", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true)]
    public gamedataObjectQuestStepChoice[] questStepChoice {
        get {
            return this.questStepChoiceField;
        }
        set {
            this.questStepChoiceField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("stat", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true)]
    public gamedataObjectStat[] stat {
        get {
            return this.statField;
        }
        set {
            this.statField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("xpStat", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public gamedataObjectStat[] xpStat
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
    public gamedataObjectHenchman[] henchman {
        get {
            return this.henchmanField;
        }
        set {
            this.henchmanField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("firstStep", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true)]
    public gamedataObjectFirstStep[] firstStep {
        get {
            return this.firstStepField;
        }
        set {
            this.firstStepField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("itemType", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true)]
    public gamedataObjectItemType[] itemType {
        get {
            return this.itemTypeField;
        }
        set {
            this.itemTypeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("requiredLocation", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public gamedataObjectRequiredLocation[] requiredLocation
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
    public gamedataObjectStartingLocation[] startingLocation
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
    public gamedataObjectAssociatedLocation[] associatedLocation
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
    public gamedataObjectUnlockLocation[] unlockLocation
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
    public gamedataObjectMoveToLocation[] moveToLocation
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
    public gamedataObjectAssociatedNPC[] associatedNPC
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
    public gamedataObjectStartingQuest[] startingQuest
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
    public gamedataObjectStatGroup[] statGroup
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
public partial class objectReference
{
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
    }
}

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

public partial class gamedataObjectNextStep: objectReference {

    public gamedataObjectNextStep() : base("QuestStepData")
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
public partial class gamedataObjectFirstStep : objectReference
{

    public gamedataObjectFirstStep() : base("QuestStepData")
    {
    }
}
public partial class gamedataObjectItemType : objectReference
{

    public gamedataObjectItemType() : base("ItemTypeData")
    {
    }
}
public partial class gamedataObjectRequiredLocation : objectReference
{

    public gamedataObjectRequiredLocation() : base("LocationData")
    {
    }
}
public partial class gamedataObjectStartingLocation : objectReference
{

    public gamedataObjectStartingLocation() : base("LocationData")
    {
    }
}
public partial class gamedataObjectAssociatedLocation : objectReference
{

    public gamedataObjectAssociatedLocation() : base("LocationData")
    {
    }
}
public partial class gamedataObjectUnlockLocation : objectReference
{

    public gamedataObjectUnlockLocation() : base("LocationData")
    {
    }
}
public partial class gamedataObjectMoveToLocation : objectReference
{

    public gamedataObjectMoveToLocation() : base("LocationData")
    {
    }
}
public partial class gamedataObjectAssociatedNPC : objectReference
{

    public gamedataObjectAssociatedNPC() : base("NPCData")
    {
    }
}
public partial class gamedataObjectStartingQuest : objectReference
{

    public gamedataObjectStartingQuest() : base("QuestData")
    {
    }
}
public partial class gamedataObjectStatGroup : objectReference
{

    public gamedataObjectStatGroup() : base("StatGroupData")
    {
    }
}

