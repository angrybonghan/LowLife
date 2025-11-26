using UnityEngine;

public interface I_Attackable
{
    void OnAttack(Transform attackerPos);

    bool CanAttack(Transform attackerPos);
}

public interface I_Destructible
{
    void OnAttack();

    bool CanDestructible();
}

public interface I_Interactable
{
    void InInteraction();
}

public interface I_DialogueCallback
{
    void OnDialogueEnd();
}

public interface I_Projectile
{
    void Collision();
}

public interface I_TriggerBox
{
    void TriggerIn();
    void TriggerOut();
}