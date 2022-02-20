using UnityEngine;
using UnityEngine.InputSystem;

namespace Utilities.Extensions
{
    public static class InputActionExtensions
    {
        public static bool ReadBool(this InputAction action) => action.ReadValue<float>() != 0;
        public static float ReadFloat(this InputAction action) => action.ReadValue<float>();
        public static Vector2 ReadV2(this InputAction action) => action.ReadValue<Vector2>();
    }
}