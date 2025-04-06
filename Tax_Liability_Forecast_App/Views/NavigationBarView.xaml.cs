using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tax_Liability_Forecast_App.ViewModels;

namespace Tax_Liability_Forecast_App.Views
{
    /// <summary>
    /// Interaction logic for NavigationBarView.xaml
    /// </summary>
    public partial class NavigationBarView : UserControl
    {
        public NavigationBarView()
        {
            InitializeComponent();

            DataContextChanged += (s, e) =>
            {
                if (e.NewValue is NavigationBarViewModel viewModel)
                {
                    viewModel.PropertyChanged += ViewModelPropertyChanged;
                }
            };
        }

        private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(NavigationBarViewModel.IsExpanded))
            {
                var viewModel = (NavigationBarViewModel)sender;
                AnimateUI(viewModel.IsExpanded);
            }
        }

        private void AnimateUI(bool isExpanded)
        {
            var duration = TimeSpan.FromMilliseconds(500);
            var easing = new CubicEase { EasingMode = EasingMode.EaseOut };

            var widthAnimation = new DoubleAnimation
            {
                To = isExpanded ? 200 : 50,
                Duration = duration,
                EasingFunction = easing
            };
            NavigationBar.BeginAnimation(WidthProperty, widthAnimation);

            var rotationAnimation = new DoubleAnimation
            {
                To = isExpanded ? 180 : 0,
                Duration = duration,
                EasingFunction = easing
            };
            ArrowRotation.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);
        }
    }
}
