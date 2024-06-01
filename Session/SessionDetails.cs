using Model.ModelSql;

namespace Session
{
    public static class SessionDetails
    {
        public static User loggedInUser;
        public static bool isTimeLog=false;
        public static int timeForPopup = 0;

        public static Timer _timer;
        public static DateTime _startTime;

        //public static int RetrieveCustomConfigSettings()
        //{
        //    TimeSettingsApp times = new TimeSettingsApp();
            
        //    int timeValue = times.Time;
        //    return timeValue;
        //}

        //public static void InitializeTimer()
        //{
        //    _startTime = DateTime.Now;

        //    _timer = new Timer(OnTimerElapsed, null, 0, 1000); // Call immediately, then every second.
        //}

        //public static void OnTimerElapsed(object state)
        //{
        //    // Calculate elapsed time
        //    TimeSpan elapsed = DateTime.Now - _startTime;
        //    if (elapsed.Seconds>(int) RetrieveCustomConfigSettings()*3600)
        //    {
        //        _startTime = DateTime.Now;
        //        isTimeLog = true;
        //    }
           
        //}

        //public static  void OnClosed()
        //{
        //    _timer.Dispose();
        //}
    }
}
