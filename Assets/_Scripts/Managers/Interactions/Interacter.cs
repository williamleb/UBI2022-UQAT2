using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Managers.Interactions
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Interacter : NetworkBehaviour
    {
        private List<Interaction> interactionsInReach = new List<Interaction>();

        // TODO PlayerInteracter: grab only nearest + show prompt if active player
        // TODO AIInteracter: grab whatever (pass an ID)
        
        public void Interact()
        {
            // TODO Interact with nearest valid interaction
        }

        private void OnTriggerEnter(Collider other)
        {
            throw new NotImplementedException();
        }

        private void OnTriggerExit(Collider other)
        {
            throw new NotImplementedException();
        }
    }
}