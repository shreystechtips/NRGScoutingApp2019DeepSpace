using System;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NRGScoutingApp
{
    public partial class ChangeEventPage : ContentPage
    {
        public ChangeEventPage()
        {
            InitializeComponent();
            CompetitionList.RefreshCommand = new Command(() =>
            {
                //Do your stuff.
                CompetitionList.IsRefreshing = true;

                try
                {
                    DataDownload.refreshEvents();
                }
                catch (Exception ex)
                {
                    DisplayAlert(ex.ToString(), "", "OK");
                }
                CompetitionList.ItemsSource = null;
                CompetitionList.ItemsSource = App.eventsList.Values;
                CompetitionList.IsRefreshing = false;
            });
            CompetitionList.ItemsSource = App.eventsList.Values;
        }

        async private void competitions_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            string key;
            try
            {
                string name = e.Item as string;
                string eventKey = App.eventsList.FirstOrDefault(x => x.Value == name).Key;
                Preferences.Set(ConstantVars.CURRENT_EVENT_NAME, eventKey);
                await Navigation.PopAsync();
                Navigation.RemovePage(this);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Unknown Error, here it is:\n" + ex.ToString(), "", "OK");
            }
        }

        /// <summary>
        /// Update list when the text in search bar is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SEARCH_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(e.NewTextValue) || String.IsNullOrEmpty(e.NewTextValue))
            {
                CompetitionList.ItemsSource = App.eventsList.Values;
            }
            else
            {
                CompetitionList.ItemsSource = App.eventsList.Values.Where(value => value.ToLower().Trim().Contains(e.NewTextValue.ToLower().Trim()));
            }
        }
    }
}
