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

        private static bool HoverMode = false;
        private MenuCheckboxItem hoverMode;

        // Constructor
        public DroneCam() {
            Tick += RunDroneCam;
        }

        private void CreateMenu() {
            menu = new Menu(Game.Player.Name, "Drone Camera parameters");

            #region main parameters

            // Hover mode checkbox
            hoverMode = new MenuCheckboxItem("Enable hover mode", "Camera behaves more like a stabilized drone for filmmaking rather than racing drone.", false);
            
            // Gravity multiplier
            List<string> gravityMultValues = new List<string>();
            for (float i = 0.5f; i <= 4.0f; i += 0.050f) {
                gravityMultValues.Add(i.ToString("0.000"));
            }
            MenuListItem gravityMultList = new MenuListItem("Gravity multiplier", gravityMultValues, 10, "Modifies gravity constant, higher values makes drone fall quicker during freefall.") {
                ShowColorPanel = false
            };

            // Timestep multiplier
            List<string> timestepValues = new List<string>();
            for (float i = 0.5f; i <= 4.0f; i += 0.050f) {
                timestepValues.Add(i.ToString("0.000"));
            }
            MenuListItem timestepMultList = new MenuListItem("Timestep multiplier", timestepValues, 10, "Affects gravity and drone responsiveness.") {
                ShowColorPanel = false
            };

            // Drag multiplier
            List<string> dragMultValues = new List<string>();
            for (float i = 0.0f; i <= 4.0f; i += 0.050f) {
                dragMultValues.Add(i.ToString("0.000"));
            }
            MenuListItem dragMultList = new MenuListItem("Drag multiplier", dragMultValues, 20, "How much air ressistance there is - higher values make drone lose velocity quicker.") {
                ShowColorPanel = false
            };

            // Acceleration multiplier
            List<string> accelerationMultValues = new List<string>();
            for (float i = 0.5f; i <= 4.0f; i += 0.050f) {
                accelerationMultValues.Add(i.ToString("0.000"));
            }
            MenuListItem accelerationMultList = new MenuListItem("Acceleration multiplier", accelerationMultValues, 10, "How responsive drone is in terms of acceleration.") {
                ShowColorPanel = false
            };

            // Rotation multipliers
            List<string> rotationMultXValues = new List<string>();
            for (float i = 0.5f; i <= 4.0f; i += 0.050f) {
                rotationMultXValues.Add(i.ToString("0.000"));
            }
            MenuListItem rotationMultXList = new MenuListItem("Pitch multiplier", rotationMultXValues, 10, "How responsive drone is in terms of rotation (pitch).") {
                ShowColorPanel = false
            };
            List<string> rotationMultYValues = new List<string>();
            for (float i = 0.5f; i <= 4.0f; i += 0.050f) {
                rotationMultYValues.Add(i.ToString("0.000"));
            }
            MenuListItem rotationMultYList = new MenuListItem("Roll multiplier", rotationMultYValues, 10, "How responsive drone is in terms of rotation (roll).") {
                ShowColorPanel = false
            };
            List<string> rotationMultZValues = new List<string>();
            for (float i = 0.5f; i <= 4.0f; i += 0.050f) {
                rotationMultZValues.Add(i.ToString("0.000"));
            }
            MenuListItem rotationMultZList = new MenuListItem("Yaw multiplier", rotationMultZValues, 10, "How responsive drone is in terms of rotation (yaw).") {
                ShowColorPanel = false
            };
            // Tilt angle
            List<string> tiltAngleValues = new List<string>();
            for (float i = 0.0f; i <= 80.0f; i += 5f) {
                tiltAngleValues.Add(i.ToString("0.0"));
            }
            MenuListItem tiltAngleList = new MenuListItem("Tilt angle", tiltAngleValues, 9, "Defines how much is camera tilted relative to the drone.") {
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
            // Max velocity
            List<string> maxVelValues = new List<string>();
            for (float i = 10.0f; i <= 50.0f; i += 1f) {
                maxVelValues.Add(i.ToString("0.0"));
            }
            MenuListItem maxVelList = new MenuListItem("Max velocity", maxVelValues, 20, "Max velocity of the drone") {
                ShowColorPanel = false
            };

            #endregion

            #region adding menu items

            menu.AddMenuItem(hoverMode);

            menu.AddMenuItem(gravityMultList);
            menu.AddMenuItem(timestepMultList);
            menu.AddMenuItem(dragMultList);
            menu.AddMenuItem(accelerationMultList);
            menu.AddMenuItem(maxVelList);
            menu.AddMenuItem(rotationMultXList);
            menu.AddMenuItem(rotationMultYList);
            menu.AddMenuItem(rotationMultZList);
            menu.AddMenuItem(tiltAngleList);
            menu.AddMenuItem(fovList);

            #endregion

            #region handling menu changes

            // Handle checkbox
            menu.OnCheckboxChange += (_menu, _item, _index, _checked) => {
                if (_item == hoverMode) {
                    HoverMode = _checked;
                }
            };

            // Handle sliders
            menu.OnListIndexChange += (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) => {
                if (_listItem == gravityMultList) {
                    gravityMult = _newIndex * 0.05f + 0.5f;
                }
                if (_listItem == timestepMultList) {
                    timestepMult = _newIndex * 0.05f + 0.5f;
                }
                if (_listItem == dragMultList) {
                    dragMult = _newIndex * 0.05f;
                }
                if (_listItem == accelerationMultList) {
                    accelerationMult = _newIndex * 0.05f + 0.5f;
                }

                if (_listItem == rotationMultXList) {
                    rotationMult.X = _newIndex * 0.05f + 0.5f;
                }
                if (_listItem == rotationMultYList) {
                    rotationMult.Y = _newIndex * 0.05f + 0.5f;
                }
                if (_listItem == rotationMultZList) {
                    rotationMult.Z = _newIndex * 0.05f + 0.5f;
                }

                if (_listItem == maxVelList) {
                    maxVel = _newIndex * 1f + 10f;
                }

                if (_listItem == tiltAngleList) {
                    tiltAngle = _newIndex * 5.0f;
                }
                if (_listItem == fovList) {
                    droneFov = _newIndex * 5.0f + 30f;
                    if (MainMenu.EnhancedCamMenu.droneCamera != null) {
                        SetCamFov(MainMenu.EnhancedCamMenu.droneCamera.Handle, droneFov);
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

        #region drone camera

        private DroneInfo drone;

        // Parameters for user to tune
        private static float gravityMult = 1.0f;
        private static float timestepMult = 1.0f;
        private static float dragMult = 1.0f;
        private static Vector3 rotationMult = new Vector3(1f, 1f, 1f);
        private static float accelerationMult = 1f;
        private static float tiltAngle = 45.0f;
        private static float droneFov = 80.0f;
        private static float maxVel = 30.0f;

        // Const drone parameters
        private const float GRAVITY_CONST = 9.8f;       // Gravity force constant
        private const float TIMESTEP_DELIMITER = 90.15f;   // Less - gravity is stronger
        private const float DRONE_DRAG = 0.0020f;        // Air resistance
        private const float DRONE_AGILITY_ROT = 55000f;   // How quick is rotational response of the drone
        private const float DRONE_AGILITY_VEL = 210f; // How quick is velocity and acceleration response
        private const float DRONE_MAX_VELOCITY = 0.01f; // Max velocity of the drone

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
                        MainMenu.EnhancedCamMenu.droneCamera.IsActive = true;
                        drone = new DroneInfo {
                            velocity = Vector3.Zero,
                            downVelocity = 0f,
                            rotation = new Quaternion(0f, 0f, 0f, 1f)
                        };
                        Game.Player.CanControlCharacter = false;
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
            public float deceleration;
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
            drone.acceleration = ((GetDisabledControlNormal(0, 71)) / 2f);
            drone.deceleration = ((GetDisabledControlNormal(0, 72)) / 2f);
            drone.controlPitch = ((GetDisabledControlNormal(1, 2)) / 2f);
            drone.controlYaw = -((GetDisabledControlNormal(1, 9)) / 2f);
            drone.controlRoll = ((GetDisabledControlNormal(1, 1)) / 2f);

            // Account for mouse controls
            if (IsInputDisabled(1)) {
                drone.controlPitch *= 3.5f;
                drone.controlYaw *= 0.55f;
                drone.controlRoll *= 4.5f;
            }
        }

        // Update drone's rotation based on input
        private void UpdateDroneRotation() {
            float deltaTime = timestepMult * Timestep() / TIMESTEP_DELIMITER;

            // Calculate delta of rotation based on user input
            float deltaPitch = drone.controlPitch * DRONE_AGILITY_ROT * 0.70f * rotationMult.X * deltaTime;
            float deltaYaw = drone.controlYaw * DRONE_AGILITY_ROT * 0.6f * rotationMult.Z * deltaTime;
            float deltaRoll = drone.controlRoll * DRONE_AGILITY_ROT * 0.75f * rotationMult.Y * deltaTime;

            // Rotate quaternion
            drone.rotation *= Quaternion.RotationAxis(Vector3.Up, deltaRoll * EnhancedCamera.CamMath.DegToRad);
            drone.rotation *= Quaternion.RotationAxis(Vector3.Right, deltaPitch * EnhancedCamera.CamMath.DegToRad);
            drone.rotation *= Quaternion.RotationAxis(Vector3.ForwardLH, deltaYaw * EnhancedCamera.CamMath.DegToRad);

            // Update camera rotation based on values
            Vector3 eulerRot = EnhancedCamera.CamMath.QuaternionToEuler(drone.rotation);
            SetCamRot(MainMenu.EnhancedCamMenu.droneCamera.Handle, eulerRot.X, eulerRot.Y, eulerRot.Z, 2);
        }

        // Implementation of drone's physics engine
        private void UpdateDronePosition() {
            // For dividing velocity into two vectors based on camera tilt
            // compared to drone itself
            float staticTilt = Tan(tiltAngle);

            // Timeframe used for calculations
            float deltaTime = timestepMult * Timestep() / TIMESTEP_DELIMITER;

            // Calculate impact of gravity force
            float deltaDownForce = GRAVITY_CONST * gravityMult;      // F = m*a = m*g

            // Calculate velocity based on acceleration
            // Drone is tilted compared to camera, so there are two vectors
            // Forward and up are opposite due to naming conventions mismatch
            float deltaVelocityForward = drone.acceleration * DRONE_AGILITY_VEL * accelerationMult * 0.5f * deltaTime;          // dV = a*dt
            float deltaVelocityUp = drone.acceleration * DRONE_AGILITY_VEL * accelerationMult * (staticTilt / 2f) * deltaTime;  // dV = a*dt
            // Enable deceleration when in hover mode and get rid of gravity force
            if (HoverMode) {
                deltaVelocityForward -= drone.deceleration * DRONE_AGILITY_VEL * accelerationMult * 0.5f * deltaTime;
                deltaVelocityUp += drone.deceleration * DRONE_AGILITY_VEL * accelerationMult * (staticTilt / 2f) * deltaTime;
                deltaDownForce = 0f;
            }

            drone.velocity += MainMenu.EnhancedCamMenu.droneCamera.ForwardVector * deltaVelocityForward;    // V1 = V0 + dV
            drone.velocity -= MainMenu.EnhancedCamMenu.droneCamera.UpVector * deltaVelocityUp;              // V1 = V0 + dV
            // Account for air resistance
            drone.velocity -= drone.velocity * DRONE_DRAG * dragMult;
            drone.velocity += Vector3.ForwardLH * deltaDownForce * deltaTime;

            // Clamp velocity to maximum with some smoothing
            if (Math.Abs(drone.velocity.Length()) > maxVel * DRONE_MAX_VELOCITY) {
                drone.velocity = Vector3.Lerp(drone.velocity, drone.velocity * maxVel * DRONE_MAX_VELOCITY / drone.velocity.Length(), 0.1f);
            }

            // Update camera position based on velocity values
            MainMenu.EnhancedCamMenu.droneCamera.Position -= drone.velocity;
        }

        #endregion
    }
}