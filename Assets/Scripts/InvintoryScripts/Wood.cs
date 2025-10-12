using UnityEngine;


namespace UseEntity
{
    public class Wood : Grabbable
    {
        public override void Interact(PlayerManager interacter)
        {
            interacter.GetInventory().AddItem(new ItemData(item, 1));
            Destroy(gameObject);
        }
    }
}
