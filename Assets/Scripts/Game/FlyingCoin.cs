using UnityEngine;

public class FlyingCoin : MonoBehaviour
{
    public GameManager gameManager;
    private Rigidbody2D rb;
    public float velocity = 1;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            rb.velocity = Vector2.up * velocity;
            rb.transform.Rotate(0f, 0f, -10f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        gameManager.GameOver();
    }
}
