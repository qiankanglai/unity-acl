using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayBy
{
    public int GetAnimationCount();
    public string GetAnimationName(int index);
    public void SelectAnimation(int index);
}
