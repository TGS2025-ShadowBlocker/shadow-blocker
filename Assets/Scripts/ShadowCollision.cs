using UnityEngine;
public class ShadowCollison : MonoBehaviour
{

    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject bone;
    [SerializeField] private float bone_Thickness;
    private GameObject[] bones;
    private string[] landmark_names = {
                            "nose", "left_eye_inner", "left_eye", "left_eye_outer",
                            "right_eye_inner", "right_eye", "right_eye_outer",
                            "left_ear", "right_ear", "mouth_left", "mouth_right",
                            "left_shoulder", "right_shoulder", "left_elbow", "right_elbow",
                            "left_wrist", "right_wrist", "left_pinky", "right_pinky",
                            "left_index", "right_index", "left_thumb", "right_thumb",
                            "left_hip", "right_hip", "left_knee", "right_knee",
                            "left_ankle", "right_ankle", "left_heel", "right_heel",
                            "left_foot_index", "right_foot_index"
                        };
    private Vector2Int[] bones_order =
    {
        // ----- 顔 -----
        new Vector2Int(0, 1),   // 鼻 → 左目（内）
        new Vector2Int(1, 2),   // 左目（内） → 左目
        new Vector2Int(2, 3),   // 左目 → 左目（外）
        new Vector2Int(3, 7),   // 左目（外） → 左耳
        new Vector2Int(0, 4),   // 鼻 → 右目（内）
        new Vector2Int(4, 5),   // 右目（内） → 右目
        new Vector2Int(5, 6),   // 右目 → 右目（外）
        new Vector2Int(6, 8),   // 右目（外） → 右耳
        new Vector2Int(9, 10),  // 左口角 → 右口角

        // ----- 胴体 -----
        new Vector2Int(11, 12), // 左肩 → 右肩
        new Vector2Int(11, 23), // 左肩 → 左腰
        new Vector2Int(12, 24), // 右肩 → 右腰
        new Vector2Int(23, 24), // 左腰 → 右腰

        // ----- 左腕 -----
        new Vector2Int(11, 13), // 左肩 → 左肘
        new Vector2Int(13, 15), // 左肘 → 左手首
        new Vector2Int(15, 17), // 左手首 → 左小指
        new Vector2Int(15, 19), // 左手首 → 左人差し指
        new Vector2Int(15, 21), // 左手首 → 左親指
        new Vector2Int(17, 19), // 左小指 → 左人差し指

        // ----- 右腕 -----
        new Vector2Int(12, 14), // 右肩 → 右肘
        new Vector2Int(14, 16), // 右肘 → 右手首
        new Vector2Int(16, 18), // 右手首 → 右小指
        new Vector2Int(16, 20), // 右手首 → 右人差し指
        new Vector2Int(16, 22), // 右手首 → 右親指
        new Vector2Int(18, 20), // 右小指 → 右人差し指

        // ----- 左足 -----
        new Vector2Int(23, 25), // 左腰 → 左膝
        new Vector2Int(25, 27), // 左膝 → 左足首
        new Vector2Int(27, 29), // 左足首 → 左かかと
        new Vector2Int(27, 31), // 左足首 → 左足指
        new Vector2Int(29, 31), // 左かかと → 左足指

        // ----- 右足 -----
        new Vector2Int(24, 26), // 右腰 → 右膝
        new Vector2Int(26, 28), // 右膝 → 右足首
        new Vector2Int(28, 30), // 右足首 → 右かかと
        new Vector2Int(28, 32), // 右足首 → 右足指
        new Vector2Int(30, 32), // 右かかと → 右足指
    };

    void Awake()
    {
        bones = new GameObject[bones_order.Length];
        for(int i=0;i<bones_order.Length;i++)
        {
            bones[i] = Instantiate(bone , transform);
        }
    }

    void Update()
    {
        var tracker = GetTrackingDatas.Instance;
        Vector3 PointA = new Vector3(0, 0, 0);
        Vector3 PointB = new Vector3(0, 0, 0);
        for (int i = 0; i < bones_order.Length; i++)
        {
            float A_x = tracker.LandmarksData[landmark_names[bones_order[i].x]].x;
            float A_y = tracker.LandmarksData[landmark_names[bones_order[i].x]].y;
            PointA = mainCamera.ViewportToWorldPoint(new Vector3(A_x, A_y, 0.0f));
            float B_x = tracker.LandmarksData[landmark_names[bones_order[i].y]].x;
            float B_y = tracker.LandmarksData[landmark_names[bones_order[i].y]].y;
            PointB = mainCamera.ViewportToWorldPoint(new Vector3(B_x, B_y, 0.0f));

            bones[i].transform.position = (PointA + PointB) / 2;
            float distance = Vector2.Distance(PointA, PointB);
            Vector2 direction = PointB - PointA;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bones[i].transform.rotation = Quaternion.Euler(0, 0, angle);
            bones[i].GetComponent<BoxCollider2D>().size = new Vector2(distance, bone_Thickness);
        }
    }
}