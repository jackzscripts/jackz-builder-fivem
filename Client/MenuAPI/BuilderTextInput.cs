using CitizenFX.Core.Native;
using ScaleformUI.Menu;

namespace test_project.Client.MenuAPI
{
    public class BuilderTextInput : BuilderMenuItem
    {
        public delegate void TextInputEvent(object sender, string text);
        public string Content { get; set; }
        
        public BuilderTextInput(string title, string prompt = null, string defaultValue = "") : base(title)
        {
            Content = defaultValue;
            Item.SetRightLabel("Text");
            Item.Activated += async (sender, item) =>
            {
                var promptText = string.IsNullOrEmpty(prompt) ? title : prompt;
                Content = await ClientMain.Instance.PromptForInput(promptText);
                OnTextChanged?.Invoke(sender, Content);
            };
        }
        
        public event TextInputEvent OnTextChanged;
    }
}