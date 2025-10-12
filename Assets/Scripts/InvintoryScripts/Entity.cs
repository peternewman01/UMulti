using System;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UseEntity {
    [RequireComponent(typeof(NetworkObject))]
    public class Entity : NetworkBehaviour
    {
        //TODO: Spawn / despawn entity (networking)
    }
}


