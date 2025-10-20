using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class BackgroundBoundsSetter : MonoBehaviour
{
    public SpriteRenderer backgroundSprite;

    private PolygonCollider2D polygonCollider;

    void Start()
    {
        polygonCollider = GetComponent<PolygonCollider2D>();

        if (backgroundSprite == null)
        {
            Debug.LogError("��ָ������ͼ�� SpriteRenderer��");
            return;
        }

        // ��̬���ɱ߽�
        UpdateBackgroundBounds();
    }

    public void UpdateBackgroundBounds()
    {
        Bounds backgroundBounds = backgroundSprite.bounds;

        Vector2[] vertices = new Vector2[4];
        vertices[0] = new Vector2(backgroundBounds.min.x, backgroundBounds.min.y); // ����
        vertices[1] = new Vector2(backgroundBounds.max.x, backgroundBounds.min.y); // ����
        vertices[2] = new Vector2(backgroundBounds.max.x, backgroundBounds.max.y); // ����
        vertices[3] = new Vector2(backgroundBounds.min.x, backgroundBounds.max.y); // ����

        polygonCollider.points = vertices;

        transform.position = backgroundBounds.center;
    }
}
