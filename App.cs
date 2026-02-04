
using HBSADLibrary.BusinessLogic;
namespace HBSADConsole
{
    public class App
    {
        private readonly IUserRead _userread;
        public App(IUserRead userRead) {
            _userread=userRead;
        }

        public void Run(string[] args)
        {
            string lang = "en";

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower().StartsWith("lang="))
                {
                    lang = args[i].Substring(5);
                    break;
                }
            }

            

            //Console.WriteLine(message);
        }

    }
}
