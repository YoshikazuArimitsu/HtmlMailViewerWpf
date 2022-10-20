using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleApp;
using Wpf.Extensions.Hosting;

var builder = WpfApplication<App, MainWindow>.CreateBuilder(args);

// Configure dependency injection.
builder.Services.AddTransient<MainWindowViewModel>();

// Configure the settings.
builder.Services.Configure<AppSettings>(builder.Configuration);

// Build Host & Run App
var app = builder.Build();
app.Startup += (sender, eventArgs) =>
{

};

await app.RunAsync();