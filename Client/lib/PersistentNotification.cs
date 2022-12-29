using System.Diagnostics.CodeAnalysis;

namespace jackz_builder.Client.Notification
{
    static class Extensions
    {
        public static string Convert(this Style style)
        {
            return style.ToString().ToLower();
        }
        public static string Convert(this Position position)
        {
            switch (position)
            {
                case Notification.Position.TopLeft:
                    return "top-left";
                case Notification.Position.TopCenter:
                    return "top-center";
                case Notification.Position.TopRight:
                    return "top-right";
                case Notification.Position.BottomLeft:
                    return "bottom-left";
                case Notification.Position.BottomCenter:
                    return "bottom-center";
                case Notification.Position.BottomRight:
                    return "bottom-right";
                case Notification.Position.MiddleLeft:
                    return "middle-left";
                case Notification.Position.MiddleRight:
                    return "middle-right";
                default:
                    return null;
            }
        }
    }
    // public static class Style
    // {
    //     public const string Info = "info";
    //     public const string Error = "error";
    //     public const string Warning = "warning";
    //     public const string Success = "success";
    //     public const string Message = "message";
    //     
    //     public static ToString()
    // }

    public enum Style
    {
        Info,
        Error,
        Warning,
        Success,
        Message
    }

    public enum Position
    {
        TopLeft,
        TopCenter,
        TopRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
        MiddleLeft,
        MiddleRight
    }
    
    public class PersistentNotification
    {
        private string id;
        private dynamic lib;
        private NotificationOptions options;
        public bool IsActive { get; private set; }

        public string Title
        {
            get => options.title;
            set
            {
                options.title = value;
                Update(options);
            }
        }
        
        public string Message
        {
            get => options.message;
            set
            {
                options.message = value;
                Update(options);
            }
        }

        public Style Style
        {
            set
            {
                options.style = value.Convert();
                Update(options);
            }
        }
        
        public string StyleRaw
        {
            get => options.style;
            set
            {
                options.style = value;
                Update(options);
            }
        }

        public PersistentNotification(dynamic tNotify, string id)
        {
            this.id = id;
            this.lib = tNotify;
        }

        public void Start(NotificationOptions _options)
        {
            options = _options;
            IsActive = true;
            _set("start", options);
        }

        public void Update(NotificationOptions _options)
        {
            options = _options;
            _set("update", options);
        }

        public void Stop()
        {
            IsActive = false;
            _set("end", null);
        }

        private void _set(string step, NotificationOptions _options)
        {
            lib.Persist(new
            {
                id,
                step,
                options = _options
            });
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class NotificationOptions
    {

        public string style { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public bool sound { get; set; }
        public int duration { get; set; }
        public string position { get; set; }

        private string? _convertPosition(Position? position)
        {
            return position switch
            {
                Notification.Position.TopLeft => "top-left",
                Notification.Position.TopCenter => "top-center",
                Notification.Position.TopRight => "top-right",
                Notification.Position.BottomLeft => "bottom-left",
                Notification.Position.BottomCenter => "bottom-center",
                Notification.Position.BottomRight => "bottom-right",
                Notification.Position.MiddleLeft => "middle-left",
                Notification.Position.MiddleRight => "middle-right",
                _ => null
            };
        }
        
    }
}