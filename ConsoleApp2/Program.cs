using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

public class Program
{
    private static string connectionString = "Server=localhost;Database=phone_book;Uid=root;Pwd=";

    public static async Task Main(string[] args)
    {
        IWebHost host = new WebHostBuilder()
            .UseKestrel()
            .ConfigureServices(services => services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>())
            .Configure(app =>
            {
                app.Run(async (context) =>
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        List<string> abonents = new List<string>();

                        MySqlCommand command = new MySqlCommand("SELECT * FROM abonents", connection);
                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string name = reader.GetString("name");
                                abonents.Add(name);
                            }
                        }

                        string response = $@"
                            <!DOCTYPE html>
                            <html>
                            <head>
                                <meta charset=""UTF-8"">
                            </head>
                            <body>
                            <ul>
                            {string.Join("", abonents.ConvertAll(abonent => $"<li>{abonent}</li>"))}
                            </ul>
                            </body>
                            </html>";

                        context.Response.ContentType = "text/html; charset=utf-8";
                        await context.Response.WriteAsync(response, Encoding.UTF8);
                    }
                });
            })
            .Build();

        await host.RunAsync();
    }
}
