using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Coffee.UIExtensions;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private LeaderboardAnimSettings settings;
    [SerializeField] private VerticalLayoutGroup layout;
    [SerializeField] private RectTransform[] rows;
    [SerializeField] private TMP_Text[] rankTexts;
    [SerializeField] private UIParticle leftBurstParticle;
    [SerializeField] private UIParticle rightBurstParticle;
    [SerializeField] private UIParticle rankParticle;

    private float[] slotY;
    private float[] slotX;
    private RectTransform layoutRect;

    private RectTransform[] initialRows;
    private TMP_Text[] initialTexts;
    private Vector2 initialLayoutPos;
    private Sequence current;
    private Tween delayTween;

    private float OffsetX => ((RectTransform)layoutRect.root).rect.width + settings.offsetPadding;

    private void Start()
    {
        layoutRect = (RectTransform)layout.transform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRect);
        layout.enabled = false;

        slotY = new float[rows.Length];
        slotX = new float[rows.Length];
        for (int i = 0; i < rows.Length; i++)
        {
            slotY[i] = rows[i].anchoredPosition.y;
            slotX[i] = rows[i].anchoredPosition.x;
        }

        initialRows = (RectTransform[])rows.Clone();
        initialTexts = (TMP_Text[])rankTexts.Clone();
        initialLayoutPos = layoutRect.anchoredPosition;

        Play();
    }

    private void Play()
    {
        current = Intro();
        current.OnComplete(() =>
            delayTween = DOVirtual.DelayedCall(settings.delayBeforeRankUp, () => current = RankUp(5, 3)));
    }

    public void Replay()
    {
        ResetState();
        Play();
    }

    private void ResetState()
    {
        current?.Kill();
        delayTween?.Kill();
        DOTween.Kill(layoutRect);
        foreach (var r in rows) DOTween.Kill(r);

        rows = (RectTransform[])initialRows.Clone();
        rankTexts = (TMP_Text[])initialTexts.Clone();
        layoutRect.anchoredPosition = initialLayoutPos;

        for (int i = 0; i < rows.Length; i++)
        {
            rows[i].SetSiblingIndex(i);
            rows[i].anchoredPosition = new Vector2(slotX[i], slotY[i]);
            rows[i].localScale = Vector3.one;
            rankTexts[i].text = (i + 1).ToString();
        }

        if (leftBurstParticle) leftBurstParticle.Stop();
        if (rightBurstParticle) rightBurstParticle.Stop();
        if (rankParticle) rankParticle.Stop();
    }

    private Sequence Intro()
    {
        float offset = OffsetX;
        var seq = DOTween.Sequence();
        for (int i = 0; i < rows.Length; i++)
        {
            rows[i].anchoredPosition = new Vector2(slotX[i] + offset, slotY[i]);
            seq.Insert(i * settings.offsetDelay,
                rows[i].DOAnchorPosX(slotX[i], settings.introDuration).SetEase(settings.introEase));
        }
        return seq.SetDelay(settings.introDelay);
    }

    private Sequence RankUp(int from, int to)
    {
        int f = from - 1, t = to - 1;
        var moving = rows[f];
        var movingText = rankTexts[f];

        for (int i = f; i > t; i--)
        {
            rows[i] = rows[i - 1];
            rankTexts[i] = rankTexts[i - 1];
        }
        rows[t] = moving;
        rankTexts[t] = movingText;
        moving.SetAsLastSibling();

        var seq = DOTween.Sequence();

        for (int i = t; i <= f; i++)
            seq.Insert(0f, rows[i].DOAnchorPosY(slotY[i], settings.moveDuration)
                                  .SetEase(settings.moveEase))
                .Join(moving.DOScale(settings.rankUpScale, settings.rankUpScaleDuration)
                            .SetEase(settings.scaleUpEase));

        float scrollAmount = Mathf.Abs(slotY[t] - slotY[f]);
        seq.Insert(0f, layoutRect.DOAnchorPosY(
            layoutRect.anchoredPosition.y - scrollAmount, settings.scrollDuration)
            .SetEase(settings.moveEase));

        seq.InsertCallback(settings.moveDuration * 0.5f, RefreshRanks);

        seq.Insert(settings.moveDuration, moving.DOScale(settings.rankUpScaleDown, settings.rankUpScaleDownDuration)
            .SetEase(settings.scaleDownEase).OnComplete(() =>
            {
                leftBurstParticle.Play();
                rightBurstParticle.Play();
            }));

        seq.Append(moving.DOScale(1f, settings.rankUpScaleDuration).SetEase(settings.scaleUpEase).OnComplete(() =>
        {
            rankParticle.Play();
        }));

        return seq;
    }

    private void RefreshRanks()
    {
        for (int i = 0; i < rankTexts.Length; i++)
            rankTexts[i].text = (i + 1).ToString();
    }
}