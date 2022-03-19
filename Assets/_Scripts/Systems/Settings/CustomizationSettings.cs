using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Settings
{
    [CreateAssetMenu(menuName = "Settings/Customization Settings")]
    public class CustomizationSettings : ScriptableObject
    {
        private const int NUMBER_OF_MATERIALS_HAIR = 4;
        private const int NUMBER_OF_MATERIALS_CLOTHES = 6;

        [SerializeField, TableList] private List<HeadElement> headElements = new List<HeadElement>();
        [SerializeField, TableList] private List<EyeElement> eyeElements = new List<EyeElement>();
        [SerializeField, TableList] private List<SkinElement> skinElements = new List<SkinElement>();
        [SerializeField, TableList] private List<ClothesColorElement> clothesColorElements = new List<ClothesColorElement>();

        public int NumberOfHeadElements => headElements.Count;
        public int NumberOfHairColors => NUMBER_OF_MATERIALS_HAIR;
        public int NumberOfEyeElements => eyeElements.Count;
        public int NumberOfSkinElements => skinElements.Count;
        public int NumberOfClothesColorElements => clothesColorElements.Count;

        public GameObject GetHeadElementPrefab(int index)
        {
            Debug.Assert(index >= 0 && index < headElements.Count);

            return headElements[index].ModelPrefab;
        }

        public Material GetHairMaterial(int headElementIndex, int hairMaterialIndex)
        {
            Debug.Assert(headElementIndex >= 0 && headElementIndex < headElements.Count);
            Debug.Assert(hairMaterialIndex >= 0 && hairMaterialIndex < NUMBER_OF_MATERIALS_HAIR);

            return headElements[headElementIndex].GetHairMaterial(hairMaterialIndex);
        }

        public int GetHairMaterialIndex(int headElementIndex)
        {
            Debug.Assert(headElementIndex >= 0 && headElementIndex < headElements.Count);

            return headElements[headElementIndex].HairMaterialIndex;
        }

        public GameObject GetFacePrefabForEyes(int eyeIndex)
        {
            Debug.Assert(eyeIndex >= 0 && eyeIndex < eyeElements.Count);
            return eyeElements[eyeIndex].FacePrefab;
        }

        public GameObject GetNosePrefabForEyes(int eyeIndex)
        {
            Debug.Assert(eyeIndex >= 0 && eyeIndex < eyeElements.Count);
            return eyeElements[eyeIndex].NosePrefab;
        }

        public GameObject GetLeftEyePrefabForEyes(int eyeIndex)
        {
            Debug.Assert(eyeIndex >= 0 && eyeIndex < eyeElements.Count);
            return eyeElements[eyeIndex].LeftEyePrefab;
        }

        public GameObject GetRightEyePrefabForEyes(int eyeIndex)
        {
            Debug.Assert(eyeIndex >= 0 && eyeIndex < eyeElements.Count);
            return eyeElements[eyeIndex].RightEyePrefab;
        }

        public GameObject GetAltLeftEyePrefabForEyes(int eyeIndex)
        {
            Debug.Assert(eyeIndex >= 0 && eyeIndex < eyeElements.Count);
            return eyeElements[eyeIndex].LeftAltEyePrefab;
        }

        public GameObject GetAltRightEyePrefabForEyes(int eyeIndex)
        {
            Debug.Assert(eyeIndex >= 0 && eyeIndex < eyeElements.Count);
            return eyeElements[eyeIndex].RightAltEyePrefab;
        }

        public Material GetSkin(int skinIndex)
        {
            Debug.Assert(skinIndex >= 0 && skinIndex < skinElements.Count);
            return skinElements[skinIndex].SkinMaterial;
        }
        
        public Material GetClothesColor(int clothesColorIndex)
        {
            Debug.Assert(clothesColorIndex >= 0 && clothesColorIndex < clothesColorElements.Count);
            return clothesColorElements[clothesColorIndex].ClothesColorMaterial;
        }

        [Serializable]
        private class HeadElement
        {
            [SerializeField] private string name;
            
            [PreviewField]
            [ValidateInput(nameof(ValidateModelPrefab), "The prefab must contain a mesh renderer")]
            [SerializeField] private GameObject modelPrefab;

            // I don't use a fancy resizable list here because it would complexify the customisation logic too much
            // (I want to reduce dev time at this point of the project) and we already know we won't go over
            // 4 materials per hair
            [VerticalGroup("Textures"), PreviewField] 
            [SerializeField] private Material hairMaterial1, hairMaterial2, hairMaterial3, hairMaterial4;

            [FormerlySerializedAs("hairTextureIndex")]
            [Tooltip("The index of the material to change on the model for the hair (starts at 0)")]
            [SerializeField] private int hairMaterialIndex;
            
            public GameObject ModelPrefab => modelPrefab;
            public int HairMaterialIndex => hairMaterialIndex;

            public Material GetHairMaterial(int index)
            {
                return index switch
                {
                    0 => hairMaterial1,
                    1 => hairMaterial2,
                    2 => hairMaterial3,
                    3 => hairMaterial4,
                    _ => throw new IndexOutOfRangeException()
                };
            }
            
            private bool ValidateModelPrefab()
            {
                return modelPrefab == null || modelPrefab.GetComponentInChildren<Renderer>() != null;
            }
        }
        
        [Serializable]
        private class EyeElement
        {
            [SerializeField] private string name;
            
            [PreviewField]
            [SerializeField] private GameObject facePrefab;
            
            [PreviewField]
            [SerializeField] private GameObject nosePrefab;

            [VerticalGroup("EyePrefabs"), PreviewField] 
            [SerializeField] private GameObject leftEyePrefab, rightEyePrefab;
            
            [VerticalGroup("AltEyePrefabs"), PreviewField] 
            [SerializeField] private GameObject leftAltEyePrefab, rightAltEyePrefab;

            public GameObject FacePrefab => facePrefab;
            public GameObject NosePrefab => nosePrefab;
            public GameObject LeftEyePrefab => leftEyePrefab;
            public GameObject RightEyePrefab => rightEyePrefab;
            public GameObject LeftAltEyePrefab => leftAltEyePrefab;
            public GameObject RightAltEyePrefab => rightAltEyePrefab;
        }
        
        [Serializable]
        private class SkinElement
        {
            [SerializeField] private string name;
            
            [PreviewField]
            [SerializeField] private Material skinMaterial;

            public Material SkinMaterial => skinMaterial;
        }
        
        [Serializable]
        private class ClothesColorElement
        {
            [SerializeField] private string name;
            
            [PreviewField]
            [SerializeField] private Material clothesColorMaterial;

            public Material ClothesColorMaterial => clothesColorMaterial;
        }
    }
}