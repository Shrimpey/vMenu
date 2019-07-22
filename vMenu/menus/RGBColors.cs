using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Newtonsoft.Json;
using CitizenFX.Core;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static vMenuClient.CommonFunctions;

namespace vMenuClient
{
    public class RGBColors : BaseScript
    {
        #region variables

        private Menu menu;

        private const string P_R_ID = "RGBcols_p_r";
        private const string P_G_ID = "RGBcols_p_g";
        private const string P_B_ID = "RGBcols_p_b";
        private const string S_R_ID = "RGBcols_s_r";
        private const string S_G_ID = "RGBcols_s_g";
        private const string S_B_ID = "RGBcols_s_b";
        private const string N_R_ID = "RGBcols_n_r";
        private const string N_G_ID = "RGBcols_n_g";
        private const string N_B_ID = "RGBcols_n_b";

        public static CarRGBColors currentRGB = new CarRGBColors();

        MenuSliderItem primaryRedList = new MenuSliderItem("Primary R", "Red channel of car primary RGB color", 0, 64, 64, false)
        {
            BarColor = System.Drawing.Color.FromArgb(255, 255, 0, 0),
            BackgroundColor = System.Drawing.Color.FromArgb(255, 75, 25, 25),
            ItemData = P_R_ID
        };
        MenuSliderItem primaryGreenList = new MenuSliderItem("Primary G", "Green channel of car primary RGB color", 0, 64, 64, false)
        {
            BarColor = System.Drawing.Color.FromArgb(255, 0, 255, 0),
            BackgroundColor = System.Drawing.Color.FromArgb(255, 25, 75, 25),
            ItemData = P_G_ID
        };
        MenuSliderItem primaryBlueList = new MenuSliderItem("Primary B", "Blue channel of car primary RGB color", 0, 64, 64, false)
        {
            BarColor = System.Drawing.Color.FromArgb(255, 0, 0, 255),
            BackgroundColor = System.Drawing.Color.FromArgb(255, 25, 25, 75),
            ItemData = P_B_ID
        };

        MenuSliderItem secondaryRedList = new MenuSliderItem("Secondary R", "Red channel of car secondary RGB color", 0, 64, 64, false)
        {
            BarColor = System.Drawing.Color.FromArgb(255, 255, 0, 0),
            BackgroundColor = System.Drawing.Color.FromArgb(255, 75, 25, 25),
            ItemData = S_R_ID
        };
        MenuSliderItem secondaryGreenList = new MenuSliderItem("Secondary G", "Green channel of car secondary RGB color", 0, 64, 64, false)
        {
            BarColor = System.Drawing.Color.FromArgb(255, 0, 255, 0),
            BackgroundColor = System.Drawing.Color.FromArgb(255, 25, 75, 25),
            ItemData = S_G_ID
        };
        MenuSliderItem secondaryBlueList = new MenuSliderItem("Secondary B", "Blue channel of car secondary RGB color", 0, 64, 64, false)
        {
            BarColor = System.Drawing.Color.FromArgb(255, 0, 0, 255),
            BackgroundColor = System.Drawing.Color.FromArgb(255, 25, 25, 75),
            ItemData = S_B_ID
        };

        MenuSliderItem neonRedList = new MenuSliderItem("Neon R", "Red channel of car neon RGB color", 0, 64, 64, false)
        {
            BarColor = System.Drawing.Color.FromArgb(255, 255, 0, 0),
            BackgroundColor = System.Drawing.Color.FromArgb(255, 75, 25, 25),
            ItemData = N_R_ID
        };
        MenuSliderItem neonGreenList = new MenuSliderItem("Neon G", "Green channel of car neon RGB color", 0, 64, 64, false)
        {
            BarColor = System.Drawing.Color.FromArgb(255, 0, 255, 0),
            BackgroundColor = System.Drawing.Color.FromArgb(255, 25, 75, 25),
            ItemData = N_G_ID
        };
        MenuSliderItem neonBlueList = new MenuSliderItem("Neon B", "Blue channel of car neon RGB color", 0, 64, 64, false)
        {
            BarColor = System.Drawing.Color.FromArgb(255, 0, 0, 255),
            BackgroundColor = System.Drawing.Color.FromArgb(255, 25, 25, 75),
            ItemData = N_B_ID
        };

        public static bool shouldSave = false;

        public struct CarRGBColors
        {
            private int[] primary;
            private int[] secondary;
            private int[] neon;

            public void SetPrimaryR(int red) { primary[0] = red; }
            public void SetPrimaryG(int green) { primary[1] = green; }
            public void SetPrimaryB(int blue) { primary[2] = blue; }

            public void SetSecondaryR(int red) { secondary[0] = red; }
            public void SetSecondaryG(int green) { secondary[1] = green; }
            public void SetSecondaryB(int blue) { secondary[2] = blue; }

            public void SetNeonR(int red) { neon[0] = red; }
            public void SetNeonG(int green) { neon[1] = green; }
            public void SetNeonB(int blue) { neon[2] = blue; }

            public int[] GetPrimary() { return primary; }
            public int[] GetSecondary() { return secondary; }
            public int[] GetNeon() { return neon; }

            public void ResetRGB()
            {
                primary = new int[3] { 255, 255, 255 };
                secondary = new int[3] { 255, 255, 255 };
                neon = new int[3] { 255, 255, 255 };
            }
        }

        #endregion

        public void ResetGUI()
        {
            if (primaryRedList != null) { primaryRedList.Position = 64; }
            if (primaryGreenList != null) { primaryGreenList.Position = 64; }
            if (primaryBlueList != null) { primaryBlueList.Position = 64; }

            if (secondaryRedList != null) { secondaryRedList.Position = 64; }
            if (secondaryGreenList != null) { secondaryGreenList.Position = 64; }
            if (secondaryBlueList != null) { secondaryBlueList.Position = 64; }

            if (neonRedList != null) { neonRedList.Position = 64; }
            if (neonGreenList != null) { neonGreenList.Position = 64; }
            if (neonBlueList != null) { neonBlueList.Position = 64; }

            menu.RefreshIndex();
        }

        public void RefreshGUI()
        {
            if (primaryRedList != null) { primaryRedList.Position = currentRGB.GetPrimary()[0] / 4; primaryRedList.Text = "Primary R\t\t" + currentRGB.GetPrimary()[0].ToString(); }
            if (primaryGreenList != null) { primaryGreenList.Position = currentRGB.GetPrimary()[1] / 4; primaryGreenList.Text = "Primary G\t\t" + currentRGB.GetPrimary()[1].ToString(); }
            if (primaryBlueList != null) { primaryBlueList.Position = currentRGB.GetPrimary()[2] / 4; primaryBlueList.Text = "Primary B\t\t" + currentRGB.GetPrimary()[2].ToString(); }

            if (secondaryRedList != null) { secondaryRedList.Position = currentRGB.GetSecondary()[0] / 4; secondaryRedList.Text = "Secondary R\t" + currentRGB.GetSecondary()[0].ToString(); }
            if (secondaryGreenList != null) { secondaryGreenList.Position = currentRGB.GetSecondary()[1] / 4; secondaryGreenList.Text = "Secondary G\t" + currentRGB.GetSecondary()[1].ToString(); }
            if (secondaryBlueList != null) { secondaryBlueList.Position = currentRGB.GetSecondary()[2] / 4; secondaryBlueList.Text = "Secondary B\t" + currentRGB.GetSecondary()[2].ToString(); }

            if (neonRedList != null) { neonRedList.Position = currentRGB.GetNeon()[0] / 4; neonRedList.Text = "Neon R\t\t" + currentRGB.GetNeon()[0].ToString(); }
            if (neonGreenList != null) { neonGreenList.Position = currentRGB.GetNeon()[1] / 4; neonGreenList.Text = "Neon G\t\t" + currentRGB.GetNeon()[1].ToString(); }
            if (neonBlueList != null) { neonBlueList.Position = currentRGB.GetNeon()[2] / 4; neonBlueList.Text = "Neon B\t\t" + currentRGB.GetNeon()[2].ToString(); }
        }

        private void SetColors(int vehicleHandle)
        {
            SetVehicleCustomPrimaryColour(vehicleHandle, currentRGB.GetPrimary()[0], currentRGB.GetPrimary()[1], currentRGB.GetPrimary()[2]);
            SetVehicleCustomSecondaryColour(vehicleHandle, currentRGB.GetSecondary()[0], currentRGB.GetSecondary()[1], currentRGB.GetSecondary()[2]);
            SetVehicleNeonLightsColour(vehicleHandle, currentRGB.GetNeon()[0], currentRGB.GetNeon()[1], currentRGB.GetNeon()[2]);

            shouldSave = true;
        }

        private void CreateMenu()
        {
            // Create the menu.
            menu = new Menu("RGB Menu", "Advanced RGB car colors");
            currentRGB = new CarRGBColors();
            currentRGB.ResetRGB();

            #region add menu items

            menu.AddMenuItem(primaryRedList);
            menu.AddMenuItem(primaryGreenList);
            menu.AddMenuItem(primaryBlueList);

            menu.AddMenuItem(secondaryRedList);
            menu.AddMenuItem(secondaryGreenList);
            menu.AddMenuItem(secondaryBlueList);

            menu.AddMenuItem(neonRedList);
            menu.AddMenuItem(neonGreenList);
            menu.AddMenuItem(neonBlueList);

            ResetGUI();
            shouldSave = false;

            #endregion

            #region handle slider changes

            menu.OnSliderPositionChange += (sender, item, oldPos, newPos, itemIndex) =>
            {
                Vehicle veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    #region primary colors
                    if (item == primaryRedList)
                    {
                        currentRGB.SetPrimaryR((newPos != 64) ? (newPos * 4) : (255));
                        SetColors(veh.Handle);
                    }
                    else
                    if (item == primaryGreenList)
                    {
                        currentRGB.SetPrimaryG((newPos != 64) ? (newPos * 4) : (255));
                        SetColors(veh.Handle);
                    }
                    else
                    if (item == primaryBlueList)
                    {
                        currentRGB.SetPrimaryB((newPos != 64) ? (newPos * 4) : (255));
                        SetColors(veh.Handle);
                    }
                    else
                    #endregion

                    #region secondary colors
                    if (item == secondaryRedList)
                    {
                        currentRGB.SetSecondaryR((newPos != 64) ? (newPos * 4) : (255));
                        SetColors(veh.Handle);
                    }
                    else
                    if (item == secondaryGreenList)
                    {
                        currentRGB.SetSecondaryG((newPos != 64) ? (newPos * 4) : (255));
                        SetColors(veh.Handle);
                    }
                    else
                    if (item == secondaryBlueList)
                    {
                        currentRGB.SetSecondaryB((newPos != 64) ? (newPos * 4) : (255));
                        SetColors(veh.Handle);
                    }
                    else
                    #endregion

                    #region neon colors
                    if (item == neonRedList)
                    {
                        currentRGB.SetNeonR((newPos != 64) ? (newPos * 4) : (255));
                        SetColors(veh.Handle);
                    }
                    else
                    if (item == neonGreenList)
                    {
                        currentRGB.SetNeonG((newPos != 64) ? (newPos * 4) : (255));
                        SetColors(veh.Handle);
                    }
                    else
                    if (item == neonBlueList)
                    {
                        currentRGB.SetNeonB((newPos != 64) ? (newPos * 4) : (255));
                        SetColors(veh.Handle);
                    }
                    #endregion
                }

                RefreshGUI();
            };

            menu.OnSliderItemSelect += async (menu, sliderItem, sliderPosition, itemIndex) =>
            {
                if (sliderItem.ItemData as string == P_R_ID)
                {
                    string newVal = await GetUserInput(windowTitle: "Enter primary color R channel value (0-255)", maxInputLength: 3);
                    if (GetUserChannelValue(newVal, out int result))
                    {
                        currentRGB.SetPrimaryR(result);
                    }
                }else if (sliderItem.ItemData as string == P_G_ID)
                {
                    string newVal = await GetUserInput(windowTitle: "Enter primary color G channel value (0-255)", maxInputLength: 3);
                    if (GetUserChannelValue(newVal, out int result))
                    {
                        currentRGB.SetPrimaryG(result);
                    }
                }
                else if(sliderItem.ItemData as string == P_B_ID)
                {
                    string newVal = await GetUserInput(windowTitle: "Enter primary color B channel value (0-255)", maxInputLength: 3);
                    if (GetUserChannelValue(newVal, out int result))
                    {
                        currentRGB.SetPrimaryB(result);
                    }
                }else if (sliderItem.ItemData as string == S_R_ID)
                {
                    string newVal = await GetUserInput(windowTitle: "Enter secondary color R channel value (0-255)", maxInputLength: 3);
                    if (GetUserChannelValue(newVal, out int result))
                    {
                        currentRGB.SetSecondaryR(result);
                    }
                }
                else if (sliderItem.ItemData as string == S_G_ID)
                {
                    string newVal = await GetUserInput(windowTitle: "Enter secondary color G channel value (0-255)", maxInputLength: 3);
                    if (GetUserChannelValue(newVal, out int result))
                    {
                        currentRGB.SetSecondaryG(result);
                    }
                }
                else if (sliderItem.ItemData as string == S_B_ID)
                {
                    string newVal = await GetUserInput(windowTitle: "Enter secondary color B channel value (0-255)", maxInputLength: 3);
                    if (GetUserChannelValue(newVal, out int result))
                    {
                        currentRGB.SetSecondaryB(result);
                    }
                }
                else if (sliderItem.ItemData as string == N_R_ID)
                {
                    string newVal = await GetUserInput(windowTitle: "Enter neon color R channel value (0-255)", maxInputLength: 3);
                    if (GetUserChannelValue(newVal, out int result))
                    {
                        currentRGB.SetNeonR(result);
                    }
                }
                else if (sliderItem.ItemData as string == N_G_ID)
                {
                    string newVal = await GetUserInput(windowTitle: "Enter neon color G channel value (0-255)", maxInputLength: 3);
                    if (GetUserChannelValue(newVal, out int result))
                    {
                        currentRGB.SetNeonG(result);
                    }
                }
                else if (sliderItem.ItemData as string == N_B_ID)
                {
                    string newVal = await GetUserInput(windowTitle: "Enter neon color B channel value (0-255)", maxInputLength: 3);
                    if (GetUserChannelValue(newVal, out int result))
                    {
                        currentRGB.SetNeonB(result);
                    }
                }

                // Set color and refresh GUI
                Vehicle veh = GetVehicle();
                if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
                {
                    SetColors(veh.Handle);
                }
                RefreshGUI();
            };
            
            #endregion
        }

        private bool GetUserChannelValue(string value, out int result)
        {
            bool success = int.TryParse(value, out result);
            if (string.IsNullOrEmpty(value) || !success)
            {
                Notify.Error(CommonErrors.InvalidInput);
                return false;
            }
            else
            {
                if (result >= 0 && result < 256)
                {
                    return true;
                }
                else
                {
                    Notify.Error(CommonErrors.InvalidInput);
                    return false;
                }
            }
        }

        /// <summary>
        /// Creates the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public Menu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}