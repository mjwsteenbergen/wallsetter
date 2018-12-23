using System;
using System.Linq;
using System.Threading.Tasks;
using wallsetter.Core;
using wallsetter.Models;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;

namespace wallsetter.BackgroundTasks
{
    public sealed class UpdateScreenImages : BackgroundTask
    {
        public static string Message { get; set; }

        private volatile bool _cancelRequested = false;
        private IBackgroundTaskInstance _taskInstance;
        private BackgroundTaskDeferral _deferral;

        public override async Task Register()
        {
            var taskName = GetType().Name;
            var taskRegistration = BackgroundTaskRegistration.AllTasks.FirstOrDefault(t => t.Value.Name == taskName).Value;

            if (taskRegistration == null)
            {
                var builder = new BackgroundTaskBuilder()
                {
                    Name = taskName
                };

                // TODO WTS: Define the trigger for your background task and set any (optional) conditions
                // More details at https://docs.microsoft.com/windows/uwp/launch-resume/create-and-register-an-inproc-background-task
                builder.SetTrigger(new TimeTrigger((await Settings.GetSettings()).RunEvery, false));
                builder.AddCondition(new SystemCondition(SystemConditionType.FreeNetworkAvailable));
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));

                builder.Register();
            }
        }

        public override Task RunAsyncInternal(IBackgroundTaskInstance taskInstance)
        {
            if (taskInstance == null)
            {
                return null;
            }

            _deferral = taskInstance.GetDeferral();

            return Task.Run(async () =>
            {
                var settings = await Settings.GetSettings();

                await ImageSetter.UpdateImages(settings);

                _deferral?.Complete();

            });
        }

        public override void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _cancelRequested = true;

           // TODO WTS: Insert code to handle the cancelation request here.
           // Documentation: https://docs.microsoft.com/windows/uwp/launch-resume/handle-a-cancelled-background-task
        }
    }
}
