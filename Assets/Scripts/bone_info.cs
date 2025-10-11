using UnityEngine;

public class bone_info : MonoBehaviour
{
    [Header("2D計算結果")]
    [Tooltip("現在のXY平面上の速度 (m/s)")]
    public Vector2 velocity;

    [Tooltip("現在のZ軸周りの角速度 (deg/s)。反時計回りが正。")]
    public float angularVelocity;

    // 1フレーム前の状態を保存する変数
    private Vector2 lastPosition;
    private float lastZAngle;

    void Start()
    {
        // 最初のフレームの位置とZ軸角度を記録
        // transform.positionはVector3ですが、Vector2に代入すると自動的にXY成分が使われます
        lastPosition = transform.position;
        lastZAngle = transform.eulerAngles.z;
    }

    void Update()
    {
        Vector2 currentPosition = transform.position;
        var tracker = GetTrackingDatas.Instance;
        // ゼロ除算を防止
        if (lastPosition != currentPosition)
        {
            // --- 速度 (Vector2) の計算 ---
            Vector2 displacement = currentPosition - lastPosition;
            Vector2 v = displacement / tracker.RequestInterval;
            if (v != new Vector2(0.0f,0.0f)) velocity = v;

            // --- 角速度 (float) の計算 ---
            float currentZAngle = transform.eulerAngles.z;

            // Mathf.DeltaAngleを使い、角度が0/360度をまたぐ場合も正しく差分を計算します
            // これにより、例えば359度から1度への変化は+2度として正しく計算されます
            float deltaAngle = Mathf.DeltaAngle(lastZAngle, currentZAngle);

            // 角度の差分を経過時間で割り、角速度を求めます
            float a_v = deltaAngle / tracker.RequestInterval;
            if (a_v != 0.0f) angularVelocity = a_v;
        }

        // 次のフレームのために現在の値を保存
        lastPosition = transform.position;
        lastZAngle = transform.eulerAngles.z;
    }
}