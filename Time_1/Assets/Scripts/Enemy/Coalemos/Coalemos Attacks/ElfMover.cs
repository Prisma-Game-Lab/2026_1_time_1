using UnityEngine;

// Added at runtime by CoalemosElf. Moves the elf via transform so it ignores all physics walls.
public class ElfMover : MonoBehaviour
{
    private float dirX;
    private float speed;

    public void Init(float dirX, float speed)
    {
        this.dirX  = dirX;
        this.speed = speed;
    }

    void Update()
    {
        transform.position += Vector3.right * (dirX * speed * Time.deltaTime);
    }
}
