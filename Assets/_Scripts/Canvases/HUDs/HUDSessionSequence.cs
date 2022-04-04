using Canvases.Matchmaking;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Systems.Network;
using UnityEngine;

public class HUDSessionSequence : MonoBehaviour
{
    List<string> sessionName;
    [SerializeField, Required] private List<HostJoinSequenceElement> elements;

    void Start()
    {
         sessionName = NetworkSystem.Instance.NetworkRunner.SessionInfo.Name.Split('-').ToList();

        if (!elements.Any() || sessionName == null)
        {
            Debug.LogWarning("Missing components. The lobby name will not be displayed.");
            return;
        }

        if (elements.Count < sessionName.Count)
        {
            Debug.LogWarning("The lobby name is larger than the number of sequence elements in the UI. The lobby name will not be displayed.");
            return;
        }

        for (int i = 0; i < sessionName.Count; i++)
        {
            elements[i].CurrentValue = int.Parse(sessionName[i]);
        }
    }
}
