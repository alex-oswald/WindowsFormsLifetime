using HiddenContextSampleApp;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsFormsLifetimeAppContext<HiddenContext, MainForm>((startForm) =>
{ 
    return new HiddenContext(startForm);
});
var app = builder.Build();
app.Run();

namespace HiddenContextSampleApp
{
    public class HiddenContext : ApplicationContext
    {
        public HiddenContext(Form form)
        {
            MainForm = form;
        }
    }
}