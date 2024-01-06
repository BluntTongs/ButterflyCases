using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace museumcases.Utils
{
    public class MuseumCaseData
    {
        
        public int inventoryQuantity { get; set; }
        

        public float[] rotations = new float[4];
        public float[] vertrotations = new float[4];
        public float[] rotValue
        {
            get { return rotations; }
            set { rotations = value; }
        }

        public float[] verrotValue
        {
            get { return vertrotations; }
            set { vertrotations = value; }
        }

    }
}
