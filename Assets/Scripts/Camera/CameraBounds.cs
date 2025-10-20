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
            Debug.LogError("请指定背景图的 SpriteRenderer！");
            return;
        }

        // 动态生成边界
        UpdateBackgroundBounds();
    }

    public void UpdateBackgroundBounds()
    {
        Bounds backgroundBounds = backgroundSprite.bounds;

        Vector2[] vertices = new Vector2[4];
        vertices[0] = new Vector2(backgroundBounds.min.x, backgroundBounds.min.y); // 左下
        vertices[1] = new Vector2(backgroundBounds.max.x, backgroundBounds.min.y); // 右下
        vertices[2] = new Vector2(backgroundBounds.max.x, backgroundBounds.max.y); // 右上
        vertices[3] = new Vector2(backgroundBounds.min.x, backgroundBounds.max.y); // 左上

        polygonCollider.points = vertices;

        transform.position = backgroundBounds.center;
    }
}
