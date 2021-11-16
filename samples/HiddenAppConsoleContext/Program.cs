using AppContextSampleApp;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsFormsLifetime<ExampleApplicationContext, MainForm>((startForm) =>
{ 
    return new ExampleApplicationContext(startForm);
});
var app = builder.Build();
app.Run();

namespace AppContextSampleApp
{
    public class ExampleApplicationContext : ApplicationContext
    {
        public ExampleApplicationContext(Form form)
        {
            MainForm = form;
        }
    }
}