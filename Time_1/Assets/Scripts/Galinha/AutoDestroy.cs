using UnityEngine;
using System.Collections;
public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 10f;

    private SpriteRenderer sr;
    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("AutoDestroy precisa de SpriteRenderer no mesmo GameObject!");
            Destroy(gameObject, 1.7f);
            return;
        }
        StartCoroutine(Animar());
    }
    private IEnumerator Animar()
    {
        float intervalo = 1f / fps;
        foreach (Sprite frame in frames)
        {
            sr.sprite = frame;
            yield return new WaitForSeconds(intervalo);
        }
        Destroy(gameObject);
    }
}