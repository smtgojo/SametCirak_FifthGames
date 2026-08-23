using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "LeaderboardAnimSettings", menuName = "Scriptable Objects/LeaderboardAnimSettings")]
public class LeaderboardAnimSettings : ScriptableObject
{
    [Header("Entry")]
    public float introDelay = 0.5f;
    public float offsetPadding = 900f;
    public float offsetDelay = 0.01f;
    public float introDuration = 0.45f;
    public Ease introEase;

    [Header("RankUp")]
    public Ease moveEase;
    public Ease scaleUpEase;
    public Ease scaleDownEase;
    public float delayBeforeRankUp = 0.5f;
    public float rankUpScale = 1.1f;
    public float rankUpScaleDown = 0.9f;
    public float rankUpScaleDuration = 0.25f;
    public float rankUpScaleDownDuration = 0.2f;
    public float moveDuration = 0.5f;
    public float scrollDuration = 0.6f;
}
