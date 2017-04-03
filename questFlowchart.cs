using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace EvilWindowsEditor
{
/*	public class QuestFlowchart : Drawable
	{
		protected class PseudoButton
		{
			public Rectangle bounds;
			public gamedataObject associatedQuestStep;
            public int column;
            public int row;
		}
		protected List<PseudoButton> questButtons;
		Dictionary<string, PseudoButton> uuidToStepButton;
		public QuestFlowchart()
		{
			questButtons = new List<PseudoButton>();
            questButtons.OrderBy(b=>b.column * 1000 + b.row);//Since there shouldn't ever be more than 1000 rows, this sorts by column first, then row.  There had better not be any thousand-step quests in this game...
			uuidToStepButton = new Dictionary<string, PseudoButton>();
		}
		public void BuildFlowchart()
		{
			var gdv = (gameDataView)DataContext;
			//Populate the flowchart view now.  First, clear all old controls, which should get rid of buttons from last time
			questButtons.Clear();
			uuidToStepButton.Clear();
			List<PseudoButton> notYetUsedStepButtons = new List<PseudoButton>();
			PseudoButton startingButton = null;
			foreach (gamedataObject questStepObj in gdv.root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepData")
																						 && iter.quest != null
																						 && iter.quest.Count() > 0
																						 && iter.quest.First().Value != null
																						 && iter.quest.First().Value.Equals(gdv.gameData.uuid)))
			{
				PseudoButton stepButton = new PseudoButton();
				stepButton.associatedQuestStep = questStepObj;
				questButtons.Add(stepButton);
				notYetUsedStepButtons.Add(stepButton);
				uuidToStepButton.Add(questStepObj.uuid, stepButton);
				if (questStepObj.uuid == gdv.gameData.firstStep.First().Value)
				{
					startingButton = stepButton;
				}
			}
			//Creating a flowchart:  we form columns of these step buttons.
			//The first column consists of the button for the quests starting step.
			//Then, we loop over all the steps in the rightmost column, and for each of those, we look at the following steps; if they are already somewhere, this is a backward loop, if they are not anywhere yet, they go into the next column.
			//When we hit a point where everything in the last column either has no followon, or is a backloop, the next column is empty, so we can stop.
			//Then we loop over all the steps creating arrows
			List<List<PseudoButton>> columnsOfSteps = new List<List<PseudoButton>>();
			List<PseudoButton> firstColumn = new List<PseudoButton>();
			firstColumn.Add(startingButton);
            //startingButton.row = 0;
            //startingButton.column = 0;
			columnsOfSteps.Add(firstColumn);
			notYetUsedStepButtons.Remove(startingButton);
			bool lastColumnEmpty = false;
            int column = 0;
			while (!lastColumnEmpty)
			{
				List<PseudoButton> newColumn = new List<PseudoButton>();
                int currentRow = 0;
				List<PseudoButton> currentColumn = columnsOfSteps.Last();
				foreach (PseudoButton stepButton in currentColumn)
				{
                    stepButton.column = column;
                    stepButton.row = currentRow;
                    currentRow++;
					string stepUUID = stepButton.associatedQuestStep.uuid;

					foreach (gamedataObject choiceObj in gdv.root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceData") && iter.step.Count() > 0 && iter.step[0].Value.Equals(stepUUID)))
					{
						if (choiceObj.nextStep != null && choiceObj.nextStep.Count() > 0)
						{
							string nextStepUUID = choiceObj.nextStep[0].Value;
							PseudoButton nextStepButton = null;
							foreach (PseudoButton iterButton in notYetUsedStepButtons)
							{
								if (iterButton.associatedQuestStep.uuid.Equals(nextStepUUID))
								{
									nextStepButton = iterButton;
									notYetUsedStepButtons.Remove(iterButton);
									break;
								}
							}
							if (nextStepButton != null)
							{
								newColumn.Add(nextStepButton);
							}
						}
					}
				}
				if (newColumn.Count() == 0)
				{
					lastColumnEmpty = true;
				}
				else
				{
					columnsOfSteps.Add(newColumn);
                    column++;
				}
			}
			Invalidate(); 
			//Now that we've changed all the data, force a re-draw.  This only actually matters in the case where you 
			//were on one quest's flowchart view and jump to another quest; that doesn't trigger any redraw, so fails
			//to notice that the flowchart should change.
		}
		protected override void OnPaint(PaintEventArgs pe)
		{
			var gdv = (gameDataView)DataContext;
            //Loop over all the buttons and figure out their widths, and from that, the column bounds, and from that, the bounds of each button.
            int[] columnWidths = new int[questButtons.Count];
            int[] cumulativeColumnWidths = new int[questButtons.Count];
            foreach (PseudoButton currentButton in questButtons)
            {
                int textWidth = (int)pe.Graphics.MeasureString(Fonts.Sans(10), currentButton.associatedQuestStep.name).Width;
                if (textWidth>columnWidths[currentButton.column])
                {
                    columnWidths[currentButton.column] = textWidth;
                }
            }
            int cumulativeWidth = 0;
            for (int i=0;i<cumulativeColumnWidths.Length;i++)
            {
                cumulativeColumnWidths[i] = cumulativeWidth;
                cumulativeWidth = cumulativeWidth + columnWidths[i];
            }
            //Now that we have valid column widths, set the bounds of the current button.
            foreach (PseudoButton currentButton in questButtons)
            {
                currentButton.bounds.Top = 10 + 50 * (currentButton.row); //Buffer of 10 at the top, then each button has a height of 20, plus 30 spacing between.
                currentButton.bounds.Bottom = currentButton.bounds.Top + 20;
                currentButton.bounds.Left = 10 + 36 * currentButton.column + cumulativeColumnWidths[currentButton.column]; //Buffer of 10 on the left, plus 30 spacing between, plus 6 more for padding around the text.
                currentButton.bounds.Right = currentButton.bounds.Left + 6 +  (int)pe.Graphics.MeasureString(Fonts.Sans(10), currentButton.associatedQuestStep.name).Width;
            }

            foreach (PseudoButton currentButton in questButtons)
			{
				Pen pen = new Pen(Colors.Beige);
				pe.Graphics.DrawRectangle(pen, currentButton.bounds.Left, currentButton.bounds.Top, currentButton.bounds.Width, currentButton.bounds.Height);
				pe.Graphics.DrawText(Fonts.Sans(10), new SolidBrush(Colors.Beige), currentButton.bounds.Left + 3, currentButton.bounds.Top + 3, currentButton.associatedQuestStep.name);
				Point startOfArrow = new Point(currentButton.bounds.Right, currentButton.bounds.Top + currentButton.bounds.Height / 2);

				foreach (gamedataObject choiceObj in gdv.root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceData")
																						  && iter.step.Count() > 0
																						  && iter.step[0].Value.Equals(((gamedataObject)currentButton.associatedQuestStep).uuid)))
				{
					if (choiceObj.nextStep != null && choiceObj.nextStep.Count() > 0)
					{
						string nextStepUUID = choiceObj.nextStep[0].Value;
						PseudoButton nextStepButton = null;
						uuidToStepButton.TryGetValue(nextStepUUID, out nextStepButton);
						if (nextStepButton != null)
						{
							Point endOfArrow = new Point(nextStepButton.bounds.Left, nextStepButton.bounds.Top + nextStepButton.bounds.Height / 2);
							pe.Graphics.DrawLine(Colors.Green, startOfArrow, endOfArrow);

						}
					}
				}
			}
			base.OnPaint(pe);
		}
		protected override void OnMouseUp(MouseEventArgs e)
		{

			foreach (PseudoButton currentButton in questButtons)
			{
				if (currentButton.bounds.Contains((Point)e.Location))
				{
					((gameDataView)DataContext).SelectedQuestStep = currentButton.associatedQuestStep;
					break;
				}
			}
			base.OnMouseUp(e);
		}
	}*/
}
