using System;
using SuperRolodex.Core.Repositories;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SuperRolodex
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            HeroesRepository.Instance.Initalize();

            MainPage = new NavigationPage(new MainPage());
        }
    }
}
