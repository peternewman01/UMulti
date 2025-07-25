using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PlayerInput))]
public class Table : Object
{
    public Recipe TempRecipe;
    public GameObject tempTargetPrefab;
    public override void Initialize()
    {
        objectID = (int)Objects.TABLE;
        objectName = "Table";
    }

    private void Start()
    {
        //temp version, will need to figure out recipies with ui
        Recipe stick = new Recipe();
        stick.recipe.Add(((int)Objects.WOOD, 5));

        stick.target = tempTargetPrefab.GetComponent<Stick>();
        stick.target.Initialize();
        TempRecipe = stick;

        Debug.Log(TempRecipe.target.ObjectName);
    }

    protected override void Interact()
    {
        TempRecipe.TryCraft();
    }

    private void Update()
    {
        if (playerInArea)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                TempRecipe.targetInvintory = targetInvintory;
                Interact();
            }
        }
    }
}
