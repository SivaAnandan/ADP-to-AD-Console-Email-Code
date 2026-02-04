
using HBSADLibrary.BusinessLogic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;



using IHost host = CreateHostBuilder(args).Build();
using var scope = host.Services.CreateScope();

//var configuration = new ConfigurationBuilder()
//                      .AddJsonFile("appsettings.json")
//                      .Build();



try
{
    var app = host.Services.GetRequiredService<IUserRead>();

    app.UserFileReader();


}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}


static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureServices((_, services) =>
        {
            services.AddSingleton<IUserRead, UserRead>();
            services.AddLogging(builder=>builder.AddConsole());
            //services.AddSingleton<HBSADConsole.App>();
        });
        
}


