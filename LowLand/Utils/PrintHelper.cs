using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LowLand;
using Microsoft.UI.Xaml;

public class PrintHelper
{
    private List<UIElement> printElements = new();

    public void AddFrameworkElementToPrint(UIElement element)
    {
        printElements.Clear();
        printElements.Add(element);
    }

    public async Task ShowPrintUIAsync(string jobTitle)
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        await Windows.Graphics.Printing.PrintManagerInterop.ShowPrintUIForWindowAsync(hwnd);
    }
}
