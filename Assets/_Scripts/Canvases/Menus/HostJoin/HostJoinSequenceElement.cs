using System.Collections.Generic;
using Canvases.Components;
using UnityEngine;

namespace Canvases.Matchmaking
{
    public class HostJoinSequenceElement : MonoBehaviour
    {
        private const int EMPTY_VALUE = -1;
        
        [SerializeField] private ImageUIComponent emptyImage;
        [SerializeField] private List<ImageUIComponent> filledImages;

        private int currentValue = EMPTY_VALUE;

        public int NumberOfIcons => filledImages.Count;
        public bool IsEmpty => CurrentValue == EMPTY_VALUE;

        public int CurrentValue
        {
            get => currentValue;
            set
            {
                currentValue = value;
                UpdateVisual();
            }
        }

        private void Start()
        {
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            HideAllImages();
            if (CurrentValue == EMPTY_VALUE)
            {
                if (emptyImage)
                    emptyImage.Show();
            }
            else
            {
                if (CurrentValue < filledImages.Count)
                    filledImages[CurrentValue].Show();
            }
        }

        private void HideAllImages()
        {
            if (emptyImage)
                emptyImage.Hide();

            foreach (var filledImage in filledImages)
            {
                filledImage.Hide();
            }
        }

        public void Empty()
        {
            CurrentValue = EMPTY_VALUE;
        }
    }
}