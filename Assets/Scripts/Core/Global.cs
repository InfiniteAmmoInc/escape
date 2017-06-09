using UnityEngine;
using System.Collections;

public class Global : MonoBehaviour
{
    public static Global instance;

    public static Player player;

    static Plane plane;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            plane = new Plane(-Vector3.forward, Vector3.zero);
        }
    }

    void Start()
    {
        InitScene();
    }

    void InitScene()
    {
        player = Object.FindObjectOfType<Player>();
    }

    public static Vector3 GetWorldPosition(Vector2 point, Camera camera = null)
    {
        if (camera == null)
            camera = Camera.main;

        if (camera)
        {
            Ray ray = camera.ScreenPointToRay(point);
            float distance = 0.0f;
            if (plane.Raycast(ray, out distance))
            {
                Vector3 vec = ray.GetPoint(distance);
                return vec;
            }
        }
        return Vector3.zero;
    }
}
