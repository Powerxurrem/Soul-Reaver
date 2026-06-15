using System.Collections;
using UnityEngine;

public class SimpleScroller : MonoBehaviour
{
    public Transform bgA;
    public Transform bgB;

    public float speed = 1.5f;
    public float width = 16f;

    public bool isMoving = false;

    private Coroutine moveRoutine;

    void Update()
    {
        if (!isMoving) return;

        MoveBackground(bgA);
        MoveBackground(bgB);

        WrapIfNeeded(bgA, bgB);
        WrapIfNeeded(bgB, bgA);
    }

    public void StartMoving()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        isMoving = true;
    }

    public void StopMoving()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        isMoving = false;
    }

    public IEnumerator MoveForSeconds(float seconds)
    {
        if (moveRoutine != null)
            yield break;

        moveRoutine = StartCoroutine(MoveForSecondsRoutine(seconds));
        yield return moveRoutine;
    }

    private IEnumerator MoveForSecondsRoutine(float seconds)
    {
        isMoving = true;

        yield return new WaitForSeconds(seconds);

        isMoving = false;
        moveRoutine = null;
    }

    void MoveBackground(Transform bg)
    {
        if (bg == null) return;

        bg.position += Vector3.left * speed * Time.deltaTime;
    }

    void WrapIfNeeded(Transform current, Transform other)
    {
        if (current == null || other == null) return;

        if (current.position.x <= -width)
        {
            current.position = new Vector3(
                other.position.x + width,
                current.position.y,
                current.position.z
            );
        }
    }
}