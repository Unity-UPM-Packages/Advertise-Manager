using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLegends.Base.Ads
{
    public enum AdsEvents
    {
        None,
        LoadRequest,
        LoadAvailable,
        LoadFail,
        LoadNotAvailable,
        ShowSuccess,
        ShowFail,
        Close,
        Click,
        Cancel,
    }
}

