using UnityEngine;
using UnityEngine.UI;

public class UICutoutMask : Image
{
    public override Material materialForRendering
    {
        get
        {
            Material test = new Material(base.materialForRendering);
            test.SetInt("_StencilComp", 6);
            return test;
        }
    }
}
