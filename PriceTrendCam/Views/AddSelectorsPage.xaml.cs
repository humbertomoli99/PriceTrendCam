using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

// To learn more about WebView2, see https://docs.microsoft.com/microsoft-edge/webview2/.
public sealed partial class AddSelectorsPage : Page
{
    public AddSelectorsViewModel ViewModel
    {
        get;
    }

    public AddSelectorsPage()
    {
        ViewModel = App.GetService<AddSelectorsViewModel>();
        InitializeComponent();

        ViewModel.WebViewService.Initialize(WebView);
    }

    private void SaveButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }

    private void DataPreviewButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }

    private void ElementPreviewButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }

    private void SelectButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }

    private void CancelButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }
    private void WebView_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _ = OnPointerPressed(sender, e);
    }
    private async Task OnPointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        int x = (int)e.GetCurrentPoint(WebView).Position.X;
        int y = (int)e.GetCurrentPoint(WebView).Position.Y;


        string script4 = @"
function getCssSelector(el) {
  var path = [];
  while (el.nodeType === Node.ELEMENT_NODE) {
    var selector = el.nodeName.toLowerCase();
    if (el.id) {
      selector += '#' + el.id;
      path.unshift(selector);
      break;
    } else {
      var siblingSelector = '';
      var siblingIndex = 1;
      var sibling = el.previousSibling;
      while (sibling) {
        if (sibling.nodeType === Node.ELEMENT_NODE && sibling.nodeName.toLowerCase() === selector) {
          siblingIndex++;
        }
        sibling = sibling.previousSibling;
      }
      if (siblingIndex > 1) {
        siblingSelector = ':nth-of-type(' + siblingIndex + ')';
      }
      selector += siblingSelector;
      path.unshift(selector);
    }
    el = el.parentNode;
  }
  return path.join(' > ');
}
getCssSelector(document.elementFromPoint(" + x + ", " + y + "));";
        string script = @"
                function getElementCssSelector(el) {
                    if (!(el instanceof Element))
                        return;
                    var path = [];
                    while (el.nodeType === Node.ELEMENT_NODE) {
                        var selector = el.nodeName.toLowerCase();
                        if (el.id) {
                            selector += '#' + el.id;
                            path.unshift(selector);
                            break;
                        } else {
                            var sib = el, nth = 1;
                            while (sib = sib.previousElementSibling) {
                                if (sib.nodeName.toLowerCase() == selector)
                                    nth++;
                            }
                            if (nth != 1)
                                selector += "":nth"";
                        }
                        path.unshift(selector);
                        el = el.parentNode;
                    }
                    return path.join("">"");
                }
                getElementCssSelector(document.elementFromPoint(" + x + ", " + y + "));";

        string script2 = @"(function () { return window.__PRELOADED_STATE__ })()";
        string script3 = @"document.elementFromPoint(" + x + ", " + y + ").style.border='solid red 1px'";
        string cssSelector = await WebView.ExecuteScriptAsync(script);
        string cssSelector2 = await WebView.ExecuteScriptAsync(script2);
        _ = await WebView.ExecuteScriptAsync(script3);
        string cssSelector4 = await WebView.ExecuteScriptAsync(script4);

        SelectorTextBox.Text = cssSelector4;
        System.Diagnostics.Debug.WriteLine("#1: " + cssSelector);
        System.Diagnostics.Debug.WriteLine("#2: " + cssSelector2);
    }
}
