using Eto.Forms;
using Eto.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System;
using System.Linq;

namespace EvilOverlordContentEditor
{
    public class GameObjectPanel: TableLayout
    {

        private TextArea descriptionControlBox;
        private TextArea questPreDescriptionControlBox;
        private TextArea questStepDescriptionControlBox;
        private GridView<gamedataObject> questStepChoicesGrid;
        private WebView descriptionView;
        private WebView questPreDescriptionView;
        private WebView questStepDescriptionView;
        private QuestFlowchart questFlowchartPanel;
        private ComboBox statGroupBox;
        private ComboBox itemTypeBox;
        private ComboBox requiredLocationBox;
        private ComboBox startingLocationBox;
        private ComboBox associatedLocationBox;
        private ComboBox moveToLocationBox;
        private ComboBox unlockLocationBox;
        private ComboBox associatedNPCBox;
        private ComboBox startingQuestBox;
        public gameDataView dataView;
        public GameObjectPanel(gameDataView dv)
        {
            dataView = dv;
            descriptionControlBox = new TextArea() { Height = 100, Width = -1, Wrap = true };
            questPreDescriptionControlBox = new TextArea() { Height = 100, Width = -1, Wrap = true };
            questStepDescriptionControlBox = new TextArea() { Height = 100, Width = -1, Wrap = true };
            descriptionView = new WebView() { Height = 100, Width = -1 };
            descriptionView.LoadHtml(""); //The first time you load some html, it doesn't update the screen.  So, load a placeholder empty string now, and the first item you select will see this properly.
            questPreDescriptionView = new WebView() { Height = 100, Width = -1 };
            questPreDescriptionView.LoadHtml("");
            questStepDescriptionView = new WebView() { Height = 100, Width = -1 };
            questStepDescriptionView.LoadHtml("");
            questStepChoicesGrid = new GridView<gamedataObject>()
            {
                Size = new Size(590, -1),
                AllowMultipleSelection = false
            };
            {
                TableCell selectItemCell = new TableCell() { Control = "Select an Item" };
                selectItemCell.Control.Bind(c => c.Visible, dataView, obj => obj.pickNewObjectVisible);
                TableRow selectItemRow = new TableRow() { Cells = { selectItemCell } };
                Rows.Add(new TableRow(selectItemRow));

            }
            {
                Button deleteButton = new Button() { Text = "Delete this object", ToolTip = "Default" };
                deleteButton.Click += delegate (Object sender, EventArgs a) { dataView.deleteSelectedObject(); }; ;
                deleteButton.Bind(c => c.TextColor, dataView, obj => obj.deleteObjectButtonTextColor);
                deleteButton.Bind(c => c.ToolTip, dataView, obj => obj.deleteObjectButtonTooltip);

                StackLayoutItem deleteButtonLayoutItem = new StackLayoutItem(deleteButton, HorizontalAlignment.Right);
                StackLayout deleteButtonLayout = new StackLayout() { Items = { deleteButtonLayoutItem }, Orientation = Orientation.Vertical };

                TableCell deleteButtonCell = new TableCell() { Control = deleteButtonLayout, ScaleWidth = false };
                //deleteButtonCell.Control.Bind(c => c.Visible, this, obj => obj.afterLoad);
                TableRow loadFileRow = new TableRow() { Cells = { deleteButtonCell } };

                Rows.Add(loadFileRow);
            }
            {
                TextBox nameControlBox = new TextBox();
                nameControlBox.TextBinding.Bind(dataView, obj => obj.name);
                Label nameLabel = new Label { Text = "Name:" };
                StackLayoutItem nameLabelCell = new StackLayoutItem(nameLabel, false);
                StackLayoutItem nameBoxCell = new StackLayoutItem(nameControlBox, true); ;
                StackLayout nameTable = new StackLayout() { Items = { nameLabelCell, nameBoxCell }, Orientation = Orientation.Horizontal };
                nameTable.Bind(c => c.Visible, dataView, obj => obj.nameVisible);
                Rows.Add(new TableRow(nameTable));
            }
            {
                TextArea commentTextArea = new TextArea() { Height = 100, Width = -1, Wrap = true };
                commentTextArea.TextBinding.Bind(dataView, obj => obj.comment);
                Label commentLabel = new Label { Text = "Comment:" };
                StackLayoutItem commentLabelCell = new StackLayoutItem(commentLabel, false);
                StackLayoutItem commentBoxCell = new StackLayoutItem(commentTextArea, true); ;
                StackLayout commentTable = new StackLayout() { Items = { commentLabelCell, commentBoxCell }, Orientation = Orientation.Horizontal };
                commentTable.Bind(c => c.Visible, dataView, obj => obj.nameVisible);
                Rows.Add(new TableRow(commentTable));
            }
            {
                TextBox iconTextBox = new TextBox();
                iconTextBox.TextBinding.Bind(dataView, obj => obj.icon);
                Label iconLabel = new Label { Text = "Image:" };
                StackLayoutItem iconLabelCell = new StackLayoutItem(iconLabel, false);
                StackLayoutItem iconBoxCell = new StackLayoutItem(iconTextBox, true); ;
                StackLayout iconTable = new StackLayout() { Items = { iconLabelCell, iconBoxCell }, Orientation = Orientation.Horizontal };
                iconTable.Bind(c => c.Visible, dataView, obj => obj.iconVisible);
                Rows.Add(new TableRow(iconTable));
            }
            {
                StackLayout descriptionTableOuter = CreateTextWithWebPreview("Description", descriptionControlBox, descriptionView);
                descriptionControlBox.TextChanged += (sender, e) =>
                {
                    dataView.description = WebUtility.HtmlEncode(((TextArea)sender).Text);
                    loadDescriptionPreview(((TextArea)sender).Text, DescriptionSelector.Description);
                };
                descriptionTableOuter.Bind(c => c.Visible, dataView, obj => obj.descriptionVisible);
                TableCell descriptionTableOuterCell = new TableCell(descriptionTableOuter, true);
                Rows.Add(new TableRow(descriptionTableOuterCell));
            }
            {
                StackLayout questPreDescriptionTableOuter = CreateTextWithWebPreview("Description Seen Before Accepting Quest", questPreDescriptionControlBox, questPreDescriptionView);
                questPreDescriptionControlBox.TextChanged += (sender, e) =>
                {
                    dataView.questPreDescription = WebUtility.HtmlEncode(((TextArea)sender).Text);
                    loadDescriptionPreview(((TextArea)sender).Text, DescriptionSelector.QuestPreDescription);
                };
                questPreDescriptionTableOuter.Bind(c => c.Visible, dataView, obj => obj.questVisible);
                Rows.Add(new TableRow(questPreDescriptionTableOuter));
            }
            {
                CheckBox equippableBox = new CheckBox() { Text = "Equippable:", Checked = true };

                TableCell equippableBoxCell = new TableCell(equippableBox, true);
                equippableBox.Bind(c => c.Checked, dataView, obj => obj.equippable);
                equippableBox.Bind(c => c.Visible, dataView, obj => obj.equippableVisible);
                equippableBox.Text = "Equippable";
                Rows.Add(new TableRow(equippableBoxCell));
            }
            {
                CheckBox oneTimeOnlyBox = new CheckBox() { Text = "Quest is one time only:", Checked = true };
                oneTimeOnlyBox.Bind(c => c.Checked, dataView, obj => obj.oneTimeQuest);
                oneTimeOnlyBox.Text = "One time only";
                StackLayoutItem oneTimeOnlyBoxCell = new StackLayoutItem(oneTimeOnlyBox, false);
                Label cooldownTimerLabel = new Label() { Text = "               Cooldown timer til you can run quest again (in minutes):" };
                TextBox cooldownTimerText = new TextBox() { Width = 50 };
                cooldownTimerText.Bind(c => c.Text, dataView, obj => obj.cooldownTimer);
                cooldownTimerText.Bind(c => c.Visible, dataView, obj => obj.questVisible);
                StackLayoutItem cooldownTimerLabelCell = new StackLayoutItem(cooldownTimerLabel, false);
                StackLayoutItem cooldownTimerTextCell = new StackLayoutItem(cooldownTimerText, false); ;
                StackLayout cooldownTimerTable = new StackLayout() { Items = { oneTimeOnlyBoxCell, cooldownTimerLabelCell, cooldownTimerTextCell }, Orientation = Orientation.Horizontal };
                cooldownTimerTable.Bind(c => c.Visible, dataView, obj => obj.questVisible);
                Rows.Add(new TableRow(cooldownTimerTable));
            }
            {
                Label itemTypeLabel = new Label { Text = "Item Type:" };
                StackLayoutItem itemTypeLabelCell = new StackLayoutItem(itemTypeLabel, false);
                itemTypeBox = new ComboBox() { ReadOnly = true };    // text is the value of the dictionary
                itemTypeBox.DataStore = dataView.itemTypes;
                itemTypeBox.ItemTextBinding = Binding.Property((gamedataObject r) => r.name);
                itemTypeBox.ItemKeyBinding = Binding.Property((gamedataObject r) => r.uuid);
                itemTypeBox.Bind(c => c.SelectedKey, dataView, obj => obj.itemType);
                StackLayoutItem itemTypeBoxCell = new StackLayoutItem(itemTypeBox, false);
                StackLayout itemTypeTable = new StackLayout() { Items = { itemTypeLabelCell, itemTypeBoxCell }, Orientation = Orientation.Horizontal };
                itemTypeTable.Bind(c => c.Visible, dataView, obj => obj.itemTypeVisible);
                Rows.Add(new TableRow(itemTypeTable));
            }
            {
                Label statGroupLabel = new Label { Text = "Stat Group:" };
                StackLayoutItem statGroupLabelCell = new StackLayoutItem(statGroupLabel, false);
                statGroupBox = new ComboBox() { ReadOnly = true };    // text is the value of the dictionary
                statGroupBox.DataStore = dataView.statGroups;
                statGroupBox.ItemTextBinding = Binding.Property((gamedataObject r) => r.name);
                statGroupBox.ItemKeyBinding = Binding.Property((gamedataObject r) => r.uuid);
                statGroupBox.Bind(c => c.SelectedKey, dataView, obj => obj.itemType);
                StackLayoutItem statGroupBoxCell = new StackLayoutItem(statGroupBox, false);
                StackLayout statGroupTable = new StackLayout() { Items = { statGroupLabelCell, statGroupBoxCell }, Orientation = Orientation.Horizontal };
                statGroupTable.Bind(c => c.Visible, dataView, obj => obj.statGroupVisible);
                Rows.Add(new TableRow(statGroupTable));
            }
            {
                Label henchmanStatLabel = new Label { Text = "Stats:" };
                StackLayoutItem henchmanStatLabelCell = new StackLayoutItem(henchmanStatLabel, false);
                Button addNewStatButton = new Button() { Text = "Add new stat" };
                addNewStatButton.Click += delegate (Object sender, EventArgs a) { dataView.addNewHenchmanStat(); };
                StackLayoutItem henchmanStatAddButtonCell = new StackLayoutItem(addNewStatButton, false);
                StackLayout henchmanStatHeader = new StackLayout() { Items = { henchmanStatLabelCell, henchmanStatAddButtonCell }, Orientation = Orientation.Horizontal };
                StackLayoutItem henchmanStatHeaderCell = new StackLayoutItem(henchmanStatHeader, false);
                GridView<gamedataObject> henchmanStatGrid = new GridView<gamedataObject>()
                {
                    Size = new Size(600, -1)
                };
                henchmanStatGrid.DataStore = dataView.henchmanStats;
                henchmanStatGrid.Enabled = true;
                henchmanStatGrid.Columns.Add(new GridColumn()
                {
                    HeaderText = "Stat                    ",
                    Editable = true,

                    DataCell = new ComboBoxCell("statID")
                    {
                        DataStore = dataView.stats,
                        ComboTextBinding = Binding.Property((gamedataObject r) => r.name),
                        ComboKeyBinding = Binding.Property((gamedataObject r) => r.uuid),
                    }

                });
                henchmanStatGrid.Columns.Add(new GridColumn()
                {
                    HeaderText = "Modifier",
                    DataCell = new TextBoxCell("value") { },
                    Editable = true
                }
                );
                StackLayoutItem henchmanStatGridCell = new StackLayoutItem(henchmanStatGrid, true);
                StackLayout henchmanStatStack = new StackLayout() { Items = { henchmanStatHeaderCell, henchmanStatGridCell }, Orientation = Orientation.Vertical };
                henchmanStatStack.Bind(c => c.Visible, dataView, obj => obj.henchmanVisible);
                Rows.Add(new TableRow(henchmanStatStack));
            }
            {
                Label itemStatModifierLabel = new Label { Text = "Stat Modifiers:" };
                StackLayoutItem itemStatModifierLabelCell = new StackLayoutItem(itemStatModifierLabel, false);
                Button addNewStatModifierButton = new Button() { Text = "Add new stat modifier" };
                addNewStatModifierButton.Click += new EventHandler<EventArgs>(delegate (Object sender, EventArgs a) { dataView.addNewStatModifier(); });
                StackLayoutItem itemStatModifierAddButtonCell = new StackLayoutItem(addNewStatModifierButton, false);
                StackLayout itemStatModifierHeader = new StackLayout() { Items = { itemStatModifierLabelCell, itemStatModifierAddButtonCell }, Orientation = Orientation.Horizontal };
                StackLayoutItem itemStatModifierHeaderCell = new StackLayoutItem(itemStatModifierHeader, false);
                GridView<gamedataObject> itemStatModifierGrid = new GridView<gamedataObject>()
                {
                    Size = new Size(600, -1)
                };
                itemStatModifierGrid.DataStore = dataView.itemStatModifiersObservable;
                itemStatModifierGrid.Enabled = true;
                itemStatModifierGrid.Columns.Add(new GridColumn()
                {
                    HeaderText = "Stat                    ",
                    Editable = true,
                    DataCell = new ComboBoxCell("statID")
                    {
                        DataStore = dataView.stats,
                        ComboTextBinding = Binding.Property((gamedataObject r) => r.name),
                        ComboKeyBinding = Binding.Property((gamedataObject r) => r.uuid),
                    }

                });
                itemStatModifierGrid.Columns.Add(new GridColumn()
                {
                    HeaderText = "Modifier",
                    DataCell = new TextBoxCell("value") { },
                    Editable = true
                }
                );
                StackLayoutItem itemStatModifierGridCell = new StackLayoutItem(itemStatModifierGrid, true);
                StackLayout itemStatModifierStack = new StackLayout() { Items = { itemStatModifierHeaderCell, itemStatModifierGridCell }, Orientation = Orientation.Vertical };
                itemStatModifierStack.Bind(c => c.Visible, dataView, obj => obj.itemStatModifiersVisible);
                Rows.Add(new TableRow(itemStatModifierStack));
            }
            {
                Label requiredLocationLabel = new Label { Text = "Required Location (leave blank if none):" };
                StackLayoutItem requiredLocationCell = new StackLayoutItem(requiredLocationLabel, false);
                requiredLocationBox = new ComboBox() { ReadOnly = true };
                requiredLocationBox.DataStore = dataView.locations;
                requiredLocationBox.ItemTextBinding = Binding.Property((gamedataObject r) => r.name);
                requiredLocationBox.ItemKeyBinding = Binding.Property((gamedataObject r) => r.uuid);
                requiredLocationBox.Bind(c => c.SelectedKey, dataView, obj => obj.requiredLocation);
                StackLayoutItem requiredLocationBoxCell = new StackLayoutItem(requiredLocationBox, false);
                StackLayout requiredLocationTable = new StackLayout() { Items = { requiredLocationCell, requiredLocationBoxCell }, Orientation = Orientation.Horizontal };
                requiredLocationTable.Bind(c => c.Visible, dataView, obj => obj.questVisible);
                Rows.Add(new TableRow(requiredLocationTable));
            }
            {
                Label startingLocation = new Label { Text = "Character's Starting Location:" };
                StackLayoutItem startingLocationCell = new StackLayoutItem(startingLocation, false);
                startingLocationBox = new ComboBox() { ReadOnly = true };
                startingLocationBox.DataStore = dataView.locations;
                startingLocationBox.ItemTextBinding = Binding.Property((gamedataObject r) => r.name);
                startingLocationBox.ItemKeyBinding = Binding.Property((gamedataObject r) => r.uuid);
                startingLocationBox.Bind(c => c.SelectedKey, dataView, obj => obj.startingLocation);
                StackLayoutItem startingLocationBoxCell = new StackLayoutItem(startingLocationBox, false);
                StackLayout startingLocationTable = new StackLayout() { Items = { startingLocationCell, startingLocationBoxCell }, Orientation = Orientation.Horizontal };
                startingLocationTable.Bind(c => c.Visible, dataView, obj => obj.characterStartingInfoVisible);
                Rows.Add(new TableRow(startingLocationTable));
            }
            {
                Label startingQuest = new Label { Text = "Character's Starting Quest:" };
                StackLayoutItem startingQuestCell = new StackLayoutItem(startingQuest, false);
                startingQuestBox = new ComboBox() { ReadOnly = true };
                startingQuestBox.DataStore = dataView.quests;
                startingQuestBox.ItemTextBinding = Binding.Property((gamedataObject r) => r.name);
                startingQuestBox.ItemKeyBinding = Binding.Property((gamedataObject r) => r.uuid);
                startingQuestBox.Bind(c => c.SelectedKey, dataView, obj => obj.startingQuest);
                StackLayoutItem startingQuestBoxCell = new StackLayoutItem(startingQuestBox, false);
                StackLayout startingQuestTable = new StackLayout() { Items = { startingQuestCell, startingQuestBoxCell }, Orientation = Orientation.Horizontal };
                startingQuestTable.Bind(c => c.Visible, dataView, obj => obj.characterStartingInfoVisible);
                Rows.Add(new TableRow(startingQuestTable));
            }
            {
                Label questStatRequirementLabel = new Label { Text = "Stat Requirements:" };
                StackLayoutItem questStatRequirementLabelCell = new StackLayoutItem(questStatRequirementLabel, false);
                Button addNewStatRequirementButton = new Button() { Text = "Add new stat requirement" };
                addNewStatRequirementButton.Click += new EventHandler<EventArgs>(delegate (Object sender, EventArgs a) { dataView.addNewQuestStatRequirement(); });
                Button deleteSelectedStatRequirementButton = new Button() { Text = "Delete selected stat requirement" };
                GridView<gamedataObject> questStatRequirementGrid = new GridView<gamedataObject>()
                {
                    Width = -1,
                    Height = -1,
                };
                deleteSelectedStatRequirementButton.Click += new EventHandler<EventArgs>(delegate (Object sender, EventArgs a)
                {
                    if (questStatRequirementGrid.SelectedItem != null)
                    {
                        dataView.deleteObject(questStatRequirementGrid.SelectedItem);
                    }
                });
                StackLayoutItem questStatRequirementAddButtonCell = new StackLayoutItem(addNewStatRequirementButton, false);
                StackLayoutItem deleteSelectedStatRequirementButtonCell = new StackLayoutItem(deleteSelectedStatRequirementButton, false);
                StackLayout questStatRequirementHeader = new StackLayout() { Items = { questStatRequirementLabelCell, questStatRequirementAddButtonCell, deleteSelectedStatRequirementButtonCell }, Orientation = Orientation.Horizontal };
                StackLayoutItem questStatRequirementHeaderCell = new StackLayoutItem(questStatRequirementHeader, false);

                questStatRequirementGrid.DataStore = dataView.questStatRequirementsObservable;
                questStatRequirementGrid.Enabled = true;

                questStatRequirementGrid.Columns.Add(new GridColumn()
                {
                    HeaderText = "Stat                    ",
                    Editable = true,
                    DataCell = new ComboBoxCell("statID")
                    {
                        DataStore = dataView.stats,
                        ComboTextBinding = Binding.Property((gamedataObject r) => r.name),
                        ComboKeyBinding = Binding.Property((gamedataObject r) => r.uuid),
                    }

                });
                questStatRequirementGrid.Columns.Add(new GridColumn()
                {
                    HeaderText = "Minimum",
                    DataCell = new TextBoxCell("minimum") { },
                    Editable = true
                }
                );
                questStatRequirementGrid.Columns.Add(new GridColumn()
                {
                    HeaderText = "Maximum",
                    DataCell = new TextBoxCell("maximum") { },
                    Editable = true
                }
                );
                StackLayoutItem questStatRequirementGridCell = new StackLayoutItem(questStatRequirementGrid, true);
                StackLayout questStatRequirementStack = new StackLayout() { Items = { questStatRequirementHeaderCell, questStatRequirementGridCell }, Orientation = Orientation.Vertical };
                questStatRequirementStack.Bind(c => c.Visible, dataView, obj => obj.questStatRequirementsVisible);
                Rows.Add(new TableRow(questStatRequirementStack));
            }
            {
                //Quest step panel

                TableLayout questStepPanel = new TableLayout()
                {
                    Size = new Size(-1, -1),
                    Spacing = new Size(5, 5),
                    Padding = new Padding(10, 0, 0, 0), //Add a left margin to the step panel, so that everything indents a bit
                    BackgroundColor = Colors.Beige //Background color, to help visually distinguish the step panel from the rest of the quest stuff
                };

                questStepPanel.Bind(c => c.Visible, dataView, obj => obj.questStepPanelVisible);
                {
                    Button returnToFlowchartButton = new Button() { Text = "Return to flowchart view of quest" };
                    returnToFlowchartButton.Click += (sender, e) => { dataView.ReturnToFlowchartView(); };
                    StackLayoutItem buttonCell = new StackLayoutItem(returnToFlowchartButton, false);
                    StackLayoutItem paddingCell = new StackLayoutItem(null, true);
                    StackLayout buttonTable = new StackLayout() { Items = { buttonCell, paddingCell }, Orientation = Orientation.Horizontal, BackgroundColor = Colors.Beige };
                    questStepPanel.Rows.Add(new TableRow(buttonTable));
                }
                {
                    TextBox stepNameControlBox = new TextBox() { Width = -1, };
                    stepNameControlBox.TextBinding.Bind(dataView, obj => obj.stepName);
                    Label nameLabel = new Label { Text = "Step Name:" };
                    StackLayoutItem nameLabelCell = new StackLayoutItem(nameLabel, false);
                    StackLayoutItem nameBoxCell = new StackLayoutItem(stepNameControlBox, true); ;
                    StackLayout nameTable = new StackLayout() { Items = { nameLabelCell, nameBoxCell }, Orientation = Orientation.Horizontal, BackgroundColor = Colors.Beige };
                    questStepPanel.Rows.Add(new TableRow(nameTable));
                }
                {
                    StackLayout questStepDescriptionTableOuter = CreateTextWithWebPreview("Step Description", questStepDescriptionControlBox, questStepDescriptionView);
                    questStepDescriptionControlBox.TextChanged += (sender, e) =>
                    {
                        dataView.stepDescription = WebUtility.HtmlEncode(((TextArea)sender).Text);
                        loadDescriptionPreview(((TextArea)sender).Text, DescriptionSelector.QuestStepDescription);
                    };
                    questStepPanel.Rows.Add(new TableRow(questStepDescriptionTableOuter));
                }
                {
                    Label moveToLocation = new Label { Text = "Move character to location:" };
                    StackLayoutItem moveToLocationCell = new StackLayoutItem(moveToLocation, false);
                    moveToLocationBox = new ComboBox() { ReadOnly = true };
                    moveToLocationBox.DataStore = dataView.locations;
                    moveToLocationBox.ItemTextBinding = Binding.Property((gamedataObject r) => r.name);
                    moveToLocationBox.ItemKeyBinding = Binding.Property((gamedataObject r) => r.uuid);
                    moveToLocationBox.Bind(c => c.SelectedKey, dataView, obj => obj.moveToLocation);
                    StackLayoutItem moveToLocationBoxCell = new StackLayoutItem(moveToLocationBox, false);
                    StackLayout associatedLocationTable = new StackLayout() { Items = { moveToLocationCell, moveToLocationBoxCell }, Orientation = Orientation.Horizontal };
                    associatedLocationTable.Bind(c => c.Visible, dataView, obj => obj.questStepPanelVisible);
                    questStepPanel.Rows.Add(new TableRow(associatedLocationTable));
                }
                {
                    Label unlockLocation = new Label { Text = "Permanently unlock location:" };
                    StackLayoutItem unlockLocationCell = new StackLayoutItem(unlockLocation, false);
                    unlockLocationBox = new ComboBox() { ReadOnly = true };
                    unlockLocationBox.DataStore = dataView.locations;
                    unlockLocationBox.ItemTextBinding = Binding.Property((gamedataObject r) => r.name);
                    unlockLocationBox.ItemKeyBinding = Binding.Property((gamedataObject r) => r.uuid);
                    unlockLocationBox.Bind(c => c.SelectedKey, dataView, obj => obj.unlockLocation);
                    StackLayoutItem unlockLocationBoxCell = new StackLayoutItem(unlockLocationBox, false);
                    StackLayout unlockLocationTable = new StackLayout() { Items = { unlockLocationCell, unlockLocationBoxCell }, Orientation = Orientation.Horizontal };
                    unlockLocationTable.Bind(c => c.Visible, dataView, obj => obj.questStepPanelVisible);
                    questStepPanel.Rows.Add(new TableRow(unlockLocationTable));
                }
                {
                    Label associatedLocation = new Label { Text = "Associated Location:" };
                    StackLayoutItem associatedLocationCell = new StackLayoutItem(associatedLocation, false);
                    associatedLocationBox = new ComboBox() { ReadOnly = true };
                    associatedLocationBox.DataStore = dataView.locations;
                    associatedLocationBox.ItemTextBinding = Binding.Property((gamedataObject r) => r.name);
                    associatedLocationBox.ItemKeyBinding = Binding.Property((gamedataObject r) => r.uuid);
                    associatedLocationBox.Bind(c => c.SelectedKey, dataView, obj => obj.associatedLocation);
                    StackLayoutItem associatedLocationBoxCell = new StackLayoutItem(associatedLocationBox, false);
                    StackLayout associatedLocationTable = new StackLayout() { Items = { associatedLocationCell, associatedLocationBoxCell }, Orientation = Orientation.Horizontal };
                    associatedLocationTable.Bind(c => c.Visible, dataView, obj => obj.questStepPanelVisible);
                    questStepPanel.Rows.Add(new TableRow(associatedLocationTable));
                }
                {
                    Label associatedNPC = new Label { Text = "Associated NPC:" };
                    StackLayoutItem associatedNPCCell = new StackLayoutItem(associatedNPC, false);
                    associatedNPCBox = new ComboBox() { ReadOnly = true };
                    associatedNPCBox.DataStore = dataView.npcs;
                    associatedNPCBox.ItemTextBinding = Binding.Property((gamedataObject r) => r.name);
                    associatedNPCBox.ItemKeyBinding = Binding.Property((gamedataObject r) => r.uuid);
                    associatedNPCBox.Bind(c => c.SelectedKey, dataView, obj => obj.associatedNPC);
                    StackLayoutItem associatedNPCBoxCell = new StackLayoutItem(associatedNPCBox, false);
                    StackLayout associatedNPCTable = new StackLayout() { Items = { associatedNPCCell, associatedNPCBoxCell }, Orientation = Orientation.Horizontal };
                    associatedNPCTable.Bind(c => c.Visible, dataView, obj => obj.questStepPanelVisible);
                    questStepPanel.Rows.Add(new TableRow(associatedNPCTable));
                }
                {
                    Label questStepItemGrantsLabel = new Label { Text = "Items granted on entering this step:" };
                    StackLayoutItem questStepItemGrantsCell = new StackLayoutItem(questStepItemGrantsLabel, false);
                    Button questStepItemGrantsButton = new Button() { Text = "Add new item grant" };
                    questStepItemGrantsButton.Click += delegate (Object sender, EventArgs a) { dataView.addNewQuestStepItemGrant(); };
                    StackLayoutItem questStepItemGrantsButtonCell = new StackLayoutItem(questStepItemGrantsButton, false);
                    Button deleteSelectedItemGrantButton = new Button() { Text = "Delete the selected item grant" };
                    StackLayoutItem deleteSelectedItemGrantButtonCell = new StackLayoutItem(deleteSelectedItemGrantButton, false);
                    GridView<gamedataObject> questStepItemGrantsGrid = new GridView<gamedataObject>()
                    {
                        Size = new Size(590, -1),
                        AllowMultipleSelection = false
                    };
                    deleteSelectedItemGrantButton.Click += new EventHandler<EventArgs>(delegate (Object sender, EventArgs a)
                    {
                        if (questStepItemGrantsGrid.SelectedItem != null)
                        {
                            dataView.deleteObject(questStepItemGrantsGrid.SelectedItem);
                        }
                    });
                    StackLayout questStepItemGrantsHeader = new StackLayout() { Items = { questStepItemGrantsCell, questStepItemGrantsButtonCell, deleteSelectedItemGrantButtonCell }, Orientation = Orientation.Horizontal };
                    StackLayoutItem questStepItemGrantsHeaderCell = new StackLayoutItem(questStepItemGrantsHeader, false);
                    questStepItemGrantsGrid.DataStore = dataView.selectedQuestStepItemGrantsObservable;
                    questStepItemGrantsGrid.Enabled = true;
                    questStepItemGrantsGrid.Columns.Add(new GridColumn()
                    {
                        HeaderText = "Item To Grant                    ",
                        Resizable = true,
                        Editable = true,
                        DataCell = new ComboBoxCell("itemID")
                        {
                            DataStore = dataView.items,
                            ComboTextBinding = Binding.Property((gamedataObject r) => r.name),
                            ComboKeyBinding = Binding.Property((gamedataObject r) => r.uuid),
                        }
                    });
                    questStepItemGrantsGrid.Columns.Add(new GridColumn()
                    {
                        HeaderText = "Number To Grant                    ",
                        Resizable = true,
                        Editable = true,
                        DataCell = new TextBoxCell("integerValue") { },

                    });
                    StackLayoutItem questStepItemGrantsGridCell = new StackLayoutItem(questStepItemGrantsGrid, true);
                    StackLayout questStepItemGrantsStack = new StackLayout() { Items = { questStepItemGrantsHeaderCell, questStepItemGrantsGridCell }, Orientation = Orientation.Vertical };
                    questStepPanel.Rows.Add(new TableRow(questStepItemGrantsStack));
                }
                {
                    Label questStepStatGrantsLabel = new Label { Text = "Stats granted on entering this step:" };
                    StackLayoutItem questStepStatGrantsCell = new StackLayoutItem(questStepStatGrantsLabel, false);
                    Button questStepStatGrantsButton = new Button() { Text = "Add new stat grant" };
                    questStepStatGrantsButton.Click += delegate (Object sender, EventArgs a) { dataView.addNewQuestStepStatGrant(); };
                    StackLayoutItem questStepStatGrantsButtonCell = new StackLayoutItem(questStepStatGrantsButton, false);
                    Button deleteSelectedStatGrantButton = new Button() { Text = "Delete the selected stat grant" };
                    StackLayoutItem deleteSelectedStatGrantButtonCell = new StackLayoutItem(deleteSelectedStatGrantButton, false);
                    GridView<gamedataObject> questStepStatGrantsGrid = new GridView<gamedataObject>()
                    {
                        Size = new Size(590, -1),
                        AllowMultipleSelection = false
                    };
                    deleteSelectedStatGrantButton.Click += new EventHandler<EventArgs>(delegate (Object sender, EventArgs a)
                    {
                        if (questStepStatGrantsGrid.SelectedItem != null)
                        {
                            dataView.deleteObject(questStepStatGrantsGrid.SelectedItem);
                        }
                    });
                    StackLayout questStepStatGrantsHeader = new StackLayout() { Items = { questStepStatGrantsCell, questStepStatGrantsButtonCell, deleteSelectedStatGrantButtonCell }, Orientation = Orientation.Horizontal };
                    StackLayoutItem questStepStatGrantsHeaderCell = new StackLayoutItem(questStepStatGrantsHeader, false);
                    questStepStatGrantsGrid.DataStore = dataView.selectedQuestStepStatGrantsObservable;
                    questStepStatGrantsGrid.Enabled = true;
                    questStepStatGrantsGrid.Columns.Add(new GridColumn()
                    {
                        HeaderText = "Stat To Grant                    ",
                        Resizable = true,
                        Editable = true,
                        DataCell = new ComboBoxCell("statID")
                        {
                            DataStore = dataView.stats,
                            ComboTextBinding = Binding.Property((gamedataObject r) => r.name),
                            ComboKeyBinding = Binding.Property((gamedataObject r) => r.uuid),
                        }
                    });
                    questStepStatGrantsGrid.Columns.Add(new GridColumn()
                    {
                        HeaderText = "Amount To Grant                    ",
                        Resizable = true,
                        Editable = true,
                        DataCell = new TextBoxCell("integerValue") { },

                    });
                    StackLayoutItem questStepStatGrantsGridCell = new StackLayoutItem(questStepStatGrantsGrid, true);
                    StackLayout questStepStatGrantsStack = new StackLayout() { Items = { questStepStatGrantsHeaderCell, questStepStatGrantsGridCell }, Orientation = Orientation.Vertical };
                    questStepPanel.Rows.Add(new TableRow(questStepStatGrantsStack));
                }
                {
                    Label questStepHenchmanGrantsLabel = new Label { Text = "Henchman granted on entering this step:" };
                    StackLayoutItem questStepHenchmanGrantsCell = new StackLayoutItem(questStepHenchmanGrantsLabel, false);
                    Button questStepHenchmanGrantsButton = new Button() { Text = "Add new henchman grant" };
                    questStepHenchmanGrantsButton.Click += delegate (Object sender, EventArgs a) { dataView.addNewQuestStepHenchmanGrant(); };
                    StackLayoutItem questStepHenchmanGrantsButtonCell = new StackLayoutItem(questStepHenchmanGrantsButton, false);
                    Button deleteSelectedHenchmanGrantButton = new Button() { Text = "Delete the selected henchman grant" };
                    StackLayoutItem deleteSelectedHenchmanGrantButtonCell = new StackLayoutItem(deleteSelectedHenchmanGrantButton, false);
                    GridView<gamedataObject> questStepHenchmanGrantsGrid = new GridView<gamedataObject>()
                    {
                        Size = new Size(590, -1),
                        AllowMultipleSelection = false
                    };
                    deleteSelectedHenchmanGrantButton.Click += new EventHandler<EventArgs>(delegate (Object sender, EventArgs a)
                    {
                        if (questStepHenchmanGrantsGrid.SelectedItem != null)
                        {
                            dataView.deleteObject(questStepHenchmanGrantsGrid.SelectedItem);
                        }
                    });
                    StackLayout questStepHenchmanGrantsHeader = new StackLayout() { Items = { questStepHenchmanGrantsCell, questStepHenchmanGrantsButtonCell, deleteSelectedHenchmanGrantButtonCell }, Orientation = Orientation.Horizontal };
                    StackLayoutItem questStepHenchmanGrantsHeaderCell = new StackLayoutItem(questStepHenchmanGrantsHeader, false);
                    questStepHenchmanGrantsGrid.DataStore = dataView.selectedQuestStepHenchmanGrantsObservable;
                    questStepHenchmanGrantsGrid.Enabled = true;
                    questStepHenchmanGrantsGrid.Columns.Add(new GridColumn()
                    {
                        HeaderText = "Henchman To Grant                    ",
                        Resizable = true,
                        Editable = true,
                        DataCell = new ComboBoxCell("henchmanID")
                        {
                            DataStore = dataView.henchmen,
                            ComboTextBinding = Binding.Property((gamedataObject r) => r.name),
                            ComboKeyBinding = Binding.Property((gamedataObject r) => r.uuid),
                        }
                    });
                    StackLayoutItem questStepHenchmanGrantsGridCell = new StackLayoutItem(questStepHenchmanGrantsGrid, true);
                    StackLayout questStepHenchmanGrantsStack = new StackLayout() { Items = { questStepHenchmanGrantsHeaderCell, questStepHenchmanGrantsGridCell }, Orientation = Orientation.Vertical };
                    questStepPanel.Rows.Add(new TableRow(questStepHenchmanGrantsStack));
                }
                {
                    Label questStepChoicesLabel = new Label { Text = "Step Choices:" };
                    StackLayoutItem questStepChoicesCell = new StackLayoutItem(questStepChoicesLabel, false);
                    Button questStepChoicesButton = new Button() { Text = "Add new choice" };
                    questStepChoicesButton.Click += delegate (Object sender, EventArgs a) { dataView.addNewQuestStepChoice(); };
                    StackLayoutItem questStepChoicesButtonCell = new StackLayoutItem(questStepChoicesButton, false);
                    Button newStepButton = new Button() { Text = "Add new step as followup to the selected choice" };
                    Button deleteSelectedChoiceButton = new Button() { Text = "Delete the selected choice" };
                    StackLayoutItem deleteSelectedChoiceButtonCell = new StackLayoutItem(deleteSelectedChoiceButton, false);
                    deleteSelectedChoiceButton.Click += new EventHandler<EventArgs>(delegate (Object sender, EventArgs a)
                    {
                        if (questStepChoicesGrid.SelectedItem != null)
                        {
                            dataView.deleteObject(questStepChoicesGrid.SelectedItem);
                        }
                    });
                    StackLayout questStepChoicesHeader = new StackLayout() { Items = { questStepChoicesCell, questStepChoicesButtonCell, newStepButton, deleteSelectedChoiceButtonCell }, Orientation = Orientation.Horizontal };
                    StackLayoutItem questStepChoicesHeaderCell = new StackLayoutItem(questStepChoicesHeader, false);
                    questStepChoicesGrid.DataStore = dataView.selectedQuestStepChoicesObservable;
                    questStepChoicesGrid.Enabled = true;
                    questStepChoicesGrid.Columns.Add(new GridColumn()
                    {
                        //The column default-sizes to the width of the header text, hence the spaces to pad it out.
                        HeaderText = "Name                   ",
                        Resizable = true,
                        DataCell = new TextBoxCell("name") { },
                        Editable = true
                    }
                    );
                    questStepChoicesGrid.Columns.Add(new GridColumn()
                    {
                        HeaderText = "Description                                                                   ",
                        Resizable = true,
                        DataCell = new TextBoxCell("description") { },
                        Editable = true,
                        AutoSize = true

                    }
                    );
                    questStepChoicesGrid.Columns.Add(new GridColumn()
                    {
                        HeaderText = "Next Step                    ",
                        Resizable = true,
                        Editable = true,
                        DataCell = new ComboBoxCell("nextStepID")
                        {
                            DataStore = dataView.selectedQuestStepsObservable,
                            ComboTextBinding = Binding.Property((gamedataObject r) => r.name),
                            ComboKeyBinding = Binding.Property((gamedataObject r) => r.uuid),
                        }
                    });
                    newStepButton.Click += delegate (Object sender, EventArgs e)
                    {
                        if (questStepChoicesGrid.SelectedItems.Count() > 0)
                        {
                            dataView.addNewQuestStepToChoice(questStepChoicesGrid.SelectedItems.First());
                        }
                    };
                    questStepChoicesGrid.SelectionChanged += new EventHandler<EventArgs>(delegate (Object sender, EventArgs a)
                    {
                        dataView.SelectedQuestStepChoice = questStepChoicesGrid.SelectedItem;
                    });
                    StackLayoutItem questStepChoicesGridCell = new StackLayoutItem(questStepChoicesGrid, true);
                    StackLayout questStepChoicesStack = new StackLayout() { Items = { questStepChoicesHeaderCell, questStepChoicesGridCell }, Orientation = Orientation.Vertical };
                    questStepPanel.Rows.Add(new TableRow(questStepChoicesStack));
                }
                {
                    Label questStepChoiceItemGrantsLabel = new Label { Text = "Items granted on selecting this choice:" };
                    StackLayoutItem questStepChoiceItemGrantsCell = new StackLayoutItem(questStepChoiceItemGrantsLabel, false);
                    Button questStepChoiceItemGrantsButton = new Button() { Text = "Add new item granted by this choice" };
                    questStepChoiceItemGrantsButton.Click += delegate (Object sender, EventArgs a) { dataView.addNewQuestStepChoiceItemGrant(); };
                    StackLayoutItem questStepChoiceItemGrantsButtonCell = new StackLayoutItem(questStepChoiceItemGrantsButton, false);
                    Button deleteSelectedChoiceItemGrantButton = new Button() { Text = "Delete the selected item grant" };
                    StackLayoutItem deleteSelectedChoiceItemGrantButtonCell = new StackLayoutItem(deleteSelectedChoiceItemGrantButton, false);
                    GridView<gamedataObject> questStepChoiceItemGrantsGrid = new GridView<gamedataObject>()
                    {
                        Size = new Size(590, -1),
                        AllowMultipleSelection = false
                    };
                    deleteSelectedChoiceItemGrantButton.Click += new EventHandler<EventArgs>(delegate (Object sender, EventArgs a)
                    {
                        if (questStepChoiceItemGrantsGrid.SelectedItem != null)
                        {
                            dataView.deleteObject(questStepChoiceItemGrantsGrid.SelectedItem);
                        }
                    });
                    StackLayout questStepChoiceItemGrantsHeader = new StackLayout() { Items = { questStepChoiceItemGrantsCell, questStepChoiceItemGrantsButtonCell, deleteSelectedChoiceItemGrantButtonCell }, Orientation = Orientation.Horizontal };
                    StackLayoutItem questStepChoiceItemGrantsHeaderCell = new StackLayoutItem(questStepChoiceItemGrantsHeader, false);
                    questStepChoiceItemGrantsGrid.DataStore = dataView.selectedQuestStepChoiceItemGrantsObservable;
                    questStepChoiceItemGrantsGrid.Enabled = true;
                    questStepChoiceItemGrantsGrid.Columns.Add(new GridColumn()
                    {
                        HeaderText = "Item To Grant                    ",
                        Resizable = true,
                        Editable = true,
                        DataCell = new ComboBoxCell("itemID")
                        {
                            DataStore = dataView.items,
                            ComboTextBinding = Binding.Property((gamedataObject r) => r.name),
                            ComboKeyBinding = Binding.Property((gamedataObject r) => r.uuid),
                        }
                    });
                    questStepChoiceItemGrantsGrid.Columns.Add(new GridColumn()
                    {
                        HeaderText = "Number To Grant                    ",
                        Resizable = true,
                        Editable = true,
                        DataCell = new TextBoxCell("integerValue") { },

                    });
                    StackLayoutItem questStepChoiceItemGrantsGridCell = new StackLayoutItem(questStepChoiceItemGrantsGrid, true);
                    StackLayout questStepChoiceItemGrantsStack = new StackLayout() { Items = { questStepChoiceItemGrantsHeaderCell, questStepChoiceItemGrantsGridCell }, Orientation = Orientation.Vertical };
                    questStepChoiceItemGrantsStack.Bind(c => c.Visible, dataView, obj => obj.questStepChoiceDataVisible);
                    questStepPanel.Rows.Add(new TableRow(questStepChoiceItemGrantsStack));
                }
                {
                    Label questStepChoiceStatGrantsLabel = new Label { Text = "Stats granted on selecting this choice:" };
                    StackLayoutItem questStepChoiceStatGrantsCell = new StackLayoutItem(questStepChoiceStatGrantsLabel, false);
                    Button questStepChoiceStatGrantsButton = new Button() { Text = "Add new stat grant" };
                    questStepChoiceStatGrantsButton.Click += delegate (Object sender, EventArgs a) { dataView.addNewQuestStepChoiceStatGrant(); };
                    StackLayoutItem questStepChoiceStatGrantsButtonCell = new StackLayoutItem(questStepChoiceStatGrantsButton, false);
                    Button deleteSelectedStatGrantButton = new Button() { Text = "Delete the selected stat grant" };
                    StackLayoutItem deleteSelectedStatGrantButtonCell = new StackLayoutItem(deleteSelectedStatGrantButton, false);
                    GridView<gamedataObject> questStepChoiceStatGrantsGrid = new GridView<gamedataObject>()
                    {
                        Size = new Size(590, -1),
                        AllowMultipleSelection = false
                    };
                    deleteSelectedStatGrantButton.Click += new EventHandler<EventArgs>(delegate (Object sender, EventArgs a)
                    {
                        if (questStepChoiceStatGrantsGrid.SelectedItem != null)
                        {
                            dataView.deleteObject(questStepChoiceStatGrantsGrid.SelectedItem);
                        }
                    });
                    StackLayout questStepChoiceStatGrantsHeader = new StackLayout() { Items = { questStepChoiceStatGrantsCell, questStepChoiceStatGrantsButtonCell, deleteSelectedStatGrantButtonCell }, Orientation = Orientation.Horizontal };
                    StackLayoutItem questStepChoiceStatGrantsHeaderCell = new StackLayoutItem(questStepChoiceStatGrantsHeader, false);
                    questStepChoiceStatGrantsGrid.DataStore = dataView.selectedQuestStepChoiceStatGrantsObservable;
                    questStepChoiceStatGrantsGrid.Enabled = true;
                    questStepChoiceStatGrantsGrid.Columns.Add(new GridColumn()
                    {
                        HeaderText = "Stat To Grant                    ",
                        Resizable = true,
                        Editable = true,
                        DataCell = new ComboBoxCell("statID")
                        {
                            DataStore = dataView.stats,
                            ComboTextBinding = Binding.Property((gamedataObject r) => r.name),
                            ComboKeyBinding = Binding.Property((gamedataObject r) => r.uuid),
                        }
                    });
                    questStepChoiceStatGrantsGrid.Columns.Add(new GridColumn()
                    {
                        HeaderText = "Amount To Grant                    ",
                        Resizable = true,
                        Editable = true,
                        DataCell = new TextBoxCell("integerValue") { },

                    });
                    StackLayoutItem questStepChoiceStatGrantsGridCell = new StackLayoutItem(questStepChoiceStatGrantsGrid, true);
                    StackLayout questStepChoiceStatGrantsStack = new StackLayout() { Items = { questStepChoiceStatGrantsHeaderCell, questStepChoiceStatGrantsGridCell }, Orientation = Orientation.Vertical };
                    questStepChoiceStatGrantsStack.Bind(c => c.Visible, dataView, obj => obj.questStepChoiceDataVisible);
                    questStepPanel.Rows.Add(new TableRow(questStepChoiceStatGrantsStack));
                }
                {
                    Label questStepHenchmanGrantsLabel = new Label { Text = "Henchman granted on selecting this choice:" };
                    StackLayoutItem questStepHenchmanGrantsCell = new StackLayoutItem(questStepHenchmanGrantsLabel, false);
                    Button questStepHenchmanGrantsButton = new Button() { Text = "Add new henchman grant" };
                    questStepHenchmanGrantsButton.Click += delegate (Object sender, EventArgs a) { dataView.addNewQuestStepChoiceHenchmanGrant(); };
                    StackLayoutItem questStepHenchmanGrantsButtonCell = new StackLayoutItem(questStepHenchmanGrantsButton, false);
                    Button deleteSelectedHenchmanGrantButton = new Button() { Text = "Delete the selected henchman grant" };
                    StackLayoutItem deleteSelectedHenchmanGrantButtonCell = new StackLayoutItem(deleteSelectedHenchmanGrantButton, false);
                    GridView<gamedataObject> questStepHenchmanGrantsGrid = new GridView<gamedataObject>()
                    {
                        Size = new Size(590, -1),
                        AllowMultipleSelection = false
                    };
                    deleteSelectedHenchmanGrantButton.Click += new EventHandler<EventArgs>(delegate (Object sender, EventArgs a)
                    {
                        if (questStepHenchmanGrantsGrid.SelectedItem != null)
                        {
                            dataView.deleteObject(questStepHenchmanGrantsGrid.SelectedItem);
                        }
                    });
                    StackLayout questStepHenchmanGrantsHeader = new StackLayout() { Items = { questStepHenchmanGrantsCell, questStepHenchmanGrantsButtonCell, deleteSelectedHenchmanGrantButtonCell }, Orientation = Orientation.Horizontal };
                    StackLayoutItem questStepHenchmanGrantsHeaderCell = new StackLayoutItem(questStepHenchmanGrantsHeader, false);
                    questStepHenchmanGrantsGrid.DataStore = dataView.selectedQuestStepChoiceHenchmanGrantsObservable;
                    questStepHenchmanGrantsGrid.Enabled = true;
                    questStepHenchmanGrantsGrid.Columns.Add(new GridColumn()
                    {
                        HeaderText = "Henchman To Grant                    ",
                        Resizable = true,
                        Editable = true,
                        DataCell = new ComboBoxCell("henchmanID")
                        {
                            DataStore = dataView.henchmen,
                            ComboTextBinding = Binding.Property((gamedataObject r) => r.name),
                            ComboKeyBinding = Binding.Property((gamedataObject r) => r.uuid),
                        }
                    });
                    StackLayoutItem questStepHenchmanGrantsGridCell = new StackLayoutItem(questStepHenchmanGrantsGrid, true);
                    StackLayout questStepHenchmanGrantsStack = new StackLayout() { Items = { questStepHenchmanGrantsHeaderCell, questStepHenchmanGrantsGridCell }, Orientation = Orientation.Vertical };
                    questStepHenchmanGrantsStack.Bind(c => c.Visible, dataView, obj => obj.questStepChoiceDataVisible);
                    questStepPanel.Rows.Add(new TableRow(questStepHenchmanGrantsStack));
                }
                Rows.Add(new TableRow(questStepPanel));
            }


            {
                questFlowchartPanel = new QuestFlowchart() { BackgroundColor = Colors.DeepSkyBlue, Size = new Size(600, 600) };
                questFlowchartPanel.DataContext = dataView;
                questFlowchartPanel.Bind(c => c.Visible, dataView, obj => obj.questFlowchartVisible);
                Rows.Add(new TableRow(questFlowchartPanel));
            }















            //Final row to fill remaining vertical space, if any.
            Rows.Add(new TableRow { ScaleHeight = true });

        }
        public void rebuildQuestStepChoicesGrid()
        {
            questStepChoicesGrid.DataStore = dataView.selectedQuestStepChoicesObservable;
        }
        private void rebuildComboBox(ComboBox box, System.Collections.Generic.IEnumerable<object> datastore)
        {
            if (box != null)
            {
                box.SuspendLayout();
                box.DataStore = null;
                box.ItemTextBinding.Unbind();
                box.ItemKeyBinding.Unbind();
                box.DataStore = datastore;
                box.ItemTextBinding.Unbind();
                box.ItemTextBinding = Binding.Property((gamedataObject r) => r.name);
                box.ItemKeyBinding = Binding.Property((gamedataObject r) => r.uuid);
                box.Invalidate();
                box.ResumeLayout();
            }
            //It was already set to this, but if we re-set it, that triggers the right refresh of the names
            //in the combo box, and just sending a NotifyPropertyChanged on the list or on an individual
            //element of the list doesn't work.
            //Note that we don't need to do this for stats, because they're in a grid and the grid cells
            //get rebuilt each time, so they'll automatically be right when rebuilt.
        }
        public void rebuildLocationBoxes()
        {
            rebuildComboBox(requiredLocationBox, dataView.locations);
            rebuildComboBox(startingLocationBox, dataView.locations);
            rebuildComboBox(associatedLocationBox, dataView.locations);
            rebuildComboBox(unlockLocationBox, dataView.locations);
            rebuildComboBox(moveToLocationBox, dataView.locations);
        }
        public void rebuildItemTypeBox()
        {
            rebuildComboBox(itemTypeBox, dataView.itemTypes);
        }
        public void rebuildStatGroupBox()
        {
            rebuildComboBox(statGroupBox, dataView.statGroups);
        }
        public void rebuildQuestBox()
        {
            rebuildComboBox(startingQuestBox, dataView.quests);
        }
        public void rebuildNPCBox()
        {
            rebuildComboBox(associatedNPCBox, dataView.npcs);
        }
        public void updateFlowchartPanel()
        {
            questFlowchartPanel.BuildFlowchart();
        }
        public StackLayout CreateTextWithWebPreview(string name, TextArea controlBox, WebView view)
        {
            StackLayoutItem descriptionLabelCell = new StackLayoutItem(new Label { Text = name + ":" }, false);
            StackLayoutItem descriptionBoxCell = new StackLayoutItem(controlBox, true);
            StackLayout descriptionTable = new StackLayout() { Items = { descriptionLabelCell, descriptionBoxCell }, Orientation = Orientation.Horizontal };

            StackLayoutItem descriptionViewLabelCell = new StackLayoutItem(new Label { Text = name + " Preview:" }, false);
            StackLayoutItem descriptionViewCell = new StackLayoutItem(view, true);
            StackLayout descriptionViewTable = new StackLayout() { Items = { descriptionViewLabelCell, descriptionViewCell }, Orientation = Orientation.Horizontal };
            StackLayout descriptionTableOuter = new StackLayout() { Items = { descriptionTable, descriptionViewTable }, Orientation = Orientation.Vertical, HorizontalContentAlignment = HorizontalAlignment.Stretch };
            return descriptionTableOuter;
        }
        public void updateDescriptionAndWebView(string descriptionText, DescriptionSelector desc)
        {
            TextArea descArea;
            switch (desc)
            {
                case DescriptionSelector.QuestPreDescription:
                    {
                        descArea = questPreDescriptionControlBox;
                        break;
                    }
                case DescriptionSelector.QuestStepDescription:
                    {
                        descArea = questStepDescriptionControlBox;
                        break;
                    }

                default:
                    {
                        descArea = descriptionControlBox;
                        break;
                    }
            }
            descArea.Text = WebUtility.HtmlDecode(descriptionText);
            loadDescriptionPreview(descArea.Text, desc);
        }
        private void loadDescriptionPreview(string descriptionText, DescriptionSelector desc)
        {
            WebView descView;
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(descriptionText ?? ""));
            switch (desc)
            {
                case DescriptionSelector.QuestPreDescription:
                    {
                        descView = questPreDescriptionView;
                        break;
                    }
                case DescriptionSelector.QuestStepDescription:
                    {
                        descView = questStepDescriptionView;
                        break;
                    }

                default:
                    {
                        descView = descriptionView;
                        break;
                    }
            }
            descView.LoadHtml(ms);
        }
    }
}