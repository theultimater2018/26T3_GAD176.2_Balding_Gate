using UnityEngine;

public class PromptBounce : MonoBehaviour
{

    public Transform targetPos;

    public GameObject target;

    public SpriteRenderer spriteRenderer;
    public Vector2 pos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            targetPos.position = new Vector2(targetPos.position.x, targetPos.position.y + 0.5f);
            spriteRenderer.enabled = true;
            pos = targetPos.position;
            target.transform.position = pos;
        }
    }

}
