using UnityEngine;

public interface I_Attackable
{
    void OnAttack(Transform attackerPos);

    bool CanAttack();
}

public interface I_Interactable
{
    void InInteraction();
}