using System;

namespace RichAmbiance.Vehicles
{
    //Credit to Stealth22 for this struct
    /// <summary>
    /// Struct for vehicles primary and secondary colors
    /// </summary>
    internal struct VehicleColor
    {
        /// <summary>
        /// The primary color paint index 
        /// </summary>
        internal EPaint PrimaryColor { get; set; }

        /// <summary>
        /// The secondary color paint index 
        /// </summary>
        internal EPaint SecondaryColor { get; set; }


        /// <summary>
        /// Gets the primary color name
        /// </summary>
        internal string PrimaryColorName
        {
            get { return GetColorName(PrimaryColor); }
        }
        /// <summary>
        /// Gets the secondary color name
        /// </summary>
        internal string SecondaryColorName
        {
            get { return GetColorName(SecondaryColor); }
        }


        /// <summary>
        /// Gets the color name
        /// </summary>
        /// <param name="paint">Color to get the name from</param>
        /// <returns></returns>
        internal string GetColorName(EPaint paint)
        {
            String name = Enum.GetName(typeof(EPaint), paint);
            return name.Replace("_", " ");
        }
    }
}