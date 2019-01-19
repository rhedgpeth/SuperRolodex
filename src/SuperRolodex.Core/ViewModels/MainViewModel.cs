using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using SuperRolodex.Core.Models;
using SuperRolodex.Core.Repositories;

namespace SuperRolodex.Core.ViewModels
{
    public class MainViewModel : BaseNotify
    {
        ObservableCollection<Hero> _heroes;
        public ObservableCollection<Hero> Heroes
        {
            get => _heroes;
            set => SetPropertyChanged(ref _heroes, value);
        }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetPropertyChanged(ref _isBusy, value);
        }

        string SearchText { get; set; }

        bool SortAscending { get; set; } = true;

        string _sortText = "Sort DESC";
        public string SortText
        {
            get => _sortText;
            set => SetPropertyChanged(ref _sortText, value);
        }

        ICommand _sortCommand;
        public ICommand SortCommand
        {
            get
            {
                if (_sortCommand == null)
                {
                    _sortCommand = new Command(async () => await Sort());
                }

                return _sortCommand;
            }
        }

        public MainViewModel() => Init();

        async void Init() => await LoadAll();

        async Task LoadAll() => Heroes = new ObservableCollection<Hero>(await Task.Run(() 
                                    => { return HeroesRepository.Instance.GetAll(SortAscending); }));

        public async Task Search(string searchText)
        {
            SearchText = searchText;

            Heroes = new ObservableCollection<Hero>(await Task.Run(() =>
            { 
                return HeroesRepository.Instance.Search(SearchText, SortAscending);
            }));
        }

        Task Sort()
        {
            SortAscending = !SortAscending;

            SortText = SortAscending ? "Sort DESC" : "Sort ASC";

            if (!string.IsNullOrEmpty(SearchText))
            {
                return Search(SearchText);
            }
            else
            {
                return LoadAll();
            }
        }
    }
}
