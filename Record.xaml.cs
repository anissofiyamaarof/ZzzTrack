using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace ZzzTrack;

public partial class Record : ContentPage
{
    FirebaseHelper firebaseHelper = new FirebaseHelper();

    public Record()
	{
		InitializeComponent();
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        // Bind week paths to the Picker's ItemsSource
        weekPicker.ItemsSource = await firebaseHelper.GetWeekPaths();
        averageDurationLabel.Text = null;
        displayRecord.ItemsSource = null;
        statusLabel.Text = null;
        imageContainer.Children.Clear();
    }

    async void OnItemClickedAsync(object sender, EventArgs e)
    {
        var aboutPage = new About();
        await Navigation.PushAsync(aboutPage);
    }

    private async void OnWeekPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        if (weekPicker != null)
        {
            // Get the selected week path from the Picker
            string selectedWeek = weekPicker.SelectedItem?.ToString();
            if (selectedWeek != null)
            {
                List<SleepRecord> filteredRecords = await firebaseHelper.FilterRecordsByWeek(selectedWeek);
                displayRecord.ItemsSource = filteredRecords;

                (string weekPath, double averageDuration) = await firebaseHelper.GetAverageDurationForSelectedWeek(selectedWeek);
                string status = await firebaseHelper.GetStatusForSelectedWeek(selectedWeek);

                averageSleepLabel.Text = $"Current Average Sleep Duration:";
                averageDurationLabel.Text = averageDuration.ToString("0.00");
                statusLabel.Text = status;
                SetStatusImage(selectedWeek);
            }
        }

    }

    private async void SetStatusImage(string selectedWeek)
    {

        imageContainer.Children.Clear();

        (string weekPath, double averageDuration) = await firebaseHelper.GetAverageDurationForSelectedWeek(selectedWeek);
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

}