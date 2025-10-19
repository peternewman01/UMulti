using UnityEngine;
using UseEntity;

namespace UseEntity {
    public abstract class Interactable : Entity
    {
        public abstract void Interact(PlayerManager interacter);
    }
}