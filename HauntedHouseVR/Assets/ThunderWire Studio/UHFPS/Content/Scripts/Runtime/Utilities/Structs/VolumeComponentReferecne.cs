using System;
using UnityEngine.Rendering;

namespace UHFPS.Runtime
{
    [Serializable]
    public struct VolumeComponentReferecne
    {
        public Volume Volume;
        public int ComponentIndex;
    }
}