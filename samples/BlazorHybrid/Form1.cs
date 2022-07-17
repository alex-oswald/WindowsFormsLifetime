using Microsoft.AspNetCore.Components.WebView.WindowsForms;

namespace BlazorHybrid
{
    public partial class Form1 : Form
    {
        public Form1(IServiceProvider sp)
        {
            InitializeComponent();

            blazorWebView1.HostPage = "wwwroot\\index.html";
            blazorWebView1.Services = sp;
            blazorWebView1.RootComponents.Add<MainLayout>("#app");
        }
    }
}