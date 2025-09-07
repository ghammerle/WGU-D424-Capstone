using C424Assessment.DataRepository;
using CommunityToolkit.Maui.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C424Assessment.Views
{
    [QueryProperty(nameof(ActiveUserId), nameof(ActiveUserId))]
    public  class BaseContentPage : ContentPage
    {
        public int ActiveUserId { get; set; } = -999;
        protected readonly IDataRepository DataRepository;

        public BaseContentPage(IDataRepository dataRepository)
        {
            DataRepository = dataRepository;
        }

        // Override Content so it always goes inside a ScrollView
        public new View Content
        {
            get => base.Content is ScrollView sv ? sv.Content : base.Content;
            set => base.Content = new ScrollView { Content = value };
        }

        private bool _wrapped = false;

        protected override void OnChildAdded(Element child)
        {
            base.OnChildAdded(child);

            // Only wrap once, and only if it's not already a ScrollView
            if (!_wrapped && base.Content != null && base.Content is not ScrollView)
            {
                _wrapped = true;

                var originalContent = base.Content;
                base.Content = new ScrollView
                {
                    Content = (View)originalContent
                };
            }

            // Hide or disable the Shell back button
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                IsVisible = false,   
            });
        }

        public async Task OnError(string e)
        {
            await DisplayAlert("Error", e, "OK");
        }
        public async Task OnSuccess(string message)
        {
            await DisplayAlert("Success", message, "OK");
        }

        public async Task LogEvent(string message)
        {
            var eventMsg = new Models.EventLogModel
            {
                UserId = ActiveUserId,
                EventDescription = message,
                EventDateTime = DateTime.Now,
             };
            await DataRepository.AddOrUpdateEventLog(eventMsg);
        }

        protected async Task ShowReportPopupAsync(ContentView contentView)
        {
            var popup = new ReportPopupHelper(contentView);
            await Navigation.PushModalAsync(new ReportPage(contentView));

        }

    }
}
