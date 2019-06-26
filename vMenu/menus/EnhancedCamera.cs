using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;
using MenuAPI;
using Newtonsoft.Json;

namespace vMenuClient {
    public class EnhancedCamera : BaseScript {

        #region variables

        private Menu menu;
        public bool LeadCam { get; private set; } = false;
        public bool ChaseCam { get; private set; } = false;
        public bool DroneCam { get; private set; } = false;

        private MenuCheckboxItem leadCam;
        private MenuCheckboxItem chaseCam;
        private MenuCheckboxItem droneCam;

        public static CustomCam CustomCamMenu { get; private set; }
        public static DroneCam DroneCamMenu { get; private set; }

        public Camera driftCamera = null;
        public Camera chaseCamera = null;
        public Camera droneCamera = null;

        public static float userTilt = 0.0f;
        public static float userYaw = 0.0f;
        public static bool userLookBehind = false;

        #endregion

        // Constructor
        public EnhancedCamera() {
            Tick += GeneralUpdate;
            Tick += SlowUpdate;
        }

        private void CreateMenu() {
            // Create the menu.
            menu = new Menu(Game.Player.Name, "Enhanced Camera");

            #region checkbox items

            // Enabling angular drift cam
            leadCam = new MenuCheckboxItem("Enable lead camera", "Make sure you have disabled X and Y camera lock in misc settings.", false);
            // Enabling chase cam
            chaseCam = new MenuCheckboxItem("Enable chase camera", "Locks to a target in front, switches to regular cam if target not in range. Make sure you have disabled X and Y camera lock in misc settings.", false);
            // Enabling chase cam
            droneCam = new MenuCheckboxItem("[WIP] Enable drone camera", "Free drone camera to spectate/fly around", false);

            #endregion
            
            #region adding menu items
            // Checkboxes
            menu.AddMenuItem(leadCam);
            menu.AddMenuItem(chaseCam);
            menu.AddMenuItem(droneCam);

            // Custom cam parameters menu
            CustomCamMenu = new CustomCam();
            Menu customCamMenu = CustomCamMenu.GetMenu();
            MenuItem buttonCustom = new MenuItem("Lead/chase cam parameters", "Tune parameters for lead and chase camera") {
                Label = "→→→"
            };
            menu.AddMenuItem(buttonCustom);
            MenuController.AddSubmenu(menu, customCamMenu);
            MenuController.BindMenuItem(menu, customCamMenu, buttonCustom);
            customCamMenu.RefreshIndex();

            // Drone cam parameters menu
            DroneCamMenu = new DroneCam();
            Menu droneCamMenu = DroneCamMenu.GetMenu();
            MenuItem buttonDrone = new MenuItem("Drone cam parameters", "Tune parameters for drone camera") {
                Label = "→→→"
            };
            menu.AddMenuItem(buttonDrone);
            MenuController.AddSubmenu(menu, droneCamMenu);
            MenuController.BindMenuItem(menu, droneCamMenu, buttonDrone);
            droneCamMenu.RefreshIndex();

            #endregion

            #region handling menu changes

            // Handle checkbox
            menu.OnCheckboxChange += (_menu, _item, _index, _checked) => {
                if (_item == leadCam) {

                    LeadCam = _checked;
                    MainMenu.EnhancedCamMenu.chaseCam.Checked = false;
                    MainMenu.EnhancedCamMenu.droneCam.Checked = false;
                    ChaseCam = false;
                    DroneCam = false;

                    if (!_checked) {
                        //DisableMenus();
                        ResetCameras();
                    }

                }
                if (_item == chaseCam) {

                    ChaseCam = _checked;
                    MainMenu.EnhancedCamMenu.leadCam.Checked = false;
                    MainMenu.EnhancedCamMenu.droneCam.Checked = false;
                    LeadCam = false;
                    DroneCam = false;

                    if (!_checked) {
                        //DisableMenus();
                        ResetCameras();
                    } else {
                        //EnableMenus();
                        CustomCam.target = CustomCam.GetClosestVehicle(2000, CustomCam.maxAngle);
                    }
                }
                if (_item == droneCam) {

                    DroneCam = _checked;
                    MainMenu.EnhancedCamMenu.chaseCam.Checked = false;
                    MainMenu.EnhancedCamMenu.leadCam.Checked = false;
                    ChaseCam = false;
                    LeadCam = false;

                    if (!_checked) {
                        //DisableMenus();
                        ResetCameras();
                    }

                }
            };
            #endregion

        }

        /// <summary>
        /// Creates the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetMenu() {
            if (menu == null) {
                CreateMenu();
            }
            return menu;
        }
        
        #region math functions

        public class CamMath {

            public const float DegToRad = (float)Math.PI / 180.0f;

            /// <summary>
            /// Lerps two float values by a step
            /// </summary>
            /// <returns>lerped float value in between two supplied</returns>
            public static float Lerp(float current, float target, float by) {
                return current * (1 - by) + target * by;
            }

            /// <summary>
            /// Calculates angle between two vectors
            /// </summary>
            /// <returns>Angle between vectors in degrees</returns>
            public static float AngleBetween(Vector3 a, Vector3 b) {
                float sinA = a.X * b.Y - b.X * a.Y;
                float cosA = a.X * b.X + a.Y * b.Y;
                return (float)Math.Atan2(sinA, cosA) / DegToRad;
            }

            public static Vector3 RotateRadians(Vector3 v, float degree) {
                float radians = DegToRad * degree;
                float ca = (float)Math.Cos(radians);
                float sa = (float)Math.Sin(radians);
                return new Vector3(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y, v.Z);
            }

            public static Vector3 RotateAroundAxis(Vector3 v, Vector3 axis, float angle) {
                return Vector3.TransformCoordinate(v, Matrix.RotationAxis(Vector3.Normalize(axis), angle));
            }

            public static float Fmod(float a, float b) {
                return (a - b * (float)Math.Floor(a / b));
            }

            public static Vector3 QuaternionToEuler(Quaternion q) {
                double r11 = (double)(-2 * (q.X * q.Y - q.W * q.Z));
                double r12 = (double)(q.W * q.W - q.X * q.X + q.Y * q.Y - q.Z * q.Z);
                double r21 = (double)(2 * (q.Y * q.Z + q.W * q.X));
                double r31 = (double)(-2 * (q.X * q.Z - q.W * q.Y));
                double r32 = (double)(q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z);

                float ax = (float)Math.Asin(r21);
                float ay = (float)Math.Atan2(r31, r32);
                float az = (float)Math.Atan2(r11, r12);

                return new Vector3(ax / DegToRad, ay / DegToRad, az / DegToRad);
            }
        }

        #endregion

        #region camera switching
        public void SwitchCameraToDrift() {
            SwitchToGameplayCam();
            LeadCam = true;
            //EnableMenus();
            leadCam.Checked = true;
        }

        public void SwitchCameraToChase() {
            SwitchToGameplayCam();
            ChaseCam = true;
            //EnableMenus();
            chaseCam.Checked = true;
        }

        public void SwitchToGameplayCam() {
            LeadCam = false;
            ChaseCam = false;
            DroneCam = false;
            //DisableMenus();
            ResetCameras();
            leadCam.Checked = false;
            chaseCam.Checked = false;
            droneCam.Checked = false;
        }

        /// <summary>
        /// Disables all the submenus except for three first checkboxes
        /// </summary>
        public void DisableMenus() {
            // Disable everything
            List<MenuItem> items = GetMenu().GetMenuItems();
            foreach (MenuItem item in items) {
                item.Enabled = false;
            }
            // Reenable Drift Cam, Chase Cam, Drone Cam fields
            if (items.Count >= 3) {
                items[0].Enabled = true;
                items[1].Enabled = true;
                items[2].Enabled = true;
            } else {
                Notify.Error("Your menu does not seem to have any submenus, something got corrupted.");
            }
        }

        /// <summary>
        /// Reenables all the submenus
        /// </summary>
        public void EnableMenus() {
            // Enable everything
            List<MenuItem> submenus = menu.GetMenuItems();
            foreach (MenuItem submenu in submenus) {
                submenu.Enabled = true;
            }
        }

        #endregion

        #region camera operations

        /// <summary>
        /// Checks whether any of the custom camera is active, used
        /// to disable background activity in OnTick functions
        /// </summary>
        /// <returns></returns>
        public bool IsCustomCameraEnabled() {
            if (MainMenu.EnhancedCamMenu != null)
                return (MainMenu.EnhancedCamMenu.LeadCam || MainMenu.EnhancedCamMenu.ChaseCam || MainMenu.EnhancedCamMenu.DroneCam);
            else
                return false;
        }

        /// <summary>
        /// Creates a base camera for lead and chase cam that is not
        /// attached to any entity
        /// </summary>
        /// <returns></returns>
        public Camera CreateNonAttachedCamera() {
            // Create new camera as a copy of GameplayCamera
            Camera newCam = World.CreateCamera(GameplayCamera.Position, GameplayCamera.Rotation, CustomCam.fov);
            newCam.FarClip = GetGameplayCamFarClip();
            newCam.DepthOfFieldStrength = 50f;
            newCam.MotionBlurStrength = 0.1f;
            newCam.IsActive = true;
            return newCam;
        }

        /// <summary>
        /// Used to reset lead and chase camera
        /// </summary>
        /// <returns></returns>
        public void ResetCameras() {
            World.RenderingCamera = null;
            MainMenu.EnhancedCamMenu.driftCamera = null;
            MainMenu.EnhancedCamMenu.chaseCamera = null;
            MainMenu.EnhancedCamMenu.droneCamera = null;
            World.DestroyAllCameras();
            SetFocusArea(GameplayCamera.Position.X, GameplayCamera.Position.Y, GameplayCamera.Position.Z, 0, 0, 0);
            EnableGameplayCam(true);
            UnlockMinimapAngle();
            ClearFocus();
        }

        private const float USER_YAW_RETURN_INTERPOLATION = 0.015f;
        private static float yawReturnTimer = 0f;

        /// <summary>
        /// Additional Update function, currently takes care
        /// of user's analog stick up and down movement to
        /// control the camera tilt
        /// </summary>
        /// <returns></returns>
        private async Task GeneralUpdate() {
            if (IsCustomCameraEnabled()) {
                // User controls the tilt offset
                float tiltControl = ((float)(GetControlValue(1, 2) / 256f) - 0.5f);
                float yawControl = ((float)(GetControlValue(1, 1) / 256f) - 0.5f);

                userLookBehind = IsControlPressed(1, 26);

                if ((Math.Abs(tiltControl) > 0.01f) || (Math.Abs(yawControl) > 0.01f)) {
                    //Account for difference in gamepad and mouse acceleration
                    if (IsInputDisabled(1)) {
                        userTilt -= tiltControl * 12f;
                        userYaw -= yawControl * 32;
                    } else {
                        userTilt -= tiltControl;
                        userYaw -= yawControl * 4f;
                    }
                    userTilt = (Math.Abs(userTilt) > 80f) ? (Math.Sign(userTilt) * 80f) : (userTilt);

                    userYaw = (CamMath.Fmod((userYaw + 180.0f), 360.0f) - 180.0f);
                    yawReturnTimer = 1f;    // Set the timer before yaw starts to return to 0f

                    // Slow return of user yaw to 0f
                } else if ((Math.Abs(yawControl) <= 0.01f) && (Math.Abs(userYaw) > (USER_YAW_RETURN_INTERPOLATION + 0.01f))) {
                    // Only return to 0f if user is not moving
                    int vehicleEntity = GetVehiclePedIsIn(PlayerPedId(), false);
                    if (yawReturnTimer <= 0f) {
                        float speedModifier = (Math.Abs(GetEntityVelocity(vehicleEntity).Length()) < 3f) ? (Math.Abs(GetEntityVelocity(vehicleEntity).Length()) / 3f) : (1f);
                        userYaw = Math.Sign(userYaw) * CamMath.Lerp(Math.Abs(userYaw), 0f, USER_YAW_RETURN_INTERPOLATION * speedModifier);
                    } else {
                        yawReturnTimer -= USER_YAW_RETURN_INTERPOLATION;
                    }
                } else {
                    await Delay(0);
                }
            } else {
                await Delay(1);
            }
        }

        private async Task SlowUpdate() {
            // Refocus render distance of the camera (too heavy for normal update)
            if (IsCustomCameraEnabled()) {
                if (MainMenu.EnhancedCamMenu.droneCamera != null) {
                    SetFocusArea(MainMenu.EnhancedCamMenu.droneCamera.Position.X, MainMenu.EnhancedCamMenu.droneCamera.Position.Y, MainMenu.EnhancedCamMenu.droneCamera.Position.Z, 0, 0, 0);
                    await Delay(25);
                }
            } else {
                await Delay(10);
            }
        }

        #endregion
        
    }
}
