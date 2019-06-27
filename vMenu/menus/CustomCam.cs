﻿using System;
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

    public static class CameraConstraints {
        public const float ROLL_MIN = (-50f);
        public const float ROLL_MAX = (50f);
        public const float PITCH_MIN = (-65f);
        public const float PITCH_MAX = (65f);

        public static float ClampRoll(float roll) {
            roll = (roll < ROLL_MIN) ? (ROLL_MIN) : (roll);
            roll = (roll > ROLL_MAX) ? (ROLL_MAX) : (roll);
            return roll;
        }
        public static float ClampPitch(float pitch) {
            pitch = (pitch < PITCH_MIN) ? (PITCH_MIN) : (pitch);
            pitch = (pitch > PITCH_MAX) ? (PITCH_MAX) : (pitch);
            return pitch;
        }
        public static bool OverClampCheck(float roll, float pitch) {
            return ((roll < ROLL_MIN) ||
                        (roll > ROLL_MAX) ||
                        (pitch < PITCH_MIN) ||
                        (pitch > PITCH_MAX));
        }
        public static bool CrashCheck(int veh) {
            return ((GetEntityRoll(veh) < ROLL_MIN) ||
                        (GetEntityRoll(veh) > ROLL_MAX) ||
                        (GetEntityPitch(veh) < PITCH_MIN) ||
                        (GetEntityPitch(veh) > PITCH_MAX));
        }
    }

    public class CustomCam : BaseScript {

        #region variables

        private Menu menu;

        private Dictionary<MenuItem, KeyValuePair<string, CameraInfo>> scMenuItems = new Dictionary<MenuItem, KeyValuePair<string, CameraInfo>>();
        private Menu savedCamerasMenu;
        private Menu selectedCameraMenu = new Menu("Manage Camera", "Manage this saved camera.");
        private static KeyValuePair<string, CameraInfo> currentlySelectedCamera = new KeyValuePair<string, CameraInfo>();

        #endregion

        // Constructor
        public CustomCam() {
            Tick += RunDriftCam;
            Tick += RunChaseCam;
        }

        private void CreateMenu() {
            menu = new Menu(Game.Player.Name, "Lead/chase camera parameters");

            #region checkbox items

            // Lock position offset
            MenuCheckboxItem lockPosOffsetCheckbox = new MenuCheckboxItem("Lock position offset", "Locks position offset, useful when sticking camera to the car - on top of hood, as FPV cam, etc.", false);
            // Linear position offset
            MenuCheckboxItem linearPosCheckbox = new MenuCheckboxItem("Linear position offset", "Instead of circular motion around the car, the camera moves along car's X axis. Dope for cinematic shots.", false);
            // Lock to ped
            MenuCheckboxItem pedLockCheckbox = new MenuCheckboxItem("Lock rotation to camera plane", "Changes the way that camera rotates around car (mostly visible on uneven ground).", false);

            #endregion

            #region main parameters

            // Angular velocity modifier
            List<string> angCamModifierValues = new List<string>();
            for (float i = -1f; i < 1f; i += 0.025f) {
                angCamModifierValues.Add(i.ToString("0.000"));
            }
            MenuListItem angCamModifierList = new MenuListItem("Modifier", angCamModifierValues, 48, "This modifier * angular velocity = target rotation. Higher values make camera move further from lock. (-1,1)") {
                ShowColorPanel = false
            };

            // Yaw interpolation modifier
            List<string> angCamInterpolationValues = new List<string>();
            for (float i = 0.0f; i < 1f; i += 0.005f) {
                angCamInterpolationValues.Add(i.ToString("0.000"));
            }
            MenuListItem angCamInterpolationList = new MenuListItem("Yaw interpolation", angCamInterpolationValues, 4, "Lower values - smoother movement. WARNING: Slider is inversed for chase camera - 0 is max interpolation, 1 is complete lock. (0,1)") {
                ShowColorPanel = false
            };

            // Roll interpolation modifier
            List<string> rollInterpolationValues = new List<string>();
            for (float i = 0.0f; i < 1f; i += 0.005f) {
                rollInterpolationValues.Add(i.ToString("0.000"));
            }
            MenuListItem rollInterpolationList = new MenuListItem("Roll interpolation", rollInterpolationValues, 20, "Lower values - smoother movement. (0,1)") {
                ShowColorPanel = false
            };

            // Roll interpolation modifier
            List<string> pitchInterpolationValues = new List<string>();
            for (float i = 0.0f; i < 1f; i += 0.005f) {
                pitchInterpolationValues.Add(i.ToString("0.000"));
            }
            MenuListItem pitchInterpolationList = new MenuListItem("Pitch interpolation", pitchInterpolationValues, 20, "Lower values - smoother movement. (0,1)") {
                ShowColorPanel = false
            };

            // Chase cam offset modifier
            List<string> chaseCamOffsetValues = new List<string>();
            for (float i = 0; i <= 5; i += 0.125f) {
                chaseCamOffsetValues.Add((i).ToString("0.000"));
            }
            MenuListItem chaseCamOffsetList = new MenuListItem("Camera offset", chaseCamOffsetValues, 0, "Offsets chase camera target towards its velocity vector. (0,5)") {
                ShowColorPanel = false
            };

            // Camera x position offset interpolation modifier
            List<string> posInterpolationValues = new List<string>();
            for (float i = 0.0f; i < 1f; i += 0.01f) {
                posInterpolationValues.Add(i.ToString("0.00"));
            }
            MenuListItem posInterpolationList = new MenuListItem("Position interpolation", posInterpolationValues, 100, "Lower values - smoother movement, higher delay. (0,1)") {
                ShowColorPanel = false
            };

            // FOV modifier
            List<string> customCamFOVValues = new List<string>();
            for (float i = 20; i <= 120; i += 1f) {
                customCamFOVValues.Add((i).ToString());
            }
            MenuListItem customCamFOVList = new MenuListItem("FOV", customCamFOVValues, 43, "Change custom camera's FOV. (20,120)") {
                ShowColorPanel = false
            };

            // Custom cam forward offset
            List<string> customCamForwardOffsetValues = new List<string>(100);
            for (float i = -8; i <= 8; i += 0.05f) {
                customCamForwardOffsetValues.Add((i).ToString("0.00"));
            }
            MenuListItem customCamForwardOffsetList = new MenuListItem("Y offset", customCamForwardOffsetValues, 65, "Custom camera offset in forward direction. (-8,8)") {
                ShowColorPanel = false
            };
            // Custom cam side offset
            List<string> customCamSideOffsetValues = new List<string>(100);
            for (float i = -5; i <= 8; i += 0.05f) {
                customCamSideOffsetValues.Add((i).ToString("0.00"));
            }
            MenuListItem customCamSideOffsetList = new MenuListItem("X offset", customCamSideOffsetValues, 100, "Custom camera offset in side direction. (-5,8)") {
                ShowColorPanel = false
            };
            // Custom cam up offset
            List<string> customCamUpOffsetValues = new List<string>(100);
            for (float i = -5; i <= 8; i += 0.05f) {
                customCamUpOffsetValues.Add((i).ToString("0.00"));
            }
            MenuListItem customCamUpOffsetList = new MenuListItem("Z offset", customCamUpOffsetValues, 141, "Custom camera offset in up direction. (-5,8)") {
                ShowColorPanel = false
            };

            List<string> chaseCamMaxAngleValues = new List<string>();
            for (float i = 25; i <= 360; i += 5) {
                chaseCamMaxAngleValues.Add(i.ToString());
            }
            MenuListItem chaseCamMaxAngleList = new MenuListItem("Max angle to lock.", chaseCamMaxAngleValues, 67, "Max angle from velocity vector to keep the lock on, if angle exceeds this limit, camera switches back to normal. (25,360)") {
                ShowColorPanel = false
            };

            #endregion

            #region adding menu items
            // Checkboxes
            menu.AddMenuItem(lockPosOffsetCheckbox);
            menu.AddMenuItem(linearPosCheckbox);
            menu.AddMenuItem(pedLockCheckbox);
            // Main modifier
            menu.AddMenuItem(angCamModifierList);
            // Interpolation sliders
            menu.AddMenuItem(angCamInterpolationList);
            menu.AddMenuItem(rollInterpolationList);
            menu.AddMenuItem(pitchInterpolationList);
            menu.AddMenuItem(posInterpolationList);
            // Chase camera
            menu.AddMenuItem(chaseCamOffsetList);
            menu.AddMenuItem(chaseCamMaxAngleList);
            // FOV and offset
            menu.AddMenuItem(customCamFOVList);
            menu.AddMenuItem(customCamForwardOffsetList);
            menu.AddMenuItem(customCamUpOffsetList);
            menu.AddMenuItem(customCamSideOffsetList);

            //MainMenu.EnhancedCamMenu.DisableMenus();

            #endregion

            #region managing save/load camera stuff

            // Saving/Loading cameras
            MenuItem savedCamerasButton = new MenuItem("Saved cameras", "User created cameras");
            savedCamerasMenu = new Menu("Saved cameras");
            MenuController.AddSubmenu(menu, savedCamerasMenu);
            menu.AddMenuItem(savedCamerasButton);
            savedCamerasButton.Label = "→→→";
            MenuController.BindMenuItem(menu, savedCamerasMenu, savedCamerasButton);

            MenuItem saveCamera = new MenuItem("Save Current Camera", "Save the current camera.");
            savedCamerasMenu.AddMenuItem(saveCamera);
            savedCamerasMenu.OnMenuOpen += (sender) => {
                savedCamerasMenu.ClearMenuItems();
                savedCamerasMenu.AddMenuItem(saveCamera);
                LoadCameras();
            };

            savedCamerasMenu.OnItemSelect += (sender, item, index) => {
                if (item == saveCamera) {
                    if (Game.PlayerPed.IsInVehicle()) {
                        SaveCamera();
                        savedCamerasMenu.GoBack();
                    } else {
                        Notify.Error("You are currently not in any vehicle. Please enter a vehicle before trying to save the camera.");
                    }
                } else {
                    UpdateSelectedCameraMenu(item, sender);
                }
            };

            MenuController.AddMenu(selectedCameraMenu);
            MenuItem spawnCamera = new MenuItem("Spawn Camera", "Spawn this saved camera.");
            MenuItem renameCamera = new MenuItem("Rename Camera", "Rename your saved camera.");
            MenuItem deleteCamera = new MenuItem("~r~Delete Camera", "~r~This will delete your saved camera. Warning: this can NOT be undone!");
            selectedCameraMenu.AddMenuItem(spawnCamera);
            selectedCameraMenu.AddMenuItem(renameCamera);
            selectedCameraMenu.AddMenuItem(deleteCamera);

            selectedCameraMenu.OnMenuClose += (sender) => {
                selectedCameraMenu.RefreshIndex();
            };

            selectedCameraMenu.OnItemSelect += async (sender, item, index) => {
                if (item == spawnCamera) {
                    MainMenu.EnhancedCamMenu.ResetCameras();
                    SpawnSavedCamera();

                    if (MainMenu.EnhancedCamMenu.LeadCam) {
                        MainMenu.EnhancedCamMenu.driftCamera = MainMenu.EnhancedCamMenu.CreateNonAttachedCamera();
                        World.RenderingCamera = MainMenu.EnhancedCamMenu.driftCamera;
                        MainMenu.EnhancedCamMenu.driftCamera.IsActive = true;
                    } else if (MainMenu.EnhancedCamMenu.ChaseCam) {
                        MainMenu.EnhancedCamMenu.chaseCamera = MainMenu.EnhancedCamMenu.CreateNonAttachedCamera();
                        World.RenderingCamera = MainMenu.EnhancedCamMenu.chaseCamera;
                        MainMenu.EnhancedCamMenu.chaseCamera.IsActive = true;
                    }

                    // Update menu stuff according to loaded values
                    angCamModifierList.ListIndex = (int)((angCamModifier - 0.0001f + 1f) / 0.025f);
                    angCamInterpolationList.ListIndex = (int)((angCamInterpolation) / 0.005f);
                    chaseCamOffsetList.ListIndex = (chaseCamOffset);
                    posInterpolationList.ListIndex = (int)((posInterpolation) / 0.01f);
                    rollInterpolationList.ListIndex = (int)((cameraRollInterpolation) / 0.01f);
                    pitchInterpolationList.ListIndex = (int)((cameraPitchInterpolation) / 0.01f);
                    chaseCamMaxAngleList.ListIndex = (int)((maxAngle - 25f) / 5f);
                    customCamFOVList.ListIndex = (int)(fov - 20.0f);
                    customCamForwardOffsetList.ListIndex = (int)((forwardOffset + 8f) / 0.05f);
                    customCamUpOffsetList.ListIndex = (int)((upOffset + 5f) / 0.05f);
                    customCamSideOffsetList.ListIndex = (int)((sideOffset + 5f) / 0.05f);
                    lockPosOffsetCheckbox.Checked = lockOffsetPos;
                    linearPosCheckbox.Checked = linearPosOffset;
                    pedLockCheckbox.Checked = pedLock;

                    selectedCameraMenu.GoBack();
                    savedCamerasMenu.RefreshIndex();
                    menu.RefreshIndex();

                } else if (item == deleteCamera) {
                    item.Label = "";
                    DeleteResourceKvp(currentlySelectedCamera.Key);
                    selectedCameraMenu.GoBack();
                    savedCamerasMenu.RefreshIndex();
                    Notify.Success("Your saved camera has been deleted.");
                } else if (item == renameCamera) {
                    string newName = await GetUserInput(windowTitle: "Enter a new name for this camera.", maxInputLength: 30);
                    if (string.IsNullOrEmpty(newName)) {
                        Notify.Error(CommonErrors.InvalidInput);
                    } else {
                        if (SaveCameraInfo("xcm_" + newName, currentlySelectedCamera.Value, false)) {
                            DeleteResourceKvp(currentlySelectedCamera.Key);
                            while (!selectedCameraMenu.Visible) {
                                await BaseScript.Delay(0);
                            }
                            Notify.Success("Your camera has successfully been renamed.");
                            selectedCameraMenu.GoBack();
                            currentlySelectedCamera = new KeyValuePair<string, CameraInfo>();
                        } else {
                            Notify.Error("This name is already in use or something unknown failed. Contact the server owner if you believe something is wrong.");
                        }
                    }
                }
            };

            #endregion

            #region handling menu changes

            // Handle checkbox
            menu.OnCheckboxChange += (_menu, _item, _index, _checked) => {
                if (_item == linearPosCheckbox) {
                    linearPosOffset = _checked;
                }
                if (_item == lockPosOffsetCheckbox) {
                    lockOffsetPos = _checked;
                }
                if (_item == pedLockCheckbox) {
                    pedLock = _checked;
                }
            };

            // Handle list change
            menu.OnListIndexChange += (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) => {
                if (_listItem == angCamModifierList) {
                    angCamModifier = _newIndex * 0.025f + 0.0001f - 1f;
                }
                if (_listItem == angCamInterpolationList) {
                    angCamInterpolation = ((_newIndex) * 0.005f);
                }
                if (_listItem == chaseCamOffsetList) {
                    chaseCamOffset = (_newIndex);
                }
                if (_listItem == posInterpolationList) {
                    posInterpolation = ((_newIndex) * 0.01f);
                }
                if (_listItem == rollInterpolationList) {
                    cameraRollInterpolation = ((_newIndex) * 0.01f);
                }
                if (_listItem == pitchInterpolationList) {
                    cameraPitchInterpolation = ((_newIndex) * 0.01f);
                }
                if (_listItem == chaseCamMaxAngleList) {
                    maxAngle = (float)(_newIndex * 5f + 25f);
                }
                if (_listItem == customCamFOVList) {
                    if ((MainMenu.EnhancedCamMenu.driftCamera != null) || (MainMenu.EnhancedCamMenu.chaseCamera != null))
                        MainMenu.EnhancedCamMenu.ResetCameras();
                    fov = (float)(_newIndex * 1f + 20.0f);
                    if (MainMenu.EnhancedCamMenu.LeadCam) {
                        MainMenu.EnhancedCamMenu.driftCamera = MainMenu.EnhancedCamMenu.CreateNonAttachedCamera();
                        World.RenderingCamera = MainMenu.EnhancedCamMenu.driftCamera;
                        MainMenu.EnhancedCamMenu.driftCamera.IsActive = true;
                    } else if (MainMenu.EnhancedCamMenu.ChaseCam) {
                        MainMenu.EnhancedCamMenu.chaseCamera = MainMenu.EnhancedCamMenu.CreateNonAttachedCamera();
                        World.RenderingCamera = MainMenu.EnhancedCamMenu.chaseCamera;
                        MainMenu.EnhancedCamMenu.chaseCamera.IsActive = true;
                    }
                }
                if (_listItem == customCamForwardOffsetList) {
                    if ((MainMenu.EnhancedCamMenu.driftCamera != null) || (MainMenu.EnhancedCamMenu.chaseCamera != null))
                        MainMenu.EnhancedCamMenu.ResetCameras();
                    forwardOffset = (float)(_newIndex * 0.05f - 8f);
                    if (MainMenu.EnhancedCamMenu.LeadCam) {
                        MainMenu.EnhancedCamMenu.driftCamera = MainMenu.EnhancedCamMenu.CreateNonAttachedCamera();
                        World.RenderingCamera = MainMenu.EnhancedCamMenu.driftCamera;
                        MainMenu.EnhancedCamMenu.driftCamera.IsActive = true;
                    } else if (MainMenu.EnhancedCamMenu.ChaseCam) {
                        MainMenu.EnhancedCamMenu.chaseCamera = MainMenu.EnhancedCamMenu.CreateNonAttachedCamera();
                        World.RenderingCamera = MainMenu.EnhancedCamMenu.chaseCamera;
                        MainMenu.EnhancedCamMenu.chaseCamera.IsActive = true;
                    }
                }
                if (_listItem == customCamSideOffsetList) {
                    if ((MainMenu.EnhancedCamMenu.driftCamera != null) || (MainMenu.EnhancedCamMenu.chaseCamera != null))
                        MainMenu.EnhancedCamMenu.ResetCameras();
                    sideOffset = (float)(_newIndex * 0.05f - 5f);
                    if (MainMenu.EnhancedCamMenu.LeadCam) {
                        MainMenu.EnhancedCamMenu.driftCamera = MainMenu.EnhancedCamMenu.CreateNonAttachedCamera();
                        World.RenderingCamera = MainMenu.EnhancedCamMenu.driftCamera;
                        MainMenu.EnhancedCamMenu.driftCamera.IsActive = true;
                    } else if (MainMenu.EnhancedCamMenu.ChaseCam) {
                        MainMenu.EnhancedCamMenu.chaseCamera = MainMenu.EnhancedCamMenu.CreateNonAttachedCamera();
                        World.RenderingCamera = MainMenu.EnhancedCamMenu.chaseCamera;
                        MainMenu.EnhancedCamMenu.chaseCamera.IsActive = true;
                    }
                }
                if (_listItem == customCamUpOffsetList) {
                    if ((MainMenu.EnhancedCamMenu.driftCamera != null) || (MainMenu.EnhancedCamMenu.chaseCamera != null))
                        MainMenu.EnhancedCamMenu.ResetCameras();
                    upOffset = (float)(_newIndex * 0.05f - 5f);
                    if (MainMenu.EnhancedCamMenu.LeadCam) {
                        MainMenu.EnhancedCamMenu.driftCamera = MainMenu.EnhancedCamMenu.CreateNonAttachedCamera();
                        World.RenderingCamera = MainMenu.EnhancedCamMenu.driftCamera;
                        MainMenu.EnhancedCamMenu.driftCamera.IsActive = true;
                    } else if (MainMenu.EnhancedCamMenu.ChaseCam) {
                        MainMenu.EnhancedCamMenu.chaseCamera = MainMenu.EnhancedCamMenu.CreateNonAttachedCamera();
                        World.RenderingCamera = MainMenu.EnhancedCamMenu.chaseCamera;
                        MainMenu.EnhancedCamMenu.chaseCamera.IsActive = true;
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

        #region custom camera static variables

        public static float fov = 63.0f;
        private static float forwardOffset = -4.75f;
        private static float sideOffset = 0.0f;
        private static float upOffset = 2.05f;
        private static bool linearPosOffset = false;
        private static bool lockOffsetPos = false;
        private static float angCamModifier = 0.2f;
        private static float angCamInterpolation = 0.02f;
        private static float angularVelOld = 0f;
        private static float posInterpolation = 0.5f;
        private static float oldPosXOffset = 0f;
        public static float maxAngle = 360f;

        private static float cameraRollInterpolation = 0.1f;
        private static float cameraPitchInterpolation = 0.1f;

        private static bool pedLock = false;

        #endregion

        #region drift camera

        // Consts
        private const float MAX_ANG_VEL_OFFSET = 1.0f;
        private const float ROTATION_NORMALIZE = 100.0f;

        /// <summary>
        /// Changes main render camera behaviour, follows car with specified degree of freedom
        /// based on modifier value and interpolation value (and other variables such as
        /// angle and position offset values).
        /// </summary>
        /// <returns></returns>
        private async Task RunDriftCam() {
            if (MainMenu.EnhancedCamMenu != null) {
                if (MainMenu.EnhancedCamMenu.LeadCam) {
                    int vehicleEntity = GetVehiclePedIsIn(PlayerPedId(), false);
                    if (vehicleEntity > 0) {
                        if (MainMenu.EnhancedCamMenu.driftCamera != null) {
                            // Get vehicle's angular velocity
                            float angularVel = GetEntityRotationVelocity(vehicleEntity).Z;
                            // Keep it in reasonable range
                            angularVel = (angularVel > MAX_ANG_VEL_OFFSET) ? (MAX_ANG_VEL_OFFSET) : (angularVel);
                            // Lerp to smooth the camera transition
                            angularVel = EnhancedCamera.CamMath.Lerp(angularVelOld, angularVel, angCamInterpolation);
                            // Save the value to lerp with it in the next frame
                            angularVelOld = angularVel;

                            // Calculating target camera rotation
                            float finalRotation = -angularVel * angCamModifier * ROTATION_NORMALIZE;

                            // Get vehicle entity for further operations
                            Vehicle veh = new Vehicle(vehicleEntity);

                            // Setting the position offset also based on angular velocity
                            if (!lockOffsetPos) {
                                oldPosXOffset = EnhancedCamera.CamMath.Lerp(oldPosXOffset, finalRotation, posInterpolation);
                            } else {
                                oldPosXOffset = 0f;
                            }

                            // Get the static offset based on user's input
                            Vector3 staticPosition = Vector3.Zero;
                            if (pedLock) {
                                staticPosition = veh.ForwardVector * forwardOffset +
                                                    veh.RightVector * sideOffset +
                                                    Vector3.ForwardLH * upOffset;
                            } else {
                                staticPosition = veh.ForwardVector * forwardOffset +
                                                    veh.RightVector * sideOffset +
                                                    veh.UpVector * upOffset;
                            }

                            // Calculate final offset taking into consideration dynamic offset (oldPosXOffset), static
                            // offset and the offset resulting from rotating the camera around the car
                            if (!linearPosOffset) {
                                if (oldPosXOffset != 0f) {
                                    float rotation = oldPosXOffset + EnhancedCamera.userYaw;
                                    if (pedLock) {
                                        MainMenu.EnhancedCamMenu.driftCamera.Position = veh.Position + EnhancedCamera.CamMath.RotateAroundAxis(staticPosition, Vector3.ForwardLH, rotation * EnhancedCamera.CamMath.DegToRad);
                                    } else {
                                        MainMenu.EnhancedCamMenu.driftCamera.Position = veh.Position + EnhancedCamera.CamMath.RotateAroundAxis(staticPosition, veh.UpVector, rotation * EnhancedCamera.CamMath.DegToRad);
                                    }
                                    if (EnhancedCamera.userLookBehind) {
                                        MainMenu.EnhancedCamMenu.driftCamera.Position = veh.Position +
                                            EnhancedCamera.CamMath.RotateAroundAxis(staticPosition, veh.UpVector, 179f * EnhancedCamera.CamMath.DegToRad);
                                    }
                                } else {
                                    MainMenu.EnhancedCamMenu.driftCamera.Position = veh.Position + staticPosition;
                                    if (EnhancedCamera.userLookBehind) {
                                        MainMenu.EnhancedCamMenu.driftCamera.Position = veh.Position +
                                                                                        staticPosition -
                                                                                        (veh.RightVector * sideOffset) +
                                                                                        veh.ForwardVector * 3.5f +
                                                                                        veh.UpVector * 0.5f;
                                    }
                                }
                            } else {
                                MainMenu.EnhancedCamMenu.driftCamera.Position = veh.Position + staticPosition + veh.RightVector * oldPosXOffset / 12f;
                                if (EnhancedCamera.userLookBehind) { MainMenu.EnhancedCamMenu.driftCamera.Position = veh.Position + staticPosition + veh.ForwardVector * 3f + veh.UpVector * 0.5f; }
                            }

                            // Calculate target rotation as a heading in given range
                            Vector3 newRot = GameMath.DirectionToRotation(GameMath.HeadingToDirection((finalRotation + GetEntityRotation(vehicleEntity, 2).Z + 180.0f) % 360.0f - 180.0f), GetEntityRoll(vehicleEntity));
                            float roll = 0f;
                            float pitch = 0f;
                            // Clamp values
                            if (CameraConstraints.CrashCheck(vehicleEntity)) {
                                staticPosition = Vector3.ForwardLH * upOffset;
                                MainMenu.EnhancedCamMenu.driftCamera.Position = veh.Position + staticPosition;
                                roll = EnhancedCamera.CamMath.Lerp(MainMenu.EnhancedCamMenu.driftCamera.Rotation.Y, 0f, 0.1f);
                                pitch = EnhancedCamera.CamMath.Lerp(MainMenu.EnhancedCamMenu.driftCamera.Rotation.X, 0f, 0.1f);
                                pitch = CameraConstraints.ClampPitch(pitch);
                            } else {
                                // Calculate smooth roll and pitch rotation
                                roll = EnhancedCamera.CamMath.Lerp(MainMenu.EnhancedCamMenu.driftCamera.Rotation.Y, -GetEntityRoll(vehicleEntity), cameraRollInterpolation);
                                pitch = EnhancedCamera.CamMath.Lerp(MainMenu.EnhancedCamMenu.driftCamera.Rotation.X - EnhancedCamera.userTilt, GetEntityPitch(vehicleEntity), cameraPitchInterpolation);
                                roll = CameraConstraints.ClampRoll(roll);
                                pitch = CameraConstraints.ClampPitch(pitch);
                            }
                            // Finalize the rotation
                            float yaw = (EnhancedCamera.userLookBehind) ? (newRot.Z + 179f) : (newRot.Z + EnhancedCamera.userYaw);
                            pitch *= (EnhancedCamera.userLookBehind) ? (-1f) : (1f);
                            SetCamRot(MainMenu.EnhancedCamMenu.driftCamera.Handle, pitch + EnhancedCamera.userTilt, roll, yaw, 2);

                            // Update minimap
                            LockMinimapAngle((int)(EnhancedCamera.CamMath.Fmod(yaw, 360f)));
                        } else {
                            // In case the camera is null - reset the cameras and reassign this camera
                            MainMenu.EnhancedCamMenu.ResetCameras();
                            MainMenu.EnhancedCamMenu.driftCamera = MainMenu.EnhancedCamMenu.CreateNonAttachedCamera();
                            World.RenderingCamera = MainMenu.EnhancedCamMenu.driftCamera;
                            MainMenu.EnhancedCamMenu.driftCamera.IsActive = true;
                        }
                    }
                }
            } else {
                await Delay(0);
            }
        }

        #endregion

        #region chase camera

        /// <summary>
        /// Gets closest vehicle to Ped
        /// </summary>
        /// <returns>closest vehicle</returns>
        public static Vehicle GetClosestVehicle(int maxDistance, float requiredAngle) {
            float smallestDistance = (float)maxDistance;
            Vehicle[] vehs = World.GetAllVehicles();
            Vehicle closestVeh = null;

            int playerVeh = GetVehiclePedIsIn(PlayerPedId(), false);

            if (vehs != null) {
                foreach (Vehicle veh in vehs) {
                    if (veh.Handle != playerVeh) {
                        float distance = Vector3.Distance(GetEntityCoords(veh.Handle, true), GetEntityCoords(playerVeh, true));
                        if ((distance <= smallestDistance) && (veh != null)) {
                            smallestDistance = distance;
                            Vector3 targetVec = GetOffsetFromEntityGivenWorldCoords(playerVeh, veh.Position.X, veh.Position.Y, veh.Position.Z);
                            float angle = -EnhancedCamera.CamMath.AngleBetween(targetVec, new Vector3(0, 0.0001f, 0) + GetEntitySpeedVector(playerVeh, true));
                            // Make sure that target is in range given by angle
                            if (Math.Abs(angle) < requiredAngle) {
                                closestVeh = veh;
                            }
                        }
                    }
                }
            }
            return closestVeh;
        }

        public static Vehicle target = null;
        private static int chaseCamOffset = 0;

        /// <summary>
        /// Changes main render camera behaviour, camera locks onto closest vehicle
        /// that is in front of the player (in certain degree range in front of car's
        /// velocity's magnitude).
        /// </summary>
        /// <returns></returns>
        private async Task RunChaseCam() {
            if (MainMenu.EnhancedCamMenu != null) {
                if (MainMenu.EnhancedCamMenu.ChaseCam) {
                    // Get player's vehicle
                    int vehicleEntity = GetVehiclePedIsIn(PlayerPedId(), false);
                    if (vehicleEntity > 0) {
                        if (MainMenu.EnhancedCamMenu.chaseCamera != null) {

                            // If target car is located
                            if (target != null) {
                                // Get vector from player's car to target car offset by value
                                Vector3 targetVec = GetOffsetFromEntityGivenWorldCoords(
                                                        vehicleEntity,
                                                        target.Position.X + target.ForwardVector.X * (chaseCamOffset / 5),
                                                        target.Position.Y + target.ForwardVector.Y * (chaseCamOffset / 5),
                                                        target.Position.Z);

                                // Get rotation to target vehicle
                                float finalRotation = -EnhancedCamera.CamMath.AngleBetween(targetVec, new Vector3(0, 10, 0));

                                if (Math.Abs(finalRotation) > maxAngle) {
                                    target = null;
                                    MainMenu.EnhancedCamMenu.SwitchCameraToDrift();
                                    Notify.Info("Target exceeded angle limit, switching to Lead Camera", false);
                                    return;
                                }

                                if (finalRotation.ToString() != "NaN") {
                                    // Lerp target rotation
                                    // (1 - angCamInterpolation) instead of just interpolation so that camera
                                    // can be changed smoothly from lead cam to chase cam
                                    finalRotation = EnhancedCamera.CamMath.Lerp(GetEntityHeading(MainMenu.EnhancedCamMenu.chaseCamera.Handle), finalRotation, 1 - angCamInterpolation);

                                    // Calculate camera's position
                                    Vehicle veh = new Vehicle(vehicleEntity);

                                    // Static position as an offset from the car
                                    Vector3 staticPosition = Vector3.Zero;
                                    if (pedLock) {
                                        staticPosition = veh.ForwardVector * forwardOffset +
                                                            veh.RightVector * sideOffset +
                                                            Vector3.ForwardLH * upOffset;
                                    } else {
                                        staticPosition = veh.ForwardVector * forwardOffset +
                                                            veh.RightVector * sideOffset +
                                                            veh.UpVector * upOffset;
                                    }

                                    // Calculate chase camera position
                                    if (!lockOffsetPos) {
                                        float rotation = finalRotation + EnhancedCamera.userYaw;
                                        if (pedLock) {
                                            MainMenu.EnhancedCamMenu.chaseCamera.Position = veh.Position + EnhancedCamera.CamMath.RotateAroundAxis(staticPosition, Vector3.ForwardLH, rotation * EnhancedCamera.CamMath.DegToRad);
                                        } else {
                                            MainMenu.EnhancedCamMenu.chaseCamera.Position = veh.Position + EnhancedCamera.CamMath.RotateAroundAxis(staticPosition, veh.UpVector, rotation * EnhancedCamera.CamMath.DegToRad);
                                        }
                                        if (EnhancedCamera.userLookBehind) { MainMenu.EnhancedCamMenu.chaseCamera.Position = veh.Position - (veh.RightVector * sideOffset) + EnhancedCamera.CamMath.RotateAroundAxis(staticPosition, veh.UpVector, 179f * EnhancedCamera.CamMath.DegToRad); }
                                    } else {
                                        MainMenu.EnhancedCamMenu.chaseCamera.Position = veh.Position + staticPosition;
                                        if (EnhancedCamera.userLookBehind) { MainMenu.EnhancedCamMenu.chaseCamera.Position = veh.Position + staticPosition + veh.ForwardVector * 3f + veh.UpVector * 0.5f; }
                                    }

                                    // Calculate the camera rotation
                                    Vector3 newRot = GameMath.DirectionToRotation(GameMath.HeadingToDirection((finalRotation + GetEntityRotation(vehicleEntity, 4).Z + 180.0f) % 360.0f - 180.0f), GetEntityRoll(vehicleEntity));

                                    // Calculate smooth roll and pitch rotation
                                    float roll = 0f;
                                    float pitch = 0f;
                                    // Clamp values
                                    if (CameraConstraints.CrashCheck(vehicleEntity)) {
                                        staticPosition = Vector3.ForwardLH * upOffset;
                                        MainMenu.EnhancedCamMenu.chaseCamera.Position = veh.Position + staticPosition;
                                        roll = EnhancedCamera.CamMath.Lerp(MainMenu.EnhancedCamMenu.chaseCamera.Rotation.Y, 0f, 0.1f);
                                        pitch = EnhancedCamera.CamMath.Lerp(MainMenu.EnhancedCamMenu.chaseCamera.Rotation.X, 0f, 0.1f);
                                        pitch = CameraConstraints.ClampPitch(pitch);
                                    } else {
                                        // Calculate smooth roll and pitch rotation
                                        roll = EnhancedCamera.CamMath.Lerp(MainMenu.EnhancedCamMenu.chaseCamera.Rotation.Y, -GetEntityRoll(vehicleEntity), cameraRollInterpolation);
                                        pitch = EnhancedCamera.CamMath.Lerp(MainMenu.EnhancedCamMenu.chaseCamera.Rotation.X - EnhancedCamera.userTilt, GetEntityPitch(vehicleEntity), cameraPitchInterpolation);
                                        roll = CameraConstraints.ClampRoll(roll);
                                        pitch = CameraConstraints.ClampPitch(pitch);
                                    }
                                    // Finally, set the rotation
                                    float yaw = (EnhancedCamera.userLookBehind) ? (newRot.Z + 179f) : (newRot.Z + EnhancedCamera.userYaw);
                                    MainMenu.EnhancedCamMenu.chaseCamera.Rotation = new Vector3(pitch + EnhancedCamera.userTilt, roll, yaw);

                                    // Update minimap
                                    LockMinimapAngle((int)(EnhancedCamera.CamMath.Fmod(yaw, 360f)));
                                }
                            } else {
                                // Target car not found - switch to Lead Cam
                                MainMenu.EnhancedCamMenu.SwitchCameraToDrift();
                                Notify.Info("Target not found, switching to Lead Camera", false);
                            }

                            // Find target and generate camera
                        } else {
                            MainMenu.EnhancedCamMenu.ResetCameras();
                            MainMenu.EnhancedCamMenu.chaseCamera = MainMenu.EnhancedCamMenu.CreateNonAttachedCamera();
                            World.RenderingCamera = MainMenu.EnhancedCamMenu.chaseCamera;
                            MainMenu.EnhancedCamMenu.chaseCamera.IsActive = true;
                            target = GetClosestVehicle(2000, maxAngle);
                        }
                    }
                }
            } else {
                await Delay(0);
            }
        }

        #endregion
        
        #region save/load

        private struct CameraInfo {
            public float angCamInterpolation_;
            public float angCamModifier_;
            public float posInterpolation_;
            public float chaseCamMaxAngle_;
            public bool linearPosOffset_;
            public bool lockOffsetPos_;
            public float customCamFOV_;
            public float customCamForwardOffset_;
            public float customCamUpOffset_;
            public float customCamSideOffset_;
            public float cameraRollInterpolation_;
            public float cameraPitchInterpolation_;
            public bool pedLock_;
        }

        private bool UpdateSelectedCameraMenu(MenuItem selectedItem, Menu parentMenu = null) {
            if (!scMenuItems.ContainsKey(selectedItem)) {
                Notify.Error("In some very strange way, you've managed to select a button, that does not exist according to this list. So your vehicle could not be loaded. :( Maybe your save files are broken?");
                return false;
            }
            var camInfo = scMenuItems[selectedItem];
            currentlySelectedCamera = camInfo;
            selectedCameraMenu.MenuSubtitle = $"{camInfo.Key.Substring(4)}";
            MenuController.CloseAllMenus();
            selectedCameraMenu.OpenMenu();
            if (parentMenu != null) {
                MenuController.AddSubmenu(parentMenu, selectedCameraMenu);
            }
            return true;
        }

        private bool SpawnSavedCamera() {
            if (currentlySelectedCamera.Key != null) {
                angCamInterpolation = currentlySelectedCamera.Value.angCamInterpolation_;
                angCamModifier = currentlySelectedCamera.Value.angCamModifier_;
                posInterpolation = currentlySelectedCamera.Value.posInterpolation_;
                maxAngle = currentlySelectedCamera.Value.chaseCamMaxAngle_;
                linearPosOffset = currentlySelectedCamera.Value.linearPosOffset_;
                lockOffsetPos = currentlySelectedCamera.Value.lockOffsetPos_;
                fov = currentlySelectedCamera.Value.customCamFOV_;
                forwardOffset = currentlySelectedCamera.Value.customCamForwardOffset_;
                upOffset = currentlySelectedCamera.Value.customCamUpOffset_;
                sideOffset = currentlySelectedCamera.Value.customCamSideOffset_;
                cameraRollInterpolation = currentlySelectedCamera.Value.cameraRollInterpolation_;
                cameraPitchInterpolation = currentlySelectedCamera.Value.cameraPitchInterpolation_;
                pedLock = currentlySelectedCamera.Value.pedLock_;
            } else {
                Notify.Error("It seems that this slot got corrupted in some way, you need to delete it.");
                return false;
            }
            return true;
        }

        private bool SaveCameraInfo(string saveName, CameraInfo cameraInfo, bool overrideOldVersion) {
            if (string.IsNullOrEmpty(GetResourceKvpString(saveName)) || overrideOldVersion) {
                if (!string.IsNullOrEmpty(saveName) && saveName.Length > 4) {
                    // convert
                    string json = JsonConvert.SerializeObject(cameraInfo);

                    // log
                    Log($"[vMenu] Saving!\nName: {saveName}\nCamera Data: {json}\n");

                    // save
                    SetResourceKvp(saveName, json);

                    // confirm
                    return GetResourceKvpString(saveName) == json;
                }
            }
            // if something isn't right, then the save is aborted and return false ("failed" state).
            return false;
        }

        public async void SaveCamera(string updateExistingSavedCameraName = null) {
            // Only continue if the player is in a vehicle.
            if (Game.PlayerPed.IsInVehicle()) {
                CameraInfo ci = new CameraInfo() {
                    angCamInterpolation_ = angCamInterpolation,
                    angCamModifier_ = angCamModifier,
                    posInterpolation_ = posInterpolation,
                    chaseCamMaxAngle_ = maxAngle,
                    linearPosOffset_ = linearPosOffset,
                    lockOffsetPos_ = lockOffsetPos,
                    customCamFOV_ = fov,
                    customCamForwardOffset_ = forwardOffset,
                    customCamUpOffset_ = upOffset,
                    customCamSideOffset_ = sideOffset,
                    cameraRollInterpolation_ = cameraRollInterpolation,
                    cameraPitchInterpolation_ = cameraPitchInterpolation,
                    pedLock_ = pedLock
                };

                if (updateExistingSavedCameraName == null) {
                    var saveName = await GetUserInput(windowTitle: "Enter a save name", maxInputLength: 30);
                    // If the name is not invalid.
                    if (!string.IsNullOrEmpty(saveName)) {
                        // Save everything from the dictionary into the client's kvp storage.
                        // If the save was successfull:
                        if (SaveCameraInfo("xcm_" + saveName, ci, false)) {
                            Notify.Success($"Camera {saveName} saved.");
                            LoadCameras();
                        }
                        // If the save was not successfull:
                        else {
                            Notify.Error(CommonErrors.SaveNameAlreadyExists, placeholderValue: "(" + saveName + ")");
                        }
                    }
                    // The user did not enter a valid name to use as a save name for this vehicle.
                    else {
                        Notify.Error(CommonErrors.InvalidSaveName);
                    }
                }
                // We need to update an existing slot.
                else {
                    SaveCameraInfo("xcm_" + updateExistingSavedCameraName, ci, true);
                }
            }
            // The player is not inside a vehicle.
            else {
                Notify.Error(CommonErrors.NoVehicle);
            }
        }

        private Dictionary<string, CameraInfo> GetSavedCameras() {
            // Create a list to store all saved camera names in.
            var savedCameraNames = new List<string>();
            // Start looking for kvps starting with xcm_
            var findHandle = StartFindKvp("xcm_");
            // Keep looking...
            while (true) {
                // Get the kvp string key.
                var camString = FindKvp(findHandle);

                // If it exists then the key to the list.
                if (camString != "" && camString != null && camString != "NULL") {
                    savedCameraNames.Add(camString);
                }
                // Otherwise stop.
                else {
                    EndFindKvp(findHandle);
                    break;
                }
            }
            var camerasList = new Dictionary<string, CameraInfo>();
            // Loop through all save names (keys) from the list above, convert the string into a dictionary 
            // and add it to the dictionary above, with the camera save name as the key.
            foreach (var saveName in savedCameraNames) {
                camerasList.Add(saveName, JsonConvert.DeserializeObject<CameraInfo>(GetResourceKvpString(saveName)));
            }
            // Return the camera dictionary containing all camera save names (keys) linked to the correct camera
            return camerasList;
        }

        private async void LoadCameras() {
            var savedCameras = GetSavedCameras();
            scMenuItems = new Dictionary<MenuItem, KeyValuePair<string, CameraInfo>>();

            foreach (var sc in savedCameras) {
                MenuItem savedCameraBtn;
                if (sc.Key.Length > 4) {
                    savedCameraBtn = new MenuItem(sc.Key.Substring(4), $"Manage this saved camera.") {
                        Label = $"→→→"
                    };
                } else {
                    savedCameraBtn = new MenuItem("NULL", $"Manage this saved camera.") {
                        Label = $"→→→"
                    };
                }
                savedCamerasMenu.AddMenuItem(savedCameraBtn);
                scMenuItems.Add(savedCameraBtn, sc);
            }
            await Delay(0);
        }

        #endregion
    }
}
