using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace TaskDialogDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.Text = "Task Dialog Demos";

            int currentButtonCount = 0;
            void AddButtonForAction(string name, Action action)
            {
                int nextButton = ++currentButtonCount;

                var button = new Button()
                {
                    Text = name,
                    Size = new Size(180, 23),
                    Location = new Point(nextButton / 20 * 200 + 20, nextButton % 20 * 30)
                };
                button.Click += (s, e) =>
                {
                    action();
                };
                Controls.Add(button);
            }

            AddButtonForAction("Confirmation Dialog (3x)", ShowSimpleTaskDialog);
            AddButtonForAction("Close Document Confirmation", ShowCloseDocumentTaskDialog);
            AddButtonForAction("Minesweeper Difficulty", ShowMinesweeperDifficultySelectionTaskDialog);
            AddButtonForAction("Auto-Closing Dialog", ShowAutoClosingTaskDialog);
            AddButtonForAction("Multi-Page Dialog (modeless)", ShowMultiPageTaskDialog);
            AddButtonForAction("Elevation Required", ShowElevatedProcessTaskDialog);
            AddButtonForAction("Events Demo", ShowEventsDemoTaskDialog);
        }

        private void ShowSimpleTaskDialog()
        {
            // Show a message box.
            DialogResult messageBoxResult = MessageBox.Show(
                this,
                text: "Stopping the operation might leave your database in a corrupted state. Are you sure you want to stop?",
                caption: "Confirmation [Message Box]",
                buttons: MessageBoxButtons.YesNo,
                icon: MessageBoxIcon.Warning,
                defaultButton: MessageBoxDefaultButton.Button2);
            if (messageBoxResult == DialogResult.Yes)
            {
                Console.WriteLine("User confirmed to stop the operation.");
            }

            // Show a task dialog (simple).// TODO: Add a parameter for setting the default button?
            TaskDialogResult result = TaskDialog.ShowDialog(
                this,
                text: "Stopping the operation might leave your database in a corrupted state.",
                mainInstruction: "Are you sure you want to stop?",
                caption: "Confirmation (Task Dialog)",
                buttons: TaskDialogButtons.Yes | TaskDialogButtons.No,
                icon: TaskDialogIcon.Warning);
            if (result == TaskDialogResult.Yes)
            {
                Console.WriteLine("User confirmed to stop the operation.");
            }

            // Show a task dialog (enhanced).
            var page = new TaskDialogPage()
            {
                MainInstruction = "Are you sure you want to stop?",
                Text = "Stopping the operation might leave your database in a corrupted state.",
                Caption = "Confirmation (Task Dialog)",
                Icon = TaskDialogIcon.Warning,
                EnableHyperlinks = true,
                AllowCancel = true,

                CheckBox = new TaskDialogCheckBox()
                {
                    Text = "Do not show again"
                },

                Footer = new TaskDialogFooter()
                {
                    Text = "<a href=\"link1\">How should I decide?</a>",
                    Icon = TaskDialogIcon.Information
                },

                StandardButtons =
                {
                    new TaskDialogStandardButton(TaskDialogResult.Yes),
                    new TaskDialogStandardButton(TaskDialogResult.No)
                    {
                        DefaultButton = true
                    }
                }
            };

            page.HyperlinkClicked += (s, e) =>
            {
                if (e.Hyperlink == "link1")
                {
                    Process.Start(new ProcessStartInfo("https://dot.net/")
                    {
                        UseShellExecute = true
                    })?.Dispose();
                }
            };

            var dialog = new TaskDialog(page);
            var resultButton = dialog.ShowDialog(this);

            if (resultButton is TaskDialogStandardButton standardButton &&
                standardButton.Result == TaskDialogResult.Yes)
            {
                if (page.CheckBox.Checked)
                    Console.WriteLine("Do not show this confirmation again.");
                Console.WriteLine("User confirmed to stop the operation.");
            }
        }

        private void ShowCloseDocumentTaskDialog()
        {
            // Create the page which we want to show in the dialog.
            var page = new TaskDialogPage()
            {
                Caption = "My Application",
                MainInstruction = "Do you want to save changes to Untitled?",
            };

            TaskDialogStandardButton btnCancel = page.StandardButtons.Add(TaskDialogResult.Cancel);
            TaskDialogCustomButton btnSave = page.CustomButtons.Add("&Save");
            TaskDialogCustomButton btnDontSave = page.CustomButtons.Add("Do&n't save");

            // Show a modal dialog, then check the result.
            var dialog = new TaskDialog(page);
            TaskDialogButton result = dialog.ShowDialog(this);

            if (result == btnSave)
                Console.WriteLine("Saving");
            else if (result == btnDontSave)
                Console.WriteLine("Not saving");
            else
                Console.WriteLine("Canceling");
        }

        private void ShowMinesweeperDifficultySelectionTaskDialog()
        {
            var page = new TaskDialogPage()
            {
                Caption = "Minesweeper",
                MainInstruction = "What level of difficulty do you want to play?",
                CustomButtonStyle = TaskDialogCustomButtonStyle.CommandLinks,
                AllowCancel = true,

                Footer = new TaskDialogFooter()
                {
                    Text = "Note: You can change the difficulty level later " +
                        "by clicking Options on the Game menu.",
                },

                CustomButtons =
                {
                    new TaskDialogCustomButton("&Beginner", "10 mines, 9 x 9 tile grid")
                    {
                        Tag = 10
                    },
                    new TaskDialogCustomButton("&Intermediate", "40 mines, 16 x 16 tile grid")
                    {
                        Tag = 40
                    },
                    new TaskDialogCustomButton("&Advanced", "99 mines, 16 x 30 tile grid")
                    {
                        Tag = 99
                    }
                }
            };

            var dialog = new TaskDialog(page);
            TaskDialogButton result = dialog.ShowDialog(this);

            if (result.Tag is int resultingMines)
                Console.WriteLine($"Playing with {resultingMines} mines...");
            else
                Console.WriteLine("User canceled.");
        }

        private void ShowAutoClosingTaskDialog()
        {
            const string textFormat = "Reconnecting in {0} seconds...";
            int remainingTenthSeconds = 50;

            // Display the form's icon in the task dialog.
            // Note however that the task dialog will not scale the icon.
            var page = new TaskDialogPage()
            {
                MainInstruction = "Connection lost; reconnecting...",
                Text = string.Format(textFormat, (remainingTenthSeconds + 9) / 10),
                Icon = new TaskDialogIcon(this.Icon),
                ProgressBar = new TaskDialogProgressBar()
                {
                    State = TaskDialogProgressBarState.Paused
                }
            };

            var reconnectButton = page.CustomButtons.Add("&Reconnect now");
            var cancelButton = page.StandardButtons.Add(TaskDialogResult.Cancel);

            // Create a WinForms timer that raises the Tick event every second.
            using (var timer = new Timer()
            {
                Enabled = true,
                Interval = 100
            })
            {
                timer.Tick += (s, e) =>
                {
                    remainingTenthSeconds--;
                    if (remainingTenthSeconds > 0)
                    {
                        // Update the remaining time and progress bar.
                        page.Text = string.Format(textFormat, (remainingTenthSeconds + 9) / 10);
                        page.ProgressBar.Value = 100 - remainingTenthSeconds * 2;
                    }
                    else
                    {
                        // Stop the timer and click the "Reconnect" button - this will
                        // close the dialog.
                        timer.Enabled = false;
                        reconnectButton.PerformClick();
                    }
                };

                TaskDialogButton result = new TaskDialog(page).ShowDialog(this);
                if (result == reconnectButton)
                    Console.WriteLine("Reconnecting.");
                else
                    Console.WriteLine("Not reconnecting.");
            }
        }

        private void ShowMultiPageTaskDialog()
        {
            var dialog = new TaskDialog();

            var initialPage = new TaskDialogPage()
            {
                Caption = "My Application",
                MainInstruction = "Clean up database?",
                Text = "Do you really want to do a clean-up?\nThis action is irreversible!",
                Icon = TaskDialogIcon.ShieldWarningYellowBar,
                AllowCancel = true,
                // A modeless dialog can be minimizable.
                CanBeMinimized = true,

                CheckBox = new TaskDialogCheckBox()
                {
                    Text = "I know what I'm doing"
                },

                StandardButtons =
                {
                    new TaskDialogStandardButton(TaskDialogResult.No)
                    {
                        DefaultButton = true
                    },
                    new TaskDialogStandardButton(TaskDialogResult.Yes)
                    {
                        // Disable the "Yes" button and only enable it when the
                        // checkbox is checked.
                        Enabled = false,
                        // Do not close the dialog when this button is clicked.
                        ShouldCloseDialog = false
                    }
                }
            };

            var inProgressPage = new TaskDialogPage()
            {
                Caption = "My Application",
                MainInstruction = "Operation in progress...",
                Text = "Please wait while the operation is in progress.",
                Icon = TaskDialogIcon.Information,
                CanBeMinimized = true,

                ProgressBar = new TaskDialogProgressBar()
                {
                    State = TaskDialogProgressBarState.Marquee
                },

                Expander = new TaskDialogExpander()
                {
                    Text = "Initializing...",
                    Position = TaskDialogExpanderPosition.AfterFooter
                },

                StandardButtons =
                {
                    // For the "In Progress" page, don't allow the dialog to close, by adding
                    // a disabled button (if no button was specified, the task dialog would
                    // get an (enabled) 'OK' button).
                    new TaskDialogStandardButton(TaskDialogResult.Close)
                    {
                        Enabled = false
                    }
                }
            };

            // Add an invisible Cancel button where we will intercept the Click event
            // to prevent the dialog from closing (when the User clicks the "X" button
            // in the title bar or presses ESC or Alt+F4).
            var invisibleCancelButton = inProgressPage.StandardButtons.Add(TaskDialogResult.Cancel);
            invisibleCancelButton.Visible = false;
            invisibleCancelButton.ShouldCloseDialog = false;

            var finishedPage = new TaskDialogPage()
            {
                Caption = "My Application",
                MainInstruction = "Success!",
                Text = "The operation finished.",
                Icon = TaskDialogIcon.ShieldSuccessGreenBar,
                CustomButtonStyle = TaskDialogCustomButtonStyle.CommandLinks,
                CanBeMinimized = true,
                StandardButtons = TaskDialogButtons.Close,
            };

            // Enable the "Yes" button only when the checkbox is checked.
            TaskDialogCheckBox checkBox = initialPage.CheckBox;
            TaskDialogStandardButton initialButtonYes = initialPage.StandardButtons
                [TaskDialogResult.Yes];

            checkBox.CheckedChanged += (sender, e) =>
            {
                initialButtonYes.Enabled = checkBox.Checked;
            };

            // When the user clicks "Yes", navigate to the second page.
            initialButtonYes.Click += (sender, e) =>
            {
                // Navigate to the "In Progress" page that displays the
                // current progress of the background work.
                dialog.Page = inProgressPage;

                // NOTE: When you implement a "In Progress" page that represents
                // background work that is done e.g. by a separate thread/task,
                // which eventually calls Control.Invoke()/BeginInvoke() when
                // its work is finished in order to navigate or update the dialog,
                // then DO NOT start that work here already (directly after
                // setting the Page property). Instead, start the work in the
                // TaskDialogPage.Created event of the new page.
                //
                // See comments in the code sample in https://github.com/dotnet/winforms/issues/146
                // for more information.
            };

            // Simulate work by using a WinForms timer where we are updating the
            // progress bar and the expander with the current status.
            Timer? timer = null;
            inProgressPage.Created += (s, e) =>
            {
                // The page is now being displayed, so create the timer.
                timer = new System.Windows.Forms.Timer()
                {
                    Interval = 200,
                    Enabled = true
                };

                int currentTimerValue = 0;
                timer.Tick += (s2, e2) =>
                {
                    currentTimerValue++;

                    var progressBar = inProgressPage.ProgressBar;
                    if (currentTimerValue >= 15 && currentTimerValue <= 40)
                    {
                        if (currentTimerValue == 15)
                        {
                            // Switch the progress bar to a regular one.
                            progressBar.State = TaskDialogProgressBarState.Normal;
                        }

                        progressBar.Value = (currentTimerValue - 15) * 4;
                        inProgressPage.Expander.Text = $"Progress: {progressBar.Value} %";
                    }
                    else if (currentTimerValue == 41)
                    {
                        // Work is finished, so navigate to the third page.
                        dialog.Page = finishedPage;
                    }
                };
            };
            inProgressPage.Destroyed += (s, e) =>
            {
                // The page is being destroyed, so dispose of the timer.
                timer!.Dispose();
                timer = null;
            };

            TaskDialogCustomButton showResultsButton = finishedPage.CustomButtons.Add(
                "Show &Results");

            // Show the dialog (modeless).
            dialog.Page = initialPage;
            TaskDialogButton result = dialog.ShowDialog();
            if (result == showResultsButton)
            {
                Console.WriteLine("Showing Results!");
            }
        }

        private void ShowElevatedProcessTaskDialog()
        {
            var dialog = new TaskDialog();

            var page = new TaskDialogPage()
            {
                MainInstruction = "Settings saved - Service Restart required",
                Text = "The service needs to be restarted to apply the changes.",
                Icon = TaskDialogIcon.ShieldSuccessGreenBar,
                StandardButtons = TaskDialogButtons.Close,
                CustomButtonStyle = TaskDialogCustomButtonStyle.CommandLinks
            };

            var restartNowButton = page.CustomButtons.Add("&Restart now");
            restartNowButton.ElevationRequired = true;
            restartNowButton.Click += (s, e) =>
            {
                restartNowButton.ShouldCloseDialog = true;
                restartNowButton.Enabled = false;

                // Try to start an elevated cmd.exe.
                var psi = new ProcessStartInfo("cmd.exe", "/k echo Hi, this is an elevated command prompt.")
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process p;
                try
                {
                    p = Process.Start(psi);
                }                
                catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
                {
                    // The user canceled the UAC prompt, so don't close the dialog and
                    // re-enable the restart button.
                    restartNowButton.ShouldCloseDialog = false;
                    restartNowButton.Enabled = true;
                    return;
                }
            };

            dialog.Page = page;
            dialog.ShowDialog(this);
        }

        private void ShowEventsDemoTaskDialog()
        {
            var dialog = new TaskDialog();
            dialog.Opened += (s, e) => Console.WriteLine("Dialog Opened");
            dialog.Shown += (s, e) => Console.WriteLine("Dialog Shown");
            dialog.Closing += (s, e) => Console.WriteLine("Dialog Closing - CloseButton: " + e.CloseButton);
            dialog.Closed += (s, e) => Console.WriteLine("Dialog Closed");

            var page1 = new TaskDialogPage()
            {
                Caption = Text,
                MainInstruction = "Event Demo",
                Text = "<a href=\"linkEvent\">Event</a> <a href=\"linkDemo\">Demo</a>...",
                EnableHyperlinks = true,
                CustomButtonStyle = TaskDialogCustomButtonStyle.CommandLinksNoIcon
            };
            page1.Created += (s, e) => Console.WriteLine("Page1 Created");
            page1.Destroyed += (s, e) => Console.WriteLine("Page1 Destroyed");
            page1.HelpRequest += (s, e) => Console.WriteLine("Page1 HelpRequest");
            page1.HyperlinkClicked += (s, e) => Console.WriteLine("Page1 HyperlinkClicked: " + e.Hyperlink);

            page1.Expander = new TaskDialogExpander("Expander")
            {
                Position = TaskDialogExpanderPosition.AfterFooter
            };
            page1.Expander.ExpandedChanged += (s, e) => Console.WriteLine("Expander ExpandedChanged: " + page1.Expander.Expanded);

            var buttonOK = page1.StandardButtons.Add(TaskDialogResult.OK);
            var buttonHelp = page1.StandardButtons.Add(TaskDialogResult.Help);
            var buttonCancelClose = page1.CustomButtons.Add("C&ancel Close");
            buttonCancelClose.ShouldCloseDialog = false;
            var buttonShowInnerDialog = page1.CustomButtons.Add("&Show (modeless) Inner Dialog", "(and don't cancel the Close)");
            var buttonNavigate = page1.CustomButtons.Add("&Navigate");
            buttonNavigate.ShouldCloseDialog = false;

            buttonOK.Click += (s, e) => Console.WriteLine($"Button '{s}' Click");
            buttonHelp.Click += (s, e) => Console.WriteLine($"Button '{s}' Click");
            buttonCancelClose.Click += (s, e) =>
            {
                Console.WriteLine($"Button '{s}' Click");
            };
            buttonShowInnerDialog.Click += (s, e) =>
            {
                Console.WriteLine($"Button '{s}' Click");
                TaskDialog.ShowDialog("Inner Dialog");
                Console.WriteLine($"(returns) Button '{s}' Click");
            };
            buttonNavigate.Click += (s, e) =>
            {
                Console.WriteLine($"Button '{s}' Click");

                // Navigate to a new page.
                var page2 = new TaskDialogPage()
                {
                    MainInstruction = "AfterNavigation.",
                    StandardButtons = TaskDialogButtons.Close
                };
                page2.Created += (s, e) => Console.WriteLine("Page2 Created");
                page2.Destroyed += (s, e) => Console.WriteLine("Page2 Destroyed");

                dialog.Page = page2;
            };

            page1.CheckBox = new TaskDialogCheckBox("&CheckBox");
            page1.CheckBox.CheckedChanged += (s, e) => Console.WriteLine("CheckBox CheckedChanged: " + page1.CheckBox.Checked);

            var radioButton1 = page1.RadioButtons.Add("Radi&oButton 1");
            var radioButton2 = page1.RadioButtons.Add("RadioB&utton 2");

            radioButton1.CheckedChanged += (s, e) => Console.WriteLine("RadioButton1 CheckedChanged: " + radioButton1.Checked);
            radioButton2.CheckedChanged += (s, e) => Console.WriteLine("RadioButton2 CheckedChanged: " + radioButton2.Checked);

            dialog.Page = page1;
            var dialogResult = dialog.ShowDialog();
            Console.WriteLine("---> Dialog Result: " + dialogResult);
        }
    }
}
