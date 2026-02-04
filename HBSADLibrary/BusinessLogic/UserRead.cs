using CsvHelper;
using HBSADLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HBSADLibrary.BusinessLogic
{

    public class UserRead : IUserRead
    {

     

        //private IConfiguration _configuration;
        private static IConfiguration _configuration;
        private static bool isEmailMissed = false;
        private readonly ILogger<UserRead> _log;
        static List<string> userDetails = new List<string>();
        //public static string[] userDetails;
        
        public UserRead( ILogger<UserRead> log)
        {
            //_configuration = iconfig;
            ConfigureAppSettings();
            _log = log;
            Logger.InitializeLogFile();

        }


        private static void ConfigureAppSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            _configuration = builder.Build();
        }

        public async Task UserFileReader()
        {
            string inputFile = _configuration.GetSection("Path").GetSection("inputFile").Value;
            string outputFile = _configuration.GetSection("Path").GetSection("outputFile").Value;
            string ldapPath = _configuration.GetSection("Path").GetSection("ldapPath").Value;
            string username = _configuration.GetSection("Path").GetSection("username").Value;
            string password = _configuration.GetSection("Path").GetSection("password").Value;
            string downloadfilePath = _configuration.GetSection("Path").GetSection("downloadFilePath").Value;

            Logger.LogInfo("File Reader Started : " + DateTime.Now.ToString());

            //string host = "147.154.3.134";
            //int port = 10213;
            //string sftpusername = "hbsdevsftp_ad_user";
            //string sftppassword = "5cc26c2b-DEV-AD-bf06@29d609f5402F";
            //string sourceDirectory = "/home/users/hbsdevsftp_ad_user/Outbound";
            //string destinationDirectory = "/home/users/hbsdevsftp_ad_user/Inbound";
            //string failedDirectory = "/home/users/hbsdevsftp_ad_user/Outbound/Failure";

            string host = _configuration.GetSection("SFTP").GetSection("host").Value.ToString();
            int port = Convert.ToInt32(_configuration.GetSection("SFTP").GetSection("port").Value);
            string sftpusername = _configuration.GetSection("SFTP").GetSection("sftpusername").Value.ToString();
            string sftppassword = _configuration.GetSection("SFTP").GetSection("sftppassword").Value.ToString();
            string sourceDirectory = _configuration.GetSection("SFTP").GetSection("sourceDirectory").Value;
            string destinationDirectory = _configuration.GetSection("SFTP").GetSection("destinationDirectory").Value;
            string failedDirectory = _configuration.GetSection("SFTP").GetSection("failedDirectory").Value;




            try
            {
                using (var client = new SftpClient(host, port, sftpusername, sftppassword))
                {
                    try
                    {
                        client.Connect();
                        Logger.LogInfo("Connected to SFTP.");

                        await ListAndProcessCsvFiles(client, sourceDirectory, destinationDirectory, ldapPath, failedDirectory);

                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Error connecting or processing files: {ex.Message}");

                    }
                    finally
                    {
                        if (client.IsConnected)
                        {
                            client.Disconnect();
                            Logger.LogInfo("Disconnected from SFTP server.");
                            Logger.LogInfo("File Reader Completed : " + DateTime.Now.ToString());
                        }
                    }

                }

             }
            catch (Exception ex)
            {
                Logger.LogError($"Error connecting or processing files: {ex.Message}");
                throw;
            }

        }
        public static async Task EmailSending(string fileName,string mailType)
        {
            try
            {

                string fromMail = _configuration.GetSection("SMTP").GetSection("fromMail").Value.ToString();
                string fromPassword = _configuration.GetSection("SMTP").GetSection("fromPassword").Value.ToString();
                Int32 Port = Convert.ToInt32(_configuration.GetSection("SMTP").GetSection("Port").Value);
                string toMail = _configuration.GetSection("SMTP").GetSection("toMailId").Value.ToString();
                string ccMail = _configuration.GetSection("SMTP").GetSection("ccMailId").Value.ToString();
                //string fromMail = "adpaccount@hexacorp.com";
                //string fromPassword = "Gate@432";
                MailMessage message = new MailMessage();
                message.From = new MailAddress(fromMail);
                
                string[] MutlMailId= toMail.Split(",");
                foreach(string toMailId in MutlMailId)
                {
                    //message.To.Add(new MailAddress("venkatesan.muthukrishnan@hexacorp.com"));
                    message.To.Add(new MailAddress(toMailId));
                }
                string[] ccMutlMailId = ccMail.Split(",");
                foreach (string ccMailId in ccMutlMailId)
                {
                    //message.To.Add(new MailAddress("venkatesan.muthukrishnan@hexacorp.com"));
                    message.CC.Add(new MailAddress(ccMailId));
                }
                var htmlString = "";
                if (mailType == "F")
                {
                    message.Subject = "ADP to AD integration Completed with Error";
                    htmlString = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\"> <html><head> <meta http-equiv=\"content-type\" content=\"text/html; charset=ISO-8859-1\"> </head> ";
                    htmlString = htmlString + "<body bgcolor=\"#ffffff\" text=\"#000000\"> The ADP-AD Integration Service's Updating process for the file named " + fileName + "  completed with error. <br><br> Please find the error record details <br><br>";
                    
                    
                    foreach (string userInfo in userDetails)
                    {
                        htmlString = htmlString + "<body bgcolor=\"#ffffff\" text=\"#000000\">  " + userInfo + "<br> <br>";
                    }

                    htmlString = htmlString + "Regards<br>HBS ADP Team<br></i></div></body></html>";
                }
                else if (mailType == "S")
                {
                    message.Subject = "ADP-AD integration Success";
                    htmlString = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\"> <html><head> <meta http-equiv=\"content-type\" content=\"text/html; charset=ISO-8859-1\"> </head> ";
                    htmlString = htmlString + "<body bgcolor=\"#ffffff\" text=\"#000000\"> The ADP-AD Integration Service's Updating process for the file named " + fileName + "  Successfully Updated. <br> <div class=\"moz-signature\"><i><br> <br>";
                    htmlString = htmlString + "Regards<br>HBS ADP Team<br></i></div></body></html>";
                }
                message.Body = htmlString;
                message.IsBodyHtml = true;

               

                SmtpClient smtpClient = new SmtpClient("smtp.office365.com");
                smtpClient.Port = 25; // SMTP port (587 is typically used for TLS/STARTTLS)
                smtpClient.EnableSsl = true; // Enable SSL/TLS
                smtpClient.Credentials = new NetworkCredential(fromMail, fromPassword);

                smtpClient.Send(message);
            }

            catch (Exception ex)
            {
                Logger.LogError($"Error Sending Email: {ex.Message}");

            }
        }
        public static async Task ListAndProcessCsvFiles(SftpClient client, string sourceDir, string destDir, string ldapPath, string failedDir )
        {
            try
            {
                var files = client.ListDirectory(sourceDir);

                foreach (var file in files)
                {
                    if (file.IsRegularFile && file.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Found CSV file: {file.Name}");
                        Logger.LogInfo($"Found CSV file: {file.Name}");

                        // Download and process the CSV file
                        string sourceFilePath = sourceDir + "/" + file.Name;
                        string destinationFilePath = destDir + "/" + file.Name;
                        string FileName = file.Name;

                        await DownloadCsvFile(client, sourceFilePath, destinationFilePath, ldapPath, failedDir);

                        // Move the file to another directory
                        //await MoveFile(client, sourceFilePath, destDir);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing or processing CSV files: {ex.Message}");
                Logger.LogError($"Error listing or processing CSV files: {ex.Message}");
                //throw; // Re-throw the exception to propagate it up the call stack
            }
        }

        public static async Task DownloadCsvFile(SftpClient client, string sourceFilePath, string localDestinationPath, string ldapPath, string failedDir)
        {
           
            try
            {
                

                using (var memoryStream = new MemoryStream())
                {
                    client.DownloadFile(sourceFilePath, memoryStream);
                    

                    // Reset position to the beginning of the stream before saving
                    memoryStream.Position = 0;

                    using var reader = new StreamReader(memoryStream);

                    //using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                    using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
                    {
                        int i = 0;
                        var records = csv.GetRecords<ADModel>().ToList();
                        Console.WriteLine("List of record", records);

                        foreach (var record in records)
                        {
                            Console.WriteLine(record.GivenName + " " + record.SurName);
                            Logger.LogInfo(record.GivenName + " " + record.SurName);
                            await ModifyUserProperies(record, ldapPath);

                            i++;
                        }
                    }

                    if(isEmailMissed == true)
                    {
                        string fileName = Path.GetFileName(sourceFilePath);
                        await ErrorMoveFile(client, sourceFilePath, failedDir);
                        //await EmailSending(fileName.ToString(), "F");
                    }                    

                }
                Console.WriteLine($"Downloaded file: {sourceFilePath}");
                Logger.LogInfo($"Downloaded file: {sourceFilePath}");
            }
            catch (Exception ex)
            {

                await ErrorMoveFile(client, sourceFilePath, failedDir);
                Console.WriteLine($"Error downloading file {sourceFilePath}: {ex.Message}");
                Logger.LogError($"Error downloading file {sourceFilePath}: {ex.Message}");
               // throw; // Re-throw the exception to propagate it up the call stack
            }
        }

        public static async Task ModifyUserProperies(ADModel users, string ldapPath)
        {
            //DirectoryEntry addUserFolder = new DirectoryEntry("LDAP://CN=Users,DC=hbs,DC=com");

            string username = _configuration.GetSection("Path").GetSection("username").Value;
            string password = _configuration.GetSection("Path").GetSection("password").Value;

            string domainControllerIp = _configuration.GetSection("Path").GetSection("domainControllerIp").Value;


            Logger.LogInfo($"UserName: {username} Password : {password} IpAdddress : {domainControllerIp}");


            try
            {
                //using (DirectoryEntry entry = new DirectoryEntry(ldapPath, username, password))
                using (DirectoryEntry entry = new DirectoryEntry($"LDAP://{domainControllerIp}", username, password))
                {
                    using (DirectorySearcher search = new DirectorySearcher(entry))
                    {
                        if (users.Mail != "")
                        {
                            search.Filter = $"(mail={users.Mail})"; // Search by email address
                            SearchResultCollection results = search.FindAll();

                            if (results.Count == 0)
                            {
                                Logger.LogError($"No user found with email:: {users.Mail.ToString()}");
                            }
                            else if (results.Count > 1)
                            {
                                Logger.LogError($"Multiple users found with email: {users.Mail}");
                            }
                            else
                            {
                                SearchResult result = results[0];

                                Logger.LogInfo($"result file: {result}");
                                
                                using (DirectoryEntry userEntry = result.GetDirectoryEntry())
                                {
                                    //Object obj = userEntry.NativeObject;
                                    // Update the properties
                                    string? employeeId = null;

                                    if (userEntry.Properties["employeeID"].Count > 0)
                                    {
                                        var rawValue = userEntry.Properties["employeeID"][0]; // safe
                                        employeeId = rawValue?.ToString();
                                    }

                                    string? givenName = null;

                                    if (userEntry.Properties["givenName"].Count > 0)
                                    {
                                        var rawValue = userEntry.Properties["givenName"][0]; // safe
                                        givenName = rawValue?.ToString();
                                    }

                                    Logger.LogInfo($"Before Update-AD Properties");
                                    Logger.LogInfo($"EmployeeID: {employeeId}");
                                    Logger.LogInfo($"givenName: {givenName}");


                                    object? accountExpireObj = null;

                                    if (userEntry.Properties["accountExpireObj"].Count > 0)
                                    {
                                        accountExpireObj = userEntry.Properties["accountExpireObj"][0]; // safe
                                    }

                                   // Logger.LogInfo($"AccountExp: {accountExpireObj}");

                                    if (accountExpireObj != null)
                                    {
                                        if (Marshal.IsComObject(accountExpireObj) && Marshal.GetIUnknownForObject(accountExpireObj) != IntPtr.Zero)
                                        {
                                            DateTime expiresDate = DateTime.FromFileTime(Convert.ToInt64(accountExpireObj));
                                            Logger.LogInfo($"AccountExp: {expiresDate}");
                                            Logger.LogInfo($"ExpiresDate: {expiresDate}");
                                        }
                                        else
                                        {
                                            if (users.UID.Length > 0 && users.UID != null)
                                            {
                                                userEntry.Properties["EmployeeID"].Value = users.UID;
                                            }
                                            Logger.LogInfo($"After Update AD Properties ");
                                            Logger.LogInfo($"EmployeeID: {employeeId}");
                                            Logger.LogInfo($"UID: {users.UID}");
                                            Logger.LogInfo($"givenName: {givenName}");

                                            // Save changes
                                            userEntry.CommitChanges();

                                            Logger.LogInfo($"User properties updated successfully for email: {users.Mail}");
                                        }
                                    }
                                    else
                                    {
                                        Logger.LogInfo("ExpiresDate object is null");
                                    }
                                }
                            }
                        }
                        else
                        {
                            isEmailMissed = true;
                            userDetails.Add(users.EmployeeID + " | "+ users.GivenName +" "+ users.SurName);
                            Logger.LogError($"Mail Id for the user {users.GivenName} is not found");
                        }
                    }
                }
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                if (ex.ErrorCode == -2147016694)
                {
                    Logger.LogError($"Error On Update function Can't retrieve LDAP NativeObject property : {ex.Message}");
                }
                else
                {
                    Logger.LogError($"Error On Update function : {ex.Message}");
                }

            }
        }
        //public static async Task MoveFile(SftpClient client, string sourceFilePath, string destinationDir)
        //{
        //    try
        //    {
        //        string fileName = Path.GetFileName(sourceFilePath);
        //        string destinationPath = destinationDir + "/" + "success_" + DateTime.Now.ToString("MM-dd-yyyy_hh-mm-ss-tt") + "_" + fileName;

        //        // Move the file
        //        client.RenameFile(sourceFilePath, destinationPath);
        //        //client.DeleteFile(sourceFilePath);
        //        await EmailSending(fileName.ToString(), "S");
        //        Console.WriteLine($"Moved file {fileName} to {destinationPath}");
        //        Logger.LogInfo($"Moved file {fileName} to {destinationPath}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error moving file {sourceFilePath} to {destinationDir}: {ex.Message}");
        //        Logger.LogError($"Error moving file {sourceFilePath} to {destinationDir}: {ex.Message}");
        //        throw; // Re-throw the exception to propagate it up the call stack
        //    }
        //}
        public static async Task ErrorMoveFile(SftpClient client, string sourceFilePath, string destinationDir)
        {
            try
            {
                string fileName = Path.GetFileName(sourceFilePath);
                string destinationPath = destinationDir + "/" + DateTime.Now.ToString("MM-dd-yyyy_hh-mm-ss-tt") + "_" + fileName;

                // Move the file
                client.RenameFile(sourceFilePath, destinationPath);
                //client.DeleteFile(sourceFilePath);
                await EmailSending(fileName.ToString(),"F");

                Console.WriteLine($"Moved file {fileName} to {destinationPath}");
                Logger.LogInfo($"Moved file {fileName} to {destinationPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving file {sourceFilePath} to {destinationDir}: {ex.Message}");
                Logger.LogError($"Error moving file {sourceFilePath} to {destinationDir}: {ex.Message}");
                throw; // Re-throw the exception to propagate it up the call stack
            }
        }

    }
}
