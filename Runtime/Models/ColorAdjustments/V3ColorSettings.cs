#nullable enable
using UnityEngine;

namespace net.puk06.TexStackEditor.Models
{
    [System.Serializable]
    public class V3ColorSettings
    {
        public Gradient Gradient = new Gradient();
        [Range(2, 2048)] public int Resolution = 1024;
    }
}
