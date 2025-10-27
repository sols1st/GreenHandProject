using UnityEngine;
using System.Linq; // 添加这一行以使用LINQ排序

public class InfiniteBackgroundScroller : MonoBehaviour
{
    public Transform[] backgrounds;
    public Rigidbody2D playerRigidbody;
    private float bgWidth;
    private float screenWidth;

    void Start()
    {
        if (backgrounds.Length < 2)
        {
            Debug.LogError("请确保backgrounds数组中至少有2个对象！");
            return;
        }

        SpriteRenderer sr = backgrounds[0].GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            bgWidth = sr.bounds.size.x;
        }
        else
        {
            Debug.LogError("background对象需要有SpriteRenderer组件！");
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.orthographic)
        {
            screenWidth = mainCamera.orthographicSize * 2 * mainCamera.aspect;
        }
        else
        {
            Debug.LogError("需要使用正交相机！");
            return;
        }

        for (int i = 0; i < backgrounds.Length; i++)
        {
            backgrounds[i].position = new Vector3(i * bgWidth, backgrounds[i].position.y, backgrounds[i].position.z);
        }
    }

    void Update()
    {
        if (playerRigidbody == null) return;
        CheckAndReposition();
    }

    private void CheckAndReposition()
    {
        Camera mainCamera = Camera.main;
        float camLeft = mainCamera.transform.position.x - screenWidth / 2;
        float camRight = mainCamera.transform.position.x + screenWidth / 2;

        Transform[] sortedBgs = backgrounds.OrderBy(bg => bg.position.x).ToArray();

        Transform leftmost = sortedBgs[0];
        if (leftmost.position.x + bgWidth / 2 < camLeft - bgWidth)        {
            float rightmostX = sortedBgs[sortedBgs.Length - 1].position.x;
            leftmost.position = new Vector3(rightmostX + bgWidth, leftmost.position.y, leftmost.position.z);
        }

        Transform rightmost = sortedBgs[sortedBgs.Length - 1];
        if (rightmost.position.x - bgWidth / 2 > camRight + bgWidth)
        {
            float leftmostX = sortedBgs[0].position.x;
            rightmost.position = new Vector3(leftmostX - bgWidth, rightmost.position.y, rightmost.position.z);
        }
    }
}