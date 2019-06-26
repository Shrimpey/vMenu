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
    public class DroneCam : BaseScript {

        private Menu menu;

        // Constructor
        public DroneCam() {
            Tick += RunDroneCam;
        }

        private void CreateMenu() {
            menu = new Menu(Game.Player.Name, "Drone Camera parameters");

            #region main parameters

            // Gravity multiplier
            List<string> gravityMultValues = new List<string>();
            for (float i = 0.5f; i <= 2.0f; i += 0.025f) {
                gravityMultValues.Add(i.ToString("0.000"));
            }
            MenuListItem gravityMultList = new MenuListItem("Gravity multiplier", gravityMultValues, 20, "Modifies gravity constant, higher values makes drone fall quicker during freefall.") {
                ShowColorPanel = false
            };

            // Gravity recovery multiplier
            List<string> gravityRecoveryMultValues = new List<string>();
            for (float i = 0.5f; i <= 2.0f; i += 0.025f) {
                gravityRecoveryMultValues.Add(i.ToString("0.000"));
            }
            MenuListItem gravityRecoveryMultList = new MenuListItem("Gravity recovery multiplier", gravityRecoveryMultValues, 20, "How quickly drone stops falling after hitting acceleration.") {
                ShowColorPanel = false
            };

            // Drag multiplier
            List<string> dragMultValues = new List<string>();
            for (float i = 0.5f; i <= 2.0f; i += 0.025f) {
                dragMultValues.Add(i.ToString("0.000"));
            }
            MenuListItem dragMultList = new MenuListItem("Drag multiplier", dragMultValues, 20, "How much air ressistance there is - higher values make drone lose velocity quicker.") {
                ShowColorPanel = false
            };

            // Velocity multiplier
            List<string> velocityMultValues = new List<string>();
            for (float i = 0.5f; i <= 2.0f; i += 0.025f) {
                velocityMultValues.Add(i.ToString("0.000"));
            }
            MenuListItem velocityMultList = new MenuListItem("Velocity multiplier", velocityMultValues, 20, "How responsive drone is in terms of velocity.") {
                ShowColorPanel = false
            };

            // Rotation multipliers
            List<string> rotationMultXValues = new List<string>();
            for (float i = 0.5f; i <= 2.0f; i += 0.025f) {
                rotationMultXValues.Add(i.ToString("0.000"));
            }
            MenuListItem rotationMultXList = new MenuListItem("Pitch multiplier", rotationMultXValues, 20, "How responsive drone is in terms of rotation (pitch).") {
                ShowColorPanel = false
            };
            List<string> rotationMultYValues = new List<string>();
            for (float i = 0.5f; i <= 2.0f; i += 0.025f) {
                rotationMultYValues.Add(i.ToString("0.000"));
            }
            MenuListItem rotationMultYList = new MenuListItem("Roll multiplier", rotationMultYValues, 20, "How responsive drone is in terms of rotation (roll).") {
                ShowColorPanel = false
            };
            List<string> rotationMultZValues = new List<string>();
            for (float i = 0.5f; i <= 2.0f; i += 0.025f) {
                rotationMultZValues.Add(i.ToString("0.000"));
            }
            MenuListItem rotationMultZList = new MenuListItem("Yaw multiplier", rotationMultZValues, 20, "How responsive drone is in terms of rotation (yaw).") {
                ShowColorPanel = false
            };
            // Tilt angle
            List<string> tiltAngleValues = new List<string>();
            for (float i = 0.0f; i <= 45.0f; i += 5f) {
                tiltAngleValues.Add(i.ToString("0.0"));
            }
            MenuListItem tiltAngleList = new MenuListItem("Tilt angle", tiltAngleValues, 8, "Defines how much in camera tilted relative to the drone.") {
                ShowColorPanel = false
            };
            // FOV
            List<string> fovValues = new List<string>();
            for (float i = 30.0f; i <= 120.0f; i += 5f) {
                fovValues.Add(i.ToString("0.0"));
            }
            MenuListItem fovList = new MenuListItem("FOV", fovValues, 10, "Field of view of the camera") {
                ShowColorPanel = false
            };

            #endregion

            #region adding menu items

            menu.AddMenuItem(gravityMultList);
            menu.AddMenuItem(gravityRecoveryMultList);
            menu.AddMenuItem(dragMultList);
            menu.AddMenuItem(velocityMultList);
            menu.AddMenuItem(rotationMultXList);
            menu.AddMenuItem(rotationMultYList);
            menu.AddMenuItem(rotationMultZList);
            menu.AddMenuItem(tiltAngleList);
            menu.AddMenuItem(fovList);

            #endregion

            #region handling menu changes

            menu.OnListIndexChange += (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) => {
                if (_listItem == gravityMultList) {
                    gravityMult = _newIndex * 0.025f + 0.5f;
                }
                if (_listItem == gravityRecoveryMultList) {
                    gravityRecoveryMult = _newIndex * 0.025f + 0.5f;
                }
                if (_listItem == dragMultList) {
                    dragMult = _newIndex * 0.025f + 0.5f;
                }
                if (_listItem == velocityMultList) {
                    velocityMult = _newIndex * 0.025f + 0.5f;
                }

                if (_listItem == rotationMultXList) {
                    rotationMult.X = _newIndex * 0.025f + 0.5f;
                }
                if (_listItem == rotationMultYList) {
                    rotationMult.Y = _newIndex * 0.025f + 0.5f;
                }
                if (_listItem == rotationMultZList) {
                    rotationMult.Z = _newIndex * 0.025f + 0.5f;
                }
                
                if (_listItem == tiltAngleList) {
                    tiltAngle = _newIndex * 5.0f;
                }
                if (_listItem == fovList) {
                    droneFov = _newIndex * 5.0f + 30f;
                    SetCamFov(MainMenu.EnhancedCamMenu.droneCamera.Handle, droneFov);
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

        #region drone camera

        private DroneInfo drone;

        // Parameters for user to tune
        private static float gravityMult = 1.0f;
        private static float gravityRecoveryMult = 1.0f;
        private static float dragMult = 1.0f;
        private static Vector3 rotationMult = new Vector3(1.0f, 1.0f, 1.0f);
        private static float velocityMult = 1.0f;
        private static float tiltAngle = 40.0f;
        private static float droneFov = 80.0f;

        // Const drone parameters
        private const float GRAVITY_CONST = 10.8f;       // Gravity force constant ///9.8f
        private const float TIMESTEP_DELIMITER = 80.15f;   // Less - gravity is stronger ///60.15f
        private const float DRONE_DRAG = 0.0020f;        // Air resistance ///0.0015f
        private const float DRONE_AGILITY_ROT = 8.0f;   // How quick is rotational response of the drone ///6.5f
        private const float DRONE_AGILITY_VEL = 65f; // How quick is velocity and acceleration response ///30f
        private const float GRAVITY_RECOVERY_MULTIPLIER = 8.75f;   // How quickly can drone regain acceleration after free fall ///10.75f

        // Time of free fall, the longer fall the higher gravity down vector
        private static float freeFallTime = 0f;

        /// <summary>
        /// Changes main render camera behaviour, creates a free camera controlled
        /// like a drone.
        /// </summary>
        /// <returns></returns>
        private async Task RunDroneCam() {
            if (MainMenu.EnhancedCamMenu != null) {
                if (MainMenu.EnhancedCamMenu.DroneCam) {
                    if (MainMenu.EnhancedCamMenu.droneCamera != null) {
                        // Get user input
                        UpdateDroneControls();

                        // Update camera properties
                        UpdateDronePosition();
                        UpdateDroneRotation();
                    } else {
                        MainMenu.EnhancedCamMenu.ResetCameras();
                        MainMenu.EnhancedCamMenu.droneCamera = MainMenu.EnhancedCamMenu.CreateNonAttachedCamera();
                        MainMenu.EnhancedCamMenu.droneCamera.FieldOfView = droneFov;
                        World.RenderingCamera = MainMenu.EnhancedCamMenu.droneCamera;
                        MainMenu.EnhancedCamMenu.droneCamera.IsActive = true;
                        drone = new DroneInfo {
                            velocity = Vector3.Zero,
                            downVelocity = 0f,
                            rotation = new Quaternion(0f, 0f, 0f, 1f)
                        };
                        freeFallTime = 0f;
                    }
                }
            } else {
                await Delay(0);
            }
        }

        // Struct containing all the necessary info for tracking drone
        // movement.
        private struct DroneInfo {
            // User input
            public float acceleration;
            public float controlPitch;
            public float controlYaw;
            public float controlRoll;
            // Current values
            public Vector3 velocity;        // Drone's velocity in all directions
            public float downVelocity;      // Velocity caused by gravity
            public Quaternion rotation;     // Drone rotation in quaternion
        }

        private void DumpDebug() {
            Debug.WriteLine(drone.acceleration.ToString() +
                            drone.controlPitch.ToString() +
                            drone.controlYaw.ToString() +
                            drone.controlRoll.ToString() +
                            drone.velocity.ToString() +
                            drone.downVelocity.ToString() +
                            drone.rotation.X.ToString() +
                            drone.rotation.Y.ToString() +
                            drone.rotation.Z.ToString() +
                            drone.rotation.W.ToString() +
                            MainMenu.EnhancedCamMenu.droneCamera.Position.ToString()
                            );
        }

        // Get user input for drone camera
        private void UpdateDroneControls() {
            drone.acceleration = ((float)(GetControlValue(0, 71) / 255f) - 0.5f);
            drone.controlPitch = ((float)(GetControlValue(1, 2) / 255f) - 0.5f);
            drone.controlYaw = -((float)(GetControlValue(1, 9) / 255f) - 0.5f);
            drone.controlRoll = ((float)(GetControlValue(1, 1) / 255f) - 0.5f);

            // Account for mouse controls
            if (IsInputDisabled(1)) {
                drone.controlPitch *= 3.5f;
                drone.controlYaw *= 0.55f;
                drone.controlRoll *= 4.5f;
            }
        }

        // Update drone's rotation based on input
        private void UpdateDroneRotation() {

            // Calculate delta of rotation based on user input
            float deltaPitch = drone.controlPitch * DRONE_AGILITY_ROT * 0.75f * rotationMult.X;
            float deltaYaw = drone.controlYaw * DRONE_AGILITY_ROT * 0.7f * rotationMult.Z;
            float deltaRoll = drone.controlRoll * DRONE_AGILITY_ROT * 0.85f * rotationMult.Y;

            // Rotate quaternion
            drone.rotation *= Quaternion.RotationAxis(Vector3.Up, deltaRoll * EnhancedCamera.CamMath.DegToRad);
            drone.rotation *= Quaternion.RotationAxis(Vector3.Right, deltaPitch * EnhancedCamera.CamMath.DegToRad);
            drone.rotation *= Quaternion.RotationAxis(Vector3.ForwardLH, deltaYaw * EnhancedCamera.CamMath.DegToRad);

            // Update camera rotation based on values
            Vector3 eulerRot = EnhancedCamera.CamMath.QuaternionToEuler(drone.rotation);
            SetCamRot(MainMenu.EnhancedCamMenu.droneCamera.Handle, eulerRot.X, eulerRot.Y, eulerRot.Z, 2);
        }

        private void UpdateDronePosition() {
            float deltaTime = Timestep() / TIMESTEP_DELIMITER;

            // Calculate impact of gravity force
            freeFallTime += deltaTime;                    // Increase free fall time
            float normalizeGravity = (float)Math.Cos((double)(EnhancedCamera.CamMath.QuaternionToEuler(drone.rotation).Y * EnhancedCamera.CamMath.DegToRad));
            normalizeGravity *= (float)Math.Cos((double)(EnhancedCamera.CamMath.QuaternionToEuler(drone.rotation).X * EnhancedCamera.CamMath.DegToRad));
            normalizeGravity = (normalizeGravity < 0f) ? (0f) : (normalizeGravity);
            if (normalizeGravity.ToString() == "NaN") { normalizeGravity = 0f; }    // Gimbal lock fix
            freeFallTime -= ((drone.acceleration * GRAVITY_RECOVERY_MULTIPLIER * gravityRecoveryMult) * deltaTime * normalizeGravity);    // Free fall time is decreased when drone is accelerated
            freeFallTime = (freeFallTime < 0f) ? (0f) : (freeFallTime);
            drone.downVelocity = GRAVITY_CONST * gravityMult * freeFallTime;  // v = at

            float staticTilt = (float)Math.Tan((double)(tiltAngle * EnhancedCamera.CamMath.DegToRad));

            // Calculate velocity in each direction based on acceleration
            drone.velocity += MainMenu.EnhancedCamMenu.droneCamera.ForwardVector * drone.acceleration * DRONE_AGILITY_VEL * velocityMult * 0.5f * deltaTime;
            drone.velocity -= MainMenu.EnhancedCamMenu.droneCamera.UpVector * drone.acceleration * DRONE_AGILITY_VEL * velocityMult * (staticTilt / 2f) * deltaTime;
            // Acount for air resistance
            drone.velocity -= drone.velocity * DRONE_DRAG * dragMult;

            // Update camera position based on values
            Vector3 deltaPos = Vector3.ForwardLH * drone.downVelocity + drone.velocity;
            MainMenu.EnhancedCamMenu.droneCamera.Position -= deltaPos;
        }

        #endregion
    }
}