using HBSADLibrary.Models;

namespace HBSADLibrary.BusinessLogic
{
    public interface IUserRead
    {
        //List<CustomSettings> getPath(string key);
        Task UserFileReader();
    }
}