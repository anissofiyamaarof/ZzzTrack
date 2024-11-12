using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using Firebase.Database.Query;

namespace ZzzTrack
{
    class FirebaseHelper
    {
        FirebaseClient firebase = new FirebaseClient("https://bmicalculator-a4271-default-rtdb.asia-southeast1.firebasedatabase.app/");

        public async Task AddRecord(string dt, double dr)
        {
            int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.ParseExact(dt, "dd/MM/yyyy", CultureInfo.InvariantCulture), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            string weekPath = $"Week{weekNumber}";

            // Store the sleep record under the current week
            await firebase
                .Child("SleepRecords")
                .Child(weekPath)
                .PostAsync(new SleepRecord() { DateRecorded = dt, Duration = dr });

            await UpdateWeekAverage(weekPath);
        }

        public async Task UpdateWeekAverage(string weekPath)
        {
            // Retrieve all sleep records for the current week
            var query = await firebase
                .Child("SleepRecords")
                .Child(weekPath)
                .OrderByKey()
                .OnceAsync<SleepRecord>();

            double totalDuration = 0;
            int recordCount = 0;

            // Calculate the total duration and count of sleep records
            foreach (var record in query)
            {
                totalDuration += record.Object.Duration;
                recordCount++;
            }

            // Calculate the average sleep duration
            double averageDuration = recordCount > 0 ? totalDuration / recordCount : 0;

            // Update the WeekAverage property in the respective week
            await firebase
                .Child("SleepRecords")
                .Child("WeeklyAverage")
                .Child(weekPath)
                .Child("Average")
                .PutAsync(averageDuration);

            await UpdateStatus(weekPath, averageDuration);
        }

        public async Task UpdateStatus(string weekPath, double averageDuration)
        {
            string status;

            if (averageDuration >= 7.0)
            {
                status = "\"Getting enough sleep is crucial for you because it improves your cognitive function, regulates your mood, strengthens your immune system, and boosts your energy levels throughout the day.\"";
            }
            else if (averageDuration < 7.0)
            {
                status = "\"Not getting enough sleep can result in decreased cognitive function, mood swings, a weakened immune system, and feelings of fatigue and exhaustion throughout the day, leaving you less able to perform at your best.\"";
            }
            else
            {
                status = "null";
            }

            await firebase
                .Child("SleepRecords")
                .Child("WeeklyAverage")
                .Child(weekPath)
                .Child("Status")
                .PutAsync(status);
        }

        public async Task<(string weekPath, double averageDuration)> GetAverageDurationForCurrentWeek()
        {
            DateTime today = DateTime.Today;
            int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(today, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            string weekPath = $"Week{weekNumber}";

            var weekPathReference = firebase
            .Child("SleepRecords")
            .Child("WeeklyAverage")
            .Child(weekPath)
            .Child("Average");

            bool dataExists = await weekPathReference.OnceSingleAsync<double>().ContinueWith(task => !task.IsFaulted);

            if (dataExists)
            {
                double dataSnapshot = await weekPathReference.OnceSingleAsync<double>();
                return (weekPath, dataSnapshot);
            }
            else
            {
                return (weekPath, 0.0);
            }
        }

        public async Task<string> GetStatusForCurrentWeek()
        {
            DateTime today = DateTime.Today;
            int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(today, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            string weekPath = $"Week{weekNumber}";

            var weekPathReference = firebase
            .Child("SleepRecords")
            .Child("WeeklyAverage")
            .Child(weekPath)
            .Child("Status");

            bool dataExists = await weekPathReference.OnceSingleAsync<string>().ContinueWith(task => !task.IsFaulted);

            if (dataExists)
            {
                // Data exists at the path, retrieve and return it
                string dataSnapshot = await weekPathReference.OnceSingleAsync<string>();
                return (dataSnapshot);
            }
            else
            {
                return ("null");
            }
        }

        public async Task<List<string>> GetWeekPaths()
        {
            var weekPaths = new List<string>();

            var snapshot = await firebase
                .Child("SleepRecords")
                .Child("WeeklyAverage")
                .OnceAsync<object>();

            foreach (var item in snapshot)
            {
                weekPaths.Add(item.Key);
            }

            return weekPaths;
        }

        public async Task<List<SleepRecord>> FilterRecordsByWeek(string selectedWeek)
        {
            // Retrieve sleep records for the selected week from Firebase
            var snapshot = await firebase
                .Child("SleepRecords")
                .Child(selectedWeek)
                .OnceAsync<SleepRecord>();

            // Extract sleep records from Firebase response
            var sleepRecords = snapshot.Select(x => x.Object).ToList();

            return sleepRecords;
        }

        public async Task<(string weekPath, double averageDuration)> GetAverageDurationForSelectedWeek(string selectedWeek)
        {
            string weekPath = $"{selectedWeek}";

            var query = await firebase
                .Child("SleepRecords")
                .Child("WeeklyAverage")
                .Child(weekPath)
                .Child("Average")
                .OnceSingleAsync<double>();

            return (weekPath, query);
        }

        public async Task<string> GetStatusForSelectedWeek(string selectedWeek)
        {
            string weekPath = $"{selectedWeek}";

            string query = await firebase
                .Child("SleepRecords")
                .Child("WeeklyAverage")
                .Child(weekPath)
                .Child("Status")
                .OnceSingleAsync<string>();

            return (query);
        }
    }
}
