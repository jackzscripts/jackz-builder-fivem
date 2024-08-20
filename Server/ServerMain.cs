using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using Debug = System.Diagnostics.Debug;

namespace test_project.Server
{
    public class ServerMain : BaseScript
    {
        public ServerMain()
        {
            Debug.WriteLine("Hi from test_project.Server!");
        }

        [Command("hello_server")]
        public void HelloServer()
        {
            Debug.WriteLine("Hello.");
        }
    }
}