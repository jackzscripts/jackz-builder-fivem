using System;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using ScaleformUI.Menu;
using test_project.Client.Menu;
using static CitizenFX.Core.Native.API;

namespace test_project.Client
{
    public class ClientMain : BaseScript
    {
        private Action NextTick;
        public static ClientMain Instance { get; private set; }
        public static Builder Builder { get; private set; }
        public static PointF MenuPosition { get; set; }

        private MainMenu menu;
        
        public ClientMain()
        {
            Instance = this;
            // TODO: temp
            Builder = new Builder();
            Debug.WriteLine($"Loaded ClientMain v{Version} - {DateTime.Now}");
            DrawRect(0.5f, 0.5f, 0.5f, 0.5f, 255, 255, 255, 150);
            
            
            menu = new MainMenu();
            EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
        }

        [Command("builder")]
        public void OpenMenu()
        {
            menu.Visible = true;
        }

        public static string Version
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
                return attributes.Length == 0 ?
                    "" :
                    ((AssemblyInformationalVersionAttribute)attributes[0]).InformationalVersion;
            }
        }

        #if DEBUG
            public static readonly string ReleaseChannel = "Development";
        #else
            public static readonly string ReleaseChannel = "Release";
        #endif

        public void RunNextTick(Action cb)
        {
            if (NextTick != null)
            {
                throw new Exception("A function has already been scheduled for next tick.");
            }
            NextTick = cb;
        }

        public void OnResourceStop(string resourceName)
        {
            if (resourceName == GetCurrentResourceName())
            {
                Builder?.Reset();
            }
        }

        private TaskCompletionSource<string> _promptTaskSource;
        public Task<string> PromptForInput(string prompt)
        {
            _promptTaskSource = new TaskCompletionSource<string>();
            API.DisplayOnscreenKeyboard(1, "FMMC_MPM_NA", "", "", "", "", "", 30);
            return _promptTaskSource.Task;
        }

        [Tick]
        public Task OnTick()
        {
            if (_promptTaskSource != null)
            {
                var state = API.UpdateOnscreenKeyboard();
                if (state == 1)
                {

                    string result = API.GetOnscreenKeyboardResult();
                    if (result != null)
                    {
                        _promptTaskSource.SetResult(result);
                    }
                } else if (state == 2)
                {
                    _promptTaskSource.SetCanceled();
                } else if (state == -1)
                {
                    _promptTaskSource.SetException(new Exception("Not active"));
                }

            }
            if (NextTick != null)
            {
                NextTick();
                NextTick = null;
            }
            Builder?.OnTick();
            return Task.FromResult(0);
        }
    }
}