﻿namespace Utilities.Unity
{
    // This class should be changed when you add a new layer in Unity
    public static class Layers
    {
        public const string NAME_DEFAULT = "Default";
        public const string NAME_TRANSPARENT_FX = "TransparentFX";
        public const string NAME_IGNORE_RAYCAST = "Ignore Raycast";
        public const string NAME_PLAYER = "Player";
        public const string NAME_WATER = "Water";
        public const string NAME_UI = "UI";
        public const string NAME_INTERACTION = "Interaction";
        public const string NAME_AI = "AI";
        public const string NAME_POST_PROCESSING = "PostProcessing";
        public const string NAME_NOR_IN_REFLEXION = "Not in Reflection";

        public const int DEFAULT = 0;
        public const int TRANSPARENT_FX = 1;
        public const int IGNORE_RAYCAST = 2;
        public const int PLAYER = 3;
        public const int WATER = 4;
        public const int UI = 5;
        public const int INTERACTION = 6;
        public const int AI = 7;
        public const int POST_PROCESSING = 8;
        public const int NOT_IN_REFLEXION = 9;

        public const int DEFAULT_MASK = 1 << 0;
        public const int TRANSPARENT_FX_MASK = 1 << 1;
        public const int IGNORE_RAYCAST_MASK = 1 << 2;
        public const int PLAYER_MASK = 1 << 3;
        public const int WATER_MASK = 1 << 4;
        public const int UI_MASK = 1 << 5;
        public const int INTERACTION_MASK = 1 << 6;
        public const int AI_MASK = 1 << 7;
        public const int POST_PROCESSING_MASK = 1 << 8;
        public const int NOT_IN_REFLEXION_MASK = 1 << 9;
        
        // ReSharper disable once InconsistentNaming Reason: I wanted all the elements of this file to have the same naming convention
        public static int GAMEPLAY_MASK => PLAYER_MASK | INTERACTION_MASK | AI_MASK;
    }
}