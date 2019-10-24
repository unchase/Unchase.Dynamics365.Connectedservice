using System.Windows;
using System.Windows.Controls;

namespace Unchase.Dynamics365.ConnectedService.Common
{
    internal static class ExtensionsHelper
    {
        internal static void ChangeStackPanelVisibility(this StackPanel stackPanel)
        {
            if (stackPanel.Visibility == Visibility.Collapsed)
                stackPanel.Visibility = Visibility.Visible;
            else if (stackPanel.Visibility == Visibility.Visible)
                stackPanel.Visibility = Visibility.Collapsed;
        }
    }
}
