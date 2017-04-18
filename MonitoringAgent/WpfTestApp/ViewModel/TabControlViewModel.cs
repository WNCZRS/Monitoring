using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfTestApp.ViewModel;

namespace WpfTestApp.ViewModel
{
    public class TabControlViewModel
    {
        public interface ITabViewModel
        {
        }

        public ITabViewModel SelectedTabViewModel { get; set; }

        public TabControlViewModel()
        {
            ObservableCollection<ITabViewModel> tabControlViewModels = new ObservableCollection<ITabViewModel>();
            //tabControlViewModels.Add(new ViewModelA { Header = "Tab A" });
            //tabControlViewModels.Add(new ViewModelB { Header = "Tab B" });
            //tabControlViewModels.Add(new ViewModelC { Header = "Tab C" });

            SelectedTabViewModel = tabControlViewModels[0];
        }

    }
}
