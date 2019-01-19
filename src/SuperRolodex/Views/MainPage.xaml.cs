using SuperRolodex.Core.ViewModels;
using Xamarin.Forms;

namespace SuperRolodex
{
    public partial class MainPage : ContentPage
    {
        MainViewModel ViewModel { get; set; }

        public MainPage()
        {
            InitializeComponent();

            BindingContext = ViewModel = new MainViewModel();
        }

        async void Handle_TextChanged(object sender, TextChangedEventArgs e)
        {
            await ViewModel.Search(e.NewTextValue);
        }
    }
}
