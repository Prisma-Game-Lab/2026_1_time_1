using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnhangaScream : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    [Header("Action Names")]
    [SerializeField] private string moveActionName  = "Move";
    [SerializeField] private string throwActionName = "Throw";
    [SerializeField] private string healActionName  = "Curar";

    [Header("References")]
    [SerializeField] private GameObject  confusedEffect;
    [SerializeField] private Animator    hudAnimator;
    [SerializeField] private string      confusedStateName = "Confused";
    [SerializeField] private string      normalStateName   = "Normal";
    [SerializeField] private AudioSource screamAudioSource;

    [Header("Scream Attack")]
    [SerializeField] private float     screamDuration = 2f;
    [SerializeField] private AudioClip screamClip;

    public bool IsAttacking { get; private set; }
    public bool IsConfused  => movesRemaining > 0;

    private int movesRemaining;
    private InputAction moveAction;
    private InputAction throwAction;
    private InputAction healAction;

    private void Awake()
    {
        if (playerInput == null) return;
        moveAction  = playerInput.actions[moveActionName];
        throwAction = playerInput.actions[throwActionName];
        healAction  = playerInput.actions[healActionName];
    }

    public void Scream(int movesDuration)
    {
        StartCoroutine(ScreamRoutine(movesDuration));
    }

    private IEnumerator ScreamRoutine(int movesDuration)
    {
        IsAttacking = true;

        if (screamAudioSource != null && screamClip != null)
        {
            screamAudioSource.clip = screamClip;
            screamAudioSource.Play();
        }

        bool wasConfused = IsConfused;
        movesRemaining = movesDuration;
        if (!wasConfused)
        {
            ApplySwap();
            if (confusedEffect != null) confusedEffect.SetActive(true);
            if (hudAnimator != null) hudAnimator.SetBool(confusedStateName, true);
        }

        yield return new WaitForSeconds(screamDuration);

        if (screamAudioSource != null) screamAudioSource.Stop();
        IsAttacking = false;
    }

    public void OnMoveCompleted()
    {
        if (movesRemaining <= 0) return;
        movesRemaining--;
        if (movesRemaining <= 0)
        {
            RestoreBindings();
            if (confusedEffect != null) confusedEffect.SetActive(false);
            if (hudAnimator != null) hudAnimator.SetBool(confusedStateName, false);
        }
    }

    private void ApplySwap()
    {
        SwapMoveDirections(moveAction);
        SwapTwoActions(throwAction, healAction);
    }

    private void RestoreBindings()
    {
        moveAction?.RemoveAllBindingOverrides();
        throwAction?.RemoveAllBindingOverrides();
        healAction?.RemoveAllBindingOverrides();
    }

    // Swaps opposite composite pairs (left↔right, up↔down) of a 2D Vector action.
    // Handles multiple composites (e.g. WASD + Arrow Keys both get inverted).
    private static void SwapMoveDirections(InputAction action)
    {
        SwapNamedPair(action, "left",  "right");
        SwapNamedPair(action, "up",    "down");
    }

    private static void SwapNamedPair(InputAction action, string nameA, string nameB)
    {
        var indicesA = new List<int>();
        var indicesB = new List<int>();

        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (action.bindings[i].name == nameA) indicesA.Add(i);
            if (action.bindings[i].name == nameB) indicesB.Add(i);
        }

        int count = Mathf.Min(indicesA.Count, indicesB.Count);
        for (int i = 0; i < count; i++)
        {
            string pathA = action.bindings[indicesA[i]].effectivePath;
            string pathB = action.bindings[indicesB[i]].effectivePath;
            action.ApplyBindingOverride(indicesA[i], pathB);
            action.ApplyBindingOverride(indicesB[i], pathA);
        }
    }

    // Swaps all bindings between two actions index-by-index.
    private static void SwapTwoActions(InputAction a, InputAction b)
    {
        var aPaths = new string[a.bindings.Count];
        var bPaths = new string[b.bindings.Count];
        for (int i = 0; i < a.bindings.Count; i++) aPaths[i] = a.bindings[i].effectivePath;
        for (int i = 0; i < b.bindings.Count; i++) bPaths[i] = b.bindings[i].effectivePath;

        int aCount = Mathf.Min(a.bindings.Count, bPaths.Length);
        int bCount = Mathf.Min(b.bindings.Count, aPaths.Length);
        for (int i = 0; i < aCount; i++) a.ApplyBindingOverride(i, bPaths[i]);
        for (int i = 0; i < bCount; i++) b.ApplyBindingOverride(i, aPaths[i]);
    }

    private void OnDisable()
    {
        if (IsConfused) RestoreBindings();
    }
}
