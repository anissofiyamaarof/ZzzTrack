namespace ZzzTrack;

public partial class Information : ContentPage
{
	public Information()
	{
		InitializeComponent();
	}

    async void OnItemClickedAsync(object sender, EventArgs e)
    {
        var aboutPage = new About();
        await Navigation.PushAsync(aboutPage);
    }


}