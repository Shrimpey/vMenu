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

        private const int MIN_VAL = 0;
        private const int MAX_VAL = 255;
        private const int STEP = 5;
        private const int MAX_SLIDER_VAL = MAX_VAL / STEP;

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
        MenuSliderItem primaryRedList = new MenuSliderItem("Primary R", "Red channel of car primary RGB color", MIN_VAL, MAX_SLIDER_VAL, MAX_SLIDER_VAL, false)
        {
            BarColor = System.Drawing.Color.FromArgb(200, 255, 0, 0),
            BackgroundColor = System.Drawing.Color.FromArgb(200, 75, 25, 25),
            ItemData = P_R_ID
        };
        MenuSliderItem primaryGreenList = new MenuSliderItem("Primary G", "Green channel of car primary RGB color", MIN_VAL, MAX_SLIDER_VAL, MAX_SLIDER_VAL, false)
        {
            BarColor = System.Drawing.Color.FromArgb(200, 0, 255, 0),
            BackgroundColor = System.Drawing.Color.FromArgb(200, 25, 75, 25),
            ItemData = P_G_ID
        };
        MenuSliderItem primaryBlueList = new MenuSliderItem("Primary B", "Blue channel of car primary RGB color", MIN_VAL, MAX_SLIDER_VAL, MAX_SLIDER_VAL, false)
        {
            BarColor = System.Drawing.Color.FromArgb(200, 0, 0, 255),
            BackgroundColor = System.Drawing.Color.FromArgb(200, 25, 25, 75),
            ItemData = P_B_ID
        };

        MenuSliderItem secondaryRedList = new MenuSliderItem("Secondary R", "Red channel of car secondary RGB color", MIN_VAL, MAX_SLIDER_VAL, MAX_SLIDER_VAL, false)
        {
            BarColor = System.Drawing.Color.FromArgb(200, 255, 0, 0),
            BackgroundColor = System.Drawing.Color.FromArgb(200, 75, 25, 25),
            ItemData = S_R_ID
        };
        MenuSliderItem secondaryGreenList = new MenuSliderItem("Secondary G", "Green channel of car secondary RGB color", MIN_VAL, MAX_SLIDER_VAL, MAX_SLIDER_VAL, false)
        {
            BarColor = System.Drawing.Color.FromArgb(200, 0, 255, 0),
            BackgroundColor = System.Drawing.Color.FromArgb(200, 25, 75, 25),
            ItemData = S_G_ID
        };
        MenuSliderItem secondaryBlueList = new MenuSliderItem("Secondary B", "Blue channel of car secondary RGB color", MIN_VAL, MAX_SLIDER_VAL, MAX_SLIDER_VAL, false)
        {
            BarColor = System.Drawing.Color.FromArgb(200, 0, 0, 255),
            BackgroundColor = System.Drawing.Color.FromArgb(200, 25, 25, 75),
            ItemData = S_B_ID
        };

        MenuSliderItem neonRedList = new MenuSliderItem("Neon R", "Red channel of car neon RGB color", MIN_VAL, MAX_SLIDER_VAL, MAX_SLIDER_VAL, false)
        {
            BarColor = System.Drawing.Color.FromArgb(200, 255, 0, 0),
            BackgroundColor = System.Drawing.Color.FromArgb(200, 75, 25, 25),
            ItemData = N_R_ID
        };
        MenuSliderItem neonGreenList = new MenuSliderItem("Neon G", "Green channel of car neon RGB color", MIN_VAL, MAX_SLIDER_VAL, MAX_SLIDER_VAL, false)
        {
            BarColor = System.Drawing.Color.FromArgb(200, 0, 255, 0),
            BackgroundColor = System.Drawing.Color.FromArgb(200, 25, 75, 25),
            ItemData = N_G_ID
        };
        MenuSliderItem neonBlueList = new MenuSliderItem("Neon B", "Blue channel of car neon RGB color", MIN_VAL, MAX_SLIDER_VAL, MAX_SLIDER_VAL, false)
        {
            BarColor = System.Drawing.Color.FromArgb(200, 0, 0, 255),
            BackgroundColor = System.Drawing.Color.FromArgb(200, 25, 25, 75),
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
                primary = new int[3] { MAX_VAL, MAX_VAL, MAX_VAL };
                secondary = new int[3] { MAX_VAL, MAX_VAL, MAX_VAL };
                neon = new int[3] { MAX_VAL, MAX_VAL, MAX_VAL };
            }
        }

        #endregion

        public void ResetGUI()
        {
            if (primaryRedList != null) { primaryRedList.Position = MAX_SLIDER_VAL; }
            if (primaryGreenList != null) { primaryGreenList.Position = MAX_SLIDER_VAL; }
            if (primaryBlueList != null) { primaryBlueList.Position = MAX_SLIDER_VAL; }

            if (secondaryRedList != null) { secondaryRedList.Position = MAX_SLIDER_VAL; }
            if (secondaryGreenList != null) { secondaryGreenList.Position = MAX_SLIDER_VAL; }
            if (secondaryBlueList != null) { secondaryBlueList.Position = MAX_SLIDER_VAL; }

            if (neonRedList != null) { neonRedList.Position = MAX_SLIDER_VAL; }
            if (neonGreenList != null) { neonGreenList.Position = MAX_SLIDER_VAL; }
            if (neonBlueList != null) { neonBlueList.Position = MAX_SLIDER_VAL; }

            menu.RefreshIndex();
        }

        public void RefreshGUI()
        {
            if (primaryRedList != null) { primaryRedList.Position = (int)Math.Ceiling(((double)currentRGB.GetPrimary()[0]) / STEP); primaryRedList.Text = "Primary R\t\t" + currentRGB.GetPrimary()[0].ToString(); }
            if (primaryGreenList != null) { primaryGreenList.Position = (int)Math.Ceiling(((double)currentRGB.GetPrimary()[1]) / STEP); primaryGreenList.Text = "Primary G\t\t" + currentRGB.GetPrimary()[1].ToString(); }
            if (primaryBlueList != null) { primaryBlueList.Position = (int)Math.Ceiling(((double)currentRGB.GetPrimary()[2]) / STEP); primaryBlueList.Text = "Primary B\t\t" + currentRGB.GetPrimary()[2].ToString(); }

            if (secondaryRedList != null) { secondaryRedList.Position = (int)Math.Ceiling(((double)currentRGB.GetSecondary()[0]) / STEP); secondaryRedList.Text = "Secondary R\t" + currentRGB.GetSecondary()[0].ToString(); }
            if (secondaryGreenList != null) { secondaryGreenList.Position = (int)Math.Ceiling(((double)currentRGB.GetSecondary()[1]) / STEP); secondaryGreenList.Text = "Secondary G\t" + currentRGB.GetSecondary()[1].ToString(); }
            if (secondaryBlueList != null) { secondaryBlueList.Position = (int)Math.Ceiling(((double)currentRGB.GetSecondary()[2]) / STEP); secondaryBlueList.Text = "Secondary B\t" + currentRGB.GetSecondary()[2].ToString(); }

            if (neonRedList != null) { neonRedList.Position = (int)Math.Ceiling(((double)currentRGB.GetNeon()[0]) / STEP); neonRedList.Text = "Neon R\t\t" + currentRGB.GetNeon()[0].ToString(); }
            if (neonGreenList != null) { neonGreenList.Position = (int)Math.Ceiling(((double)currentRGB.GetNeon()[1]) / STEP); neonGreenList.Text = "Neon G\t\t" + currentRGB.GetNeon()[1].ToString(); }
            if (neonBlueList != null) { neonBlueList.Position = (int)Math.Ceiling(((double)currentRGB.GetNeon()[2]) / STEP); neonBlueList.Text = "Neon B\t\t" + currentRGB.GetNeon()[2].ToString(); }
        }

        private void RefreshGUIText()
        {

            if (primaryRedList != null) { primaryRedList.Text = "Primary R\t\t" + currentRGB.GetPrimary()[0].ToString(); }
            if (primaryGreenList != null) { primaryGreenList.Text = "Primary G\t\t" + currentRGB.GetPrimary()[1].ToString(); }
            if (primaryBlueList != null) { primaryBlueList.Text = "Primary B\t\t" + currentRGB.GetPrimary()[2].ToString(); }

            if (secondaryRedList != null) { secondaryRedList.Text = "Secondary R\t" + currentRGB.GetSecondary()[0].ToString(); }
            if (secondaryGreenList != null) { secondaryGreenList.Text = "Secondary G\t" + currentRGB.GetSecondary()[1].ToString(); }
            if (secondaryBlueList != null) { secondaryBlueList.Text = "Secondary B\t" + currentRGB.GetSecondary()[2].ToString(); }

            if (neonRedList != null) { neonRedList.Text = "Neon R\t\t" + currentRGB.GetNeon()[0].ToString(); }
            if (neonGreenList != null) { neonGreenList.Text = "Neon G\t\t" + currentRGB.GetNeon()[1].ToString(); }
            if (neonBlueList != null) { neonBlueList.Text = "Neon B\t\t" + currentRGB.GetNeon()[2].ToString(); }
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
                        currentRGB.SetPrimaryR((newPos != MAX_SLIDER_VAL) ? (newPos * STEP) : (MAX_VAL));
                        SetColors(veh.Handle);
                    }
                    else
                    if (item == primaryGreenList)
                    {
                        currentRGB.SetPrimaryG((newPos != MAX_SLIDER_VAL) ? (newPos * STEP) : (MAX_VAL));
                        SetColors(veh.Handle);
                    }
                    else
                    if (item == primaryBlueList)
                    {
                        currentRGB.SetPrimaryB((newPos != MAX_SLIDER_VAL) ? (newPos * STEP) : (MAX_VAL));
                        SetColors(veh.Handle);
                    }
                    else
                    #endregion

                    #region secondary colors
                    if (item == secondaryRedList)
                    {
                        currentRGB.SetSecondaryR((newPos != MAX_SLIDER_VAL) ? (newPos * STEP) : (MAX_VAL));
                        SetColors(veh.Handle);
                    }
                    else
                    if (item == secondaryGreenList)
                    {
                        currentRGB.SetSecondaryG((newPos != MAX_SLIDER_VAL) ? (newPos * STEP) : (MAX_VAL));
                        SetColors(veh.Handle);
                    }
                    else
                    if (item == secondaryBlueList)
                    {
                        currentRGB.SetSecondaryB((newPos != MAX_SLIDER_VAL) ? (newPos * STEP) : (MAX_VAL));
                        SetColors(veh.Handle);
                    }
                    else
                    #endregion

                    #region neon colors
                    if (item == neonRedList)
                    {
                        currentRGB.SetNeonR((newPos != MAX_SLIDER_VAL) ? (newPos * STEP) : (MAX_VAL));
                        SetColors(veh.Handle);
                    }
                    else
                    if (item == neonGreenList)
                    {
                        currentRGB.SetNeonG((newPos != MAX_SLIDER_VAL) ? (newPos * STEP) : (MAX_VAL));
                        SetColors(veh.Handle);
                    }
                    else
                    if (item == neonBlueList)
                    {
                        currentRGB.SetNeonB((newPos != MAX_SLIDER_VAL) ? (newPos * STEP) : (MAX_VAL));
                        SetColors(veh.Handle);
                    }
                    #endregion

                    RefreshGUIText();
                }
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
                if (result >= MIN_VAL && result <= MAX_VAL)
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