using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace EvilWindowsEditor
{
	public class QuestFlowchart : Canvas
	{
		protected class PseudoButton
		{
			public Button button=new Button();
			public gamedataObject associatedQuestStep;
            public int column;
            public int row;
            public double left;
            public double top;
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
            var gdv = DataContext as gameDataView;
            //Populate the flowchart view now.  First, clear all old controls, which should get rid of buttons from last time
            Children.Clear();//Kill the actual buttons and lines on the canvas
			questButtons.Clear();
			uuidToStepButton.Clear();
            MinWidth = 10;
            MinHeight = 10; //They get expanded below, after placing the buttons.
			List<PseudoButton> notYetUsedStepButtons = new List<PseudoButton>();
			PseudoButton startingButton = null;
			foreach (gamedataObject questStepObj in gdv.selectedQuestStepsObservable)
			{
                if (questStepObj.uuid.Equals(""))
                    continue;
				PseudoButton stepButton = new PseudoButton();
				stepButton.associatedQuestStep = questStepObj;
                stepButton.button.Content = questStepObj.name;
                questButtons.Add(stepButton);
				notYetUsedStepButtons.Add(stepButton);
				uuidToStepButton.Add(questStepObj.uuid, stepButton);
				if (questStepObj.uuid.Equals(gdv.gameData.firstStepID))
				{
					startingButton = stepButton;
				}
                stepButton.button.Click += delegate (object sender, RoutedEventArgs e) {
                    ((gameDataView)DataContext).SelectedQuestStep = questStepObj;
                };
                Children.Add(stepButton.button);//Add the actual button to the canvas; placement is arbitrary right now but will be fixed up below
                SetZIndex(stepButton.button, 1);
			}

            //While creating a quest, we hit this...
            if (notYetUsedStepButtons.Count() == 0)
                return;

			//Creating a flowchart:  we form columns of these step buttons.
			//The first column consists of the button for the quests starting step.
			//Then, we loop over all the steps in the rightmost column, and for each of those, we look at the following steps; if they are already somewhere, this is a backward loop, if they are not anywhere yet, they go into the next column.
			//When we hit a point where everything in the last column either has no followon, or is a backloop, the next column is empty, so we can stop.
			//Then we loop over all the steps creating arrows
			List<List<PseudoButton>> columnsOfSteps = new List<List<PseudoButton>>();
			List<PseudoButton> firstColumn = new List<PseudoButton>();
			firstColumn.Add(startingButton);
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

					foreach (gamedataObject choiceObj in gdv.root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceData") 
                                                                                                && iter.deleted.Equals("False") 
                                                                                                && iter.stepID.Equals(stepUUID)))
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
            //Place un-reachable nodes so they don't overlap the actual root node; right now they're still at row 0 col 0:
            int unusedButtonRow = 1;
            foreach (PseudoButton unusedButton in notYetUsedStepButtons)
            {
                unusedButton.row = unusedButtonRow;
                unusedButtonRow++;
            }

            UpdateLayout();//This does the _internal_ layout on all of the buttons, which causes their widths to be set according to the text in them, which is needed below.

            //Now that all the buttons are set up, shuffle them around to the right locations and add lines.
            //Loop over all the buttons and figure out their widths, and from that, the column bounds, and from that, the bounds of each button.
            double[] columnWidths = new double[questButtons.Count];
            double[] cumulativeColumnWidths = new double[questButtons.Count];
            foreach (PseudoButton currentButton in questButtons)
            {
                double textWidth = currentButton.button.ActualWidth;
                if (textWidth>columnWidths[currentButton.column])
                {
                    columnWidths[currentButton.column] = textWidth;
                }
            }
            double cumulativeWidth = 0;
            for (int i=0;i<cumulativeColumnWidths.Length;i++)
            {
                cumulativeColumnWidths[i] = cumulativeWidth;
                cumulativeWidth = cumulativeWidth + columnWidths[i] +36;
            }
            //Now that we have valid column widths, set the location of the current button.
            foreach (PseudoButton currentButton in questButtons)
            {
                currentButton.top = 10 + 50 * (currentButton.row);
                SetTop(currentButton.button, currentButton.top);
                if (currentButton.top + currentButton.button.ActualHeight +10 > MinHeight)
                {
                    MinHeight = currentButton.top + currentButton.button.ActualHeight + 10;
                }
                currentButton.left = 10 + cumulativeColumnWidths[currentButton.column];
                if (currentButton.left +currentButton.button.ActualWidth+10 > MinWidth)
                {
                    MinWidth = currentButton.left + currentButton.button.ActualWidth + 10;
                }
                SetLeft(currentButton.button, currentButton.left);

            }
            foreach (PseudoButton currentButton in questButtons)
			{
				foreach (gamedataObject choiceObj in gdv.root.Items.Where<gamedataObject>(iter => iter.@class.Equals("QuestStepChoiceData")
                                                                                          && iter.deleted.Equals("False") 
										   												  && iter.stepID.Equals(((gamedataObject)currentButton.associatedQuestStep).uuid)))
				{
					if (choiceObj.nextStep != null && choiceObj.nextStep.Count() > 0)
					{
						string nextStepUUID = choiceObj.nextStepID;
                        if (nextStepUUID != null && nextStepUUID!="")
                        {
                            PseudoButton nextStepButton = null;
                            uuidToStepButton.TryGetValue(nextStepUUID, out nextStepButton);
                            if (nextStepButton != null)
                            {
                                Line line = new Line();
                                line.Visibility = System.Windows.Visibility.Visible;
                                line.StrokeThickness = 2;

                                //var x1 = GetRight(currentButton.button);
                                line.X1 = currentButton.left + currentButton.button.ActualWidth;
                                line.X2 = nextStepButton.left;
                                line.Y1 = currentButton.top + currentButton.button.ActualHeight / 2;
                                line.Y2 = nextStepButton.top + nextStepButton.button.ActualHeight / 2;
                                Children.Add(line);
                                //Arrowhead:
                                var arrowheadlength = 7;
                                Point source = new Point(line.X1, line.Y1);
                                Point destination = new Point(line.X2, line.Y2);
                                Matrix rotatorone = new Matrix();
                                Vector backvect = (source - destination);
                                backvect.Normalize();
                                backvect = backvect * arrowheadlength;
                                rotatorone.Rotate(45);
                                Line arrowlineone = new Line();
                                arrowlineone.X1= line.X2;
                                arrowlineone.Y1 = line.Y2;
                                Point arrowlineoneend= new Point(line.X2,line.Y2) + backvect * rotatorone;
                                arrowlineone.X2 = arrowlineoneend.X;
                                arrowlineone.Y2 = arrowlineoneend.Y;
                                Line arrowlinetwo = new Line();
                                arrowlinetwo.X1 = line.X2;
                                arrowlinetwo.Y1 = line.Y2;
                                Matrix rotatortwo = new Matrix();
                                rotatortwo.Rotate(-45);
                                Point arrowlinetwoend = new Point(line.X2, line.Y2) + backvect * rotatortwo;
                                arrowlinetwo.X2 = arrowlinetwoend.X;
                                arrowlinetwo.Y2 = arrowlinetwoend.Y;

                                arrowlineone.Visibility = System.Windows.Visibility.Visible;
                                arrowlineone.StrokeThickness = 2;
                                arrowlinetwo.Visibility = System.Windows.Visibility.Visible;
                                arrowlinetwo.StrokeThickness = 2;
                                if (line.X1 < line.X2)
                                {
                                    line.Stroke = System.Windows.Media.Brushes.Black;
                                    arrowlineone.Stroke = System.Windows.Media.Brushes.Black;
                                    arrowlinetwo.Stroke = System.Windows.Media.Brushes.Black;
                                }
                                else
                                {
                                    line.Stroke = System.Windows.Media.Brushes.DarkGray;
                                    arrowlineone.Stroke = System.Windows.Media.Brushes.DarkGray;
                                    arrowlinetwo.Stroke = System.Windows.Media.Brushes.DarkGray;
                                }
                                Children.Add(arrowlineone);
                                Children.Add(arrowlinetwo);
                                SetZIndex(line, 0);
                            }
                        }
					}
				}
			}
		}
	}
}
