using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChameleonLib.Model;
using ChameleonLib.Resources;

namespace Chameleon
{
    public partial class MainPage : PhoneApplicationPage
    {
        PeriodicTask periodicTask;
        //ResourceIntensiveTask resourceIntensiveTask;

        private void StartPeriodicAgent()
        {
            // Obtain a reference to the period task, if one exists
            periodicTask = ScheduledActionService.Find(Constants.PERIODIC_TASK_NAME) as PeriodicTask;

            // If the task already exists and background agents are enabled for the
            // application, you must remove the task and then add it again to update 
            // the schedule
            if (periodicTask != null)
            {
                RemoveAgent(Constants.PERIODIC_TASK_NAME);
            }

            periodicTask = new PeriodicTask(Constants.PERIODIC_TASK_NAME);


            // The description is required for periodic agents. This is the string that the user
            // will see in the background services Settings page on the device.
            periodicTask.Description = "Chameleon Live Tiles and Lock screen";

            // Place the call to Add in a try block in case the user has disabled agents.
            try
            {
                ScheduledActionService.Add(periodicTask);
                //PeriodicStackPanel.DataContext = periodicTask;

                // If debugging is enabled, use LaunchForTest to launch the agent in one minute.
#if(DEBUG_AGENT)
                ScheduledActionService.LaunchForTest(Constants.PERIODIC_TASK_NAME, TimeSpan.FromSeconds(30));
                MessageBox.Show("스케줄러가 시작됨");
#endif
            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("BNS Error: The action is disabled"))
                {
                    MessageBox.Show(AppResources.MsgDisabledBackgroundAgent);
                    UseLockscreen.IsChecked = false;
                }

                if (exception.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
                {
                    // No user action required. The system prompts the user when the hard limit of periodic tasks has been reached.

                }
                UseLockscreen.IsChecked = false; ;
            }
            catch (SchedulerServiceException)
            {
                // No user action required.
                //PeriodicCheckBox.IsChecked = false;
            }
        }

        private void RemoveAgent(string name)
        {
            try
            {
                if (ScheduledActionService.Find(name) != null)
                {
                    ScheduledActionService.Remove(name);
#if(DEBUG_AGENT)
                    MessageBox.Show("스케줄러가 제거됨");
#endif
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
