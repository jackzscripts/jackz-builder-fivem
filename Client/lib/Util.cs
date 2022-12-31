using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using MenuAPI;
using static CitizenFX.Core.Native.API;

namespace jackz_builder.Client.lib
{
    public class Color
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public int A { get; set; }
    
        public static readonly Color White = new Color(255, 255, 255, 255);
        public static readonly Color Black = new Color(0, 0, 0, 255);
        public static readonly Color Red = new Color(255, 0, 0, 255);
        public static readonly Color Green = new Color(0, 255, 0, 255);
        public static readonly Color Blue = new Color(0, 0, 255, 255);
        public static readonly Color Aqua = new Color(5, 195, 221, 255);
        public static readonly Color Orange = new Color(255, 163, 0, 255);
    
        public Color(int r, int g, int b, int a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }
    public enum WarningType
    {
        NONE = 0,
        SELECT = 1,
        OK = 2,
        YES = 4,
        BACK = 8,
        BACK_SELECT = 9,
        BACK_OK = 10,
        BACK_YES = 12,
        CANCEL = 16,
        CANCEL_SELECT = 17,
        CANCEL_OK = 18,
        CANCEL_YES = 20,
        NO = 32,
        NO_SELECT = 33,
        NO_OK = 34,
        YES_NO = 36,
        RETRY = 64,
        RETRY_SELECT = 65,
        RETRY_OK = 66,
        RETRY_YES = 68,
        RETRY_BACK = 72,
        RETRY_BACK_SELECT = 73,
        RETRY_BACK_OK = 74,
        RETRY_BACK_YES = 76,
        RETRY_CANCEL = 80,
        RETRY_CANCEL_SELECT = 81,
        RETRY_CANCEL_OK = 82,
        RETRY_CANCEL_YES = 84,
        SKIP = 256,
        SKIP_SELECT = 257,
        SKIP_OK = 258,
        SKIP_YES = 260,
        SKIP_BACK = 264,
        SKIP_BACK_SELECT = 265,
        SKIP_BACK_OK = 266,
        SKIP_BACK_YES = 268,
        SKIP_CANCEL = 272,
        SKIP_CANCEL_SELECT = 273,
        SKIP_CANCEL_OK = 274,
        SKIP_CANCEL_YES = 276,
        CONTINUE = 16384,
        BACK_CONTINUE = 16392,
        CANCEL_CONTINUE = 16400,
        LOADING_SPINNER = 134217728,
        SELECT_LOADING_SPINNER = 134217729,
        OK_LOADING_SPINNER = 134217730,
        YES_LOADING_SPINNER = 134217732,
        BACK_LOADING_SPINNER = 134217736,
        BACK_SELECT_LOADING_SPINNER = 134217737,
        BACK_OK_LOADING_SPINNER = 134217738,
        BACK_YES_LOADING_SPINNER = 134217740,
        CANCEL_LOADING_SPINNER = 134217744,
        CANCEL_SELECT_LOADING_SPINNER = 134217745,
        CANCEL_OK_LOADING_SPINNER = 134217746,
        CANCEL_YES_LOADING_SPINNER = 134217748
    }
    public static class Util
    {
        private const int MaxRequestWaitTime = 20000;
        private const int RequestModelDelayMs = 15;
        private const int NotificationDuration = 5000;

        public static async Task<bool> ShowConfirmDialog(string title, string subtitle, WarningType type = WarningType.CANCEL_YES)
        {
            bool? result = null;
            while (result == null)
            {
                DrawText(new Vector2(0.4f, 04f), "Waiting for input", 0.6f, Color.White);
                
                AddTextEntry("confirm_title", title);
                AddTextEntry("confirm_subtitle", subtitle);
                int a = 0;
                API.SetWarningMessage("confirm_title", (int)type, "confirm_subtitle", false, 0, ref a, ref a, false, 0);
                DisableControlAction(2, (int) Control.FrontendAccept, true);
                DisableControlAction(2, (int) Control.FrontendCancel, true);
                DisableControlAction(2, 203, true);
                if (IsDisabledControlPressed(2, (int)Control.FrontendAccept))
                {
                    Debug.WriteLine("Confirm: OK");
                    result = true;
                } else if(IsDisabledControlPressed(2, 203)) {
                    Debug.WriteLine("Confirm: Alt");
                } else if(IsDisabledControlPressed(2, (int) Control.FrontendCancel))
                {
                    Debug.WriteLine("Confirm: Cancel");
                    result = false;
                }

                await BaseScript.Delay(0);
            }

            return result.Value;
        }

        //Modified from https://forum.cfx.re/t/how-to-supas-helper-scripts/41100
        public static void HighlightPosition(Vector3 pos, float size = 0.01f, Color color = null)
        {
            if (color == null) color = new Color(255, 0, 0, 200);
            SetDrawOrigin(pos.X, pos.Y, pos.Z, 0);
            RequestStreamedTextureDict("helicopterhud", false);
            DrawSprite("helicopterhud", "hud_corner", new Vector2(-size, -size), new Vector2(0.006f, 0.006f), 0f, color);
            DrawSprite("helicopterhud", "hud_corner", new Vector2(size, -size), new Vector2(0.006f, 0.006f), 90f, color);
            DrawSprite("helicopterhud", "hud_corner", new Vector2(-size, size), new Vector2(0.006f, 0.006f), 270f, color);
            DrawSprite("helicopterhud", "hud_corner", new Vector2(size, size), new Vector2(0.006f, 0.006f), 180f, color);
            ClearDrawOrigin();
        }

        public static void DrawSprite(string textureDict, string textureName, Vector2 screenPos, Vector2 size,
            float heading, Color color)
        {
            API.DrawSprite(textureDict, textureName, screenPos.X, screenPos.Y, size.X, size.Y, heading, color.R, color.G, color.B, color.A);
        }
            
        public static Blip CreateBlipForEntity(Entity entity, BlipSprite? sprite = null, string name = null)
        {
            var blip = new Blip(AddBlipForEntity(entity.Handle));
            if (sprite != null)
            {
                blip.Sprite = sprite.Value;
            }

            if (name != null)
            {
                API.BeginTextCommandSetBlipName("STRING");
                API.AddTextComponentString(name);
                API.EndTextCommandSetBlipName(blip.Handle);
            }
            return blip;
        }
        public static Menu CreateSubMenu(Menu parent, string subMenuName, string subMenuDescription = null)
        {
            var submenu = new Menu(parent.MenuTitle, subMenuName);
            MenuController.AddSubmenu(parent, submenu);
            var entry = new MenuItem(subMenuName, subMenuDescription);
            MenuController.BindMenuItem(parent, submenu, entry);
            parent.AddMenuItem(entry);
            return submenu;
        }
        
        // Taken from vMenu
        public static async Task<string?> GetUserInput(string windowTitle, string defaultText, int maxInputLength)
        {
            // Create the window title string.
            var spacer = "\t";
            AddTextEntry($"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE",
                $"{windowTitle ?? "Enter"}:{spacer}(MAX {maxInputLength} Characters)");

            // Display the input box.
            DisplayOnscreenKeyboard(1, $"{GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", "", defaultText ?? "", "",
                "", "", maxInputLength);
            await BaseScript.Delay(0);
            // Wait for a result.
            while (true)
            {
                int keyboardStatus = UpdateOnscreenKeyboard();

                switch (keyboardStatus)
                {
                    case 3: // not displaying input field anymore somehow
                    case 2: // cancelled
                        return null;
                    case 1: // finished editing
                        return GetOnscreenKeyboardResult();
                    default:
                        await BaseScript.Delay(0);
                        break;
                }
            }
        }

        /// <summary>
        /// Converts from PascalCase to "Pascal Case", preserving abbreviations like "MyWPN" -> "My WPN"
        /// </summary>
        /// <param name="str">The PascalCaseVariable</param>
        /// <returns>Split up pascal case</returns>
        public static string SplitPascalCase(string str)
        {
            List<char> newStr = new List<char>(str.Length + 3);
            newStr.Add(str[0]);
            bool isLastUpper = false;
            for(int i = 1; i < str.Length; i++)
            {
                if(char.IsUpper(str[i]))
                {
                    if (isLastUpper)
                    {
                        newStr.Add(str[i]);
                    }
                    else
                    {
                        isLastUpper = true;
                        newStr.Add(' ');
                        newStr.Add(str[i]);
                    }
                }
                else
                {
                    newStr.Add(str[i]);
                    isLastUpper = false;
                }
            }
            return new string(newStr.ToArray());
        }
        
        /// <summary>
        /// Converts "EnumName" to "Enum Name" by calling <see cref="SplitPascalCase"/> with the enum.ToString()
        /// </summary>
        /// <param name="en">The enum</param>
        /// <returns></returns>
        public static string EnumToDisplay(Enum en)
        {
            return SplitPascalCase(en.ToString());
        }
        
        public static Vehicle getVehicle(bool includeLast = true)
        {
            var vehicle = Game.PlayerPed.CurrentVehicle;
            if (vehicle == null && includeLast) vehicle = Game.PlayerPed.LastVehicle;
            if (vehicle == null || (Game.PlayerPed.LastVehicle == vehicle && vehicle.Driver != Game.PlayerPed))
            {
                BuilderMain.TNotify.Alert(new
                {
                    style = "error",
                    message = "You are not in a vehicle",
                    duration = NotificationDuration
                });
                return null;
            } else if (vehicle.Driver != Game.PlayerPed)
            {
                BuilderMain.TNotify.Alert(new
                {
                    style = "error",
                    message = "You must be the driver of the vehicle",
                    duration = NotificationDuration
                });
                return null;
            }

            return vehicle;
        }

        public static void Alert(string message, string title = null, string style = "error", float duration = NotificationDuration, string position = null)
        {
            BuilderMain.TNotify.Custom(new
            {
                style,
                title,
                message,
                duration,
                position
            });
        }

        public static void Chat(string message, Color color = null)
        {
            BaseScript.TriggerEvent("chat:addMessage", new
            {
                color = color != null ? new[] {color.R, color.B, color.G} : null,
                args = new[] { message }
            });

        }
        
        public static void Chat(string[] messages, Color color = null)
        {
            BaseScript.TriggerEvent("chat:addMessage", new
            {
                color = color != null ? new[] {color.R, color.B, color.G} : null,
                multiline = true,
                args = messages
            });

        }

        public static async Task RequestModel(uint model)
        {
            if (!IsModelValid(model)) throw new Exception($"Model {model} is invalid");
            API.RequestModel(model);
            int time = 0;
            while (!API.HasModelLoaded(model))
            {
                await BaseScript.Delay(RequestModelDelayMs);
                time += RequestModelDelayMs;
                if (time >= MaxRequestWaitTime)
                {
                    throw new Exception($"Timed out requesting model {model}: Took {time} milliseconds");
                }
            }
        }

        public static void DrawLine(Vector3 from, Vector3 to, Color color)
        {
            API.DrawLine(from.X, from.Y, from.Z, to.X, to.Y,
                to.Z, color.R, color.G, color.B, color.A); 
        }

        public static float Vdist(Vector3 from, Vector3 to, bool squareRoot = false)
        {
            return squareRoot
                ? API.Vdist(from.X, from.Y, from.Z, to.X, to.Y, to.Z)
                : API.Vdist2(from.X, from.Y, from.Z, to.X, to.Y, to.Z);
        }

        public static Vector2 GetScreenCoords(Vector3 worldPos)
        {
            Vector2 outVec = Vector2.Zero;
            API.GetScreenCoordFromWorldCoord(worldPos.X, worldPos.Y, worldPos.Z, ref outVec.X, ref outVec.Y);
            return outVec;
        }

        public static void DrawSphere(Vector3 pos, float radius, Color color)
        {
            API.DrawSphere(pos.X, pos.Y, pos.Z, radius, color.R, color.G, color.B, color.A / 255f);
        }

        public static void DrawSphere(Vector3 pos, float radius = 1f)
        {
            DrawSphere(pos, radius, Color.White);
        }

        public static void DrawBox(Vector3 cornerA, Vector3 cornerB, Color color)
        {
            API.DrawBox(cornerA.X, cornerA.Y, cornerA.Z, cornerB.X, cornerB.Y, cornerB.Z, color.R, color.G, color.B, color.A);
        }

        public static void DrawRect(Vector2 coords, Vector2 size, Color color)
        {
            API.DrawRect(coords.X, coords.Y, size.X, size.Y, color.R, color.B, color.G, color.A);
        }

        /// <summary>
        /// Draws text on the UI
        /// </summary>
        /// <param name="pos">The X and Y position to render at. 0, 0 is top left</param>
        /// <param name="content">The text content</param>
        /// <param name="scale">The font scale of the text</param>
        /// <param name="color">The color of the text</param>
        public static void DrawText(Vector2 pos, string content, float scale, Color color)
        {
            // var screenPos = GetScreenCoords(worldPos);
            ClearDrawOrigin();
            AddText(pos, content, scale, color);
        }

        /// <summary>
        /// Draws text on screen, ontop of the current draw origin
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="content"></param>
        /// <param name="scale"></param>
        /// <param name="color"></param>
        public static void AddText(Vector2 pos, string content, float scale, Color color)
        {
            SetTextScale(0.0f, scale);
            SetTextDropshadow(1, 0, 0, 0,0);
            if (color == null) color = Color.White;
            SetTextColour(color.R, color.G, color.B, color.A);
            SetTextEntry("STRING");
            AddTextComponentString(content);
            API.DrawText(pos.X, pos.Y);
        }

        /// <summary>
        /// Draws text using SetDrawOrigin() to be 3d at a world coordinate. Only 32 are allowed per frame, any more and some elements may flicker
        /// </summary>
        /// <param name="worldPos">The position in the world</param>
        /// <param name="content">The text content</param>
        /// <param name="scale">The font scale of the text</param>
        /// <param name="color">The text color</param>
        /// <param name="offset">Offset of the UI position when drawing</param>
        public static void DrawText3D(Vector3 worldPos, string content, float scale, Color color, Vector2 offset)
        {
            SetDrawOrigin(worldPos.X, worldPos.Y, worldPos.Z, 0);
            AddText(offset, content, scale, color);
            API.ClearDrawOrigin();
        }
        
        public static void DrawText3D(Vector3 worldPos, IEnumerable<string> contents, float scale, Color color, Vector2 offset)
        {
            SetDrawOrigin(worldPos.X, worldPos.Y, worldPos.Z, 0);
            foreach (var line in contents)
            {
                AddText(offset, line, scale, color);
                offset.Y += scale;
            }
            API.ClearDrawOrigin();
        }

        public static void DrawText3D(Vector3 worldPos, string content, float scale, Color color = null)
        {
            DrawText3D(worldPos, content, scale, color, Vector2.Zero);
        }

        public static void DrawText3D(Entity entity, string content, float scale, Color color, Vector2 offset)
        {
            DrawText3D(entity.Position, content, scale, color, offset);
        }

        public static void DrawText3D(Entity entity, string content, float scale, Color color = null)
        {
            DrawText3D(entity.Position, content, scale, color, Vector2.Zero);
        }

        /// <summary>
        /// Draws 3D text similar to <see cref="DrawText3D" /> but without the limitation of the amount of origins. Will shift around as it's slower than SetDrawOrigin
        /// </summary>
        /// <param name="worldPos"></param>
        /// <param name="content"></param>
        /// <param name="scale"></param>
        /// <param name="color"></param>
        /// <param name="offset"></param>
        public static void DrawText3D2(Vector3 worldPos, string content, float scale, Color color, Vector2 offset)
        {
            var screenPos = GetScreenCoords(worldPos) + offset;
            ClearDrawOrigin();
            AddText(screenPos, content, scale, color);
        }

        public static void DrawText3D2(Vector3 worldPos, string content, float scale, Color color = null)
        {
            DrawText3D2(worldPos, content, scale, color, Vector2.Zero);
        }

        public static void DrawText3D2(Entity entity, string content, float scale, Color color, Vector2 offset)
        {
            DrawText3D2(entity.Position, content, scale, color, offset);
        }

        public static void DrawText3D2(Entity entity, string content, float scale, Color color = null)
        {
            DrawText3D2(entity.Position, content, scale, color, Vector2.Zero);
        }

        /// <summary>
        /// Draws a marker, note that the Y position will be offset by +2f 
        /// </summary>
        public static void DrawMarker(int type, Vector3 pos, Vector3 dir, Vector3 rot, Vector3 scale, Color color, bool bobUpAndDown, bool faceCamera, bool rotate, bool drawOnEnts, string textureDict, string textureName)
        {
            API.DrawMarker(type, pos.X, pos.Y,
                pos.Z + 2f, dir.X, dir.Y, dir.Z, rot.X, rot.Y, rot.Z, scale.X, scale.Y, scale.Z, color.R, color.G, color.B, color.A, bobUpAndDown, faceCamera, 2, rotate, textureDict,
                textureName,
                drawOnEnts);
        }

        public static void DrawMarker(int type, Vector3 pos, Color color, bool bobUpAndDown = false, bool faceCamera = false, bool rotate = false, bool drawOnEnts = false)
        {
            DrawMarker(type, pos, Vector3.Zero, Vector3.Zero, Vector3.One, color, bobUpAndDown, faceCamera, rotate, drawOnEnts, null, null);
        }

        public static void ShowBusySpinner(string text)
        {
            API.BeginTextCommandBusyspinnerOn("STRING");
            API.AddTextComponentString(text);
            API.EndTextCommandBusyspinnerOn(2);
        }

        public static void HideBusySpinner()
        {
            API.BusyspinnerOff();
        }
    }
    
}

namespace ExtensionMethods
{

    public static class Vector3Ext
    {
        public static Vector3 Clone(this Vector3 vec)
        {
            return new Vector3(vec.X, vec.Y, vec.Z);
        }
        
        [Flags]
        public enum CheckOptions
        {
            CheckUnk1 = 1,
            CheckPeds,
            CheckVehicles,
            CheckUnk2,
            CheckUnk3
        }

        public static bool IsPositionOccupied(this Vector3 vec, float range = 1.0f, CheckOptions checkOptions = 0, int ignoreEntity = 0)
        {
            return API.IsPositionOccupied(vec.X, vec.Y, vec.Z, range, checkOptions.HasFlag(CheckOptions.CheckUnk1),
                checkOptions.HasFlag(CheckOptions.CheckPeds), checkOptions.HasFlag(CheckOptions.CheckVehicles),
                checkOptions.HasFlag(CheckOptions.CheckUnk2), checkOptions.HasFlag(CheckOptions.CheckUnk3),
                ignoreEntity, false);
        }

        public static Vector3 Midpoint(this Vector3 a, Vector3 b)
        {
            return new Vector3(
                (a.X + b.X) / 2f,
                (a.Y + b.Y) / 2f,
                (a.Z + b.Z) / 2f
            );
        }
        
        public static Vector2 Midpoint2D(this Vector3 a, Vector3 b)
        {
            return new Vector2(
                (a.X + b.X) / 2,
                (a.Y + b.Y) / 2
            );
        }
    }

    public static class EntityExt
    {
        /// <summary>
        /// Returns the distance between entity and secondEntity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="secondEntity"></param>
        /// <param name="squareResult">Should the value be squared (slower)</param>
        /// <returns></returns>
        public static float DistanceTo(this Entity entity, Entity secondEntity, bool squareResult = true)
        {
            return DistanceTo(entity, secondEntity.Position, squareResult);
        }
        
        /// <summary>
        /// Returns the distance between secondEntity and entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="secondEntity"></param>
        /// <param name="squareResult">Should the value be squared (slower)</param>
        /// <returns></returns>
        public static float DistanceFrom(this Entity entity, Entity secondEntity, bool squareResult = true)
        {
            return DistanceFrom(entity, secondEntity.Position, squareResult);
        }
        
        /// <summary>
        /// Returns the absolute distance between two entities
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="secondEntity"></param>
        /// <param name="squareResult">Should the value be squared (slower)</param>
        /// <returns></returns>
        public static float Distance(this Entity entity, Entity secondEntity, bool squareResult = true)
        {
            return Distance(entity, secondEntity.Position, squareResult);
        }
        
        /// <summary>
        /// Returns the distance between entity and the specified position
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="position">The position to compare distance to</param>
        /// <param name="squareResult">Should the value be squared (slower)</param>
        /// <returns></returns>
        public static float DistanceTo(this Entity entity, Vector3 position, bool squareResult = true)
        {
            if (squareResult)
            {
                return Vdist(entity.Position.X, entity.Position.Y, entity.Position.Z, position.X,
                    position.Y, position.Z);
            }
            else
            {
                return Vdist2(entity.Position.X, entity.Position.Y, entity.Position.Z, position.X,
                    position.Y, position.Z);
            }
        }
        
        /// <summary>
        /// Returns the distance between secondEntity and entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="position">The position to compare distance to</param>
        /// <param name="squareResult">Should the value be squared (slower)</param>
        /// <returns></returns>
        public static float DistanceFrom(this Entity entity, Vector3 position, bool squareResult = true)
        {
            if (squareResult)
            {
                return Vdist(position.X,
                    position.Y, position.Z, entity.Position.X, entity.Position.Y, entity.Position.Z);
            }
            else
            {
                return Vdist2(position.X,
                    position.Y, position.Z, entity.Position.X, entity.Position.Y, entity.Position.Z);
            }
        }
        
        /// <summary>
        /// Returns the absolute distance between two entities
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="position">The position to compare distance to</param>
        /// <param name="squareResult">Should the value be squared (slower)</param>
        /// <returns></returns>
        public static float Distance(this Entity entity, Vector3 position, bool squareResult = true)
        {
            float result;
            if (squareResult)
            {
                result = Vdist(position.X,
                    position.Y, position.Z, entity.Position.X, entity.Position.Y, entity.Position.Z);
            }
            else
            {
                result = Vdist2(position.X,
                    position.Y, position.Z, entity.Position.X, entity.Position.Y, entity.Position.Z);
            }

            return Math.Abs(result);
        }

        public static Vector3 GetOffset(this Entity entity, float x, float y, float z)
        {
            return API.GetOffsetFromEntityInWorldCoords(entity.Handle, x, y, z);
        }
        public static Vector3 GetOffset(this Entity entity, Vector3 offset)
        {
            return API.GetOffsetFromEntityInWorldCoords(entity.Handle, offset.X, offset.Y, offset.Z);
        }
    }
}