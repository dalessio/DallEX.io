using System.Configuration;
namespace Jojatekok.PoloniexAPI.Demo
{
    static class ApiKeys
    {
        // WARNING: In order not to expose the changes of this file to git, please
        //          run the following command every time you clone the repository:
        // git update-index --assume-unchanged "PoloniexApi.Net.Demo\ApiKeys.cs"
        internal static string PublicKey = ConfigurationManager.AppSettings.Get("PublicKey");
        internal static string PrivateKey = ConfigurationManager.AppSettings.Get("PrivateKey");
    }
}
