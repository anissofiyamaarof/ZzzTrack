
using Microsoft.Maui.Controls;

namespace ZzzTrack
{
    public partial class MainPage : ContentPage
    {
        FirebaseHelper firebaseHelper = new FirebaseHelper();

        public MainPage()
        {
            InitializeComponent();

            selectDate.Date = DateTime.Today;
            selectDate.MaximumDate = DateTime.Today;
        }

        void onDatePickerSelected(object sender, DateChangedEventArgs e)
        {
            var selectedDate = e.NewDate.ToString();
        }

        async void OnSaveRecord(object sender, EventArgs e)
        {
            var selectdate = selectDate.Date.ToString("dd/MM/yyyy");
            var duration = Double.Parse(inputHour.Text);
            await firebaseHelper.AddRecord(selectdate, duration);
            await DisplayAlert("Record Saved", "Your sleep duration record has been saved", "OK");
            inputHour?.Unfocus();

            (string weekPath, double averageDuration) = await firebaseHelper.GetAverageDurationForCurrentWeek();
            string status = await firebaseHelper.GetStatusForCurrentWeek();

            averageSleepLabel.Text = $"Current Average Sleep Duration ({weekPath}):";
            averageDurationLabel.Text = averageDuration.ToString("0.00");
            statusLabel.Text = status;

            SetStatusImage();

            inputHour.Text = null;
            selectDate.Date = DateTime.Today;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            (string weekPath, double averageDuration) = await firebaseHelper.GetAverageDurationForCurrentWeek();
            string status = await firebaseHelper.GetStatusForCurrentWeek();

            averageSleepLabel.Text = $"Current Average Sleep Duration ({weekPath}):";
            averageDurationLabel.Text = averageDuration.ToString("0.00");
            statusLabel.Text = status;


            SetStatusImage();

            inputHour.Text = null;
            selectDate.Date = DateTime.Today;
        }

        private async void SetStatusImage()
        {

            imageContainer.Children.Clear();

            (string weekPath, double averageDuration) = await firebaseHelper.GetAverageDurationForCurrentWeek();
            Image statusImage = new Image();

            if (averageDuration >= 7.0)
            {
                statusImage.Source = "goodstatus.png";
            }
            else if (averageDuration < 7.0 && averageDuration > 0)
            {
                statusImage.Source = "badstatus";
            }
            else if (averageDuration == 0) 
            {
                statusImage.Source = " ";
            }

            imageContainer.Children.Add(statusImage);
        }


    async void OnItemClickedAsync(object sender, EventArgs e)
        {
            var aboutPage = new About();
            await Navigation.PushAsync(aboutPage);
        }

    }

}
