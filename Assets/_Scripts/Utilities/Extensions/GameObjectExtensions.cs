﻿using UnityEngine;

namespace Utils.Extensions
{
    public static class GameObjectExtensions
    {
        public static int GetNumberOfComponents(this GameObject gameObject)
        {
            return gameObject.GetComponentsInChildren<Component>().Length - 1;
        }
    }
}