This is a addon to [Hurricane VR 1.97.1](https://assetstore.unity.com/packages/tools/physics/hurricane-vr-physics-interaction-toolkit-177300#releases)

When you snap rotate and teleport, your hands and the items in them are falling behind. I don't like this so wrote some code that freezes your hands so they can follow you as you move.

Before:
https://www.youtube.com/watch?v=SX333_ONn68

After:
https://www.youtube.com/watch?v=XubQFUjpZ-I

How to use:
1. Add HandHelper.cs to `Assets\HurricaneVR\Framework\Scripts`
2. Replace HandleSnapRotation() in `HurricaneVR\Framework\Scripts\Core\Player\HVRPlayerController.cs` with
```cs
private void HandleSnapRotation()
{
    StartCoroutine(HandleSnapRotation_Addon());
}
private IEnumerator HandleSnapRotation_Addon()
{
    var input = GetTurnAxis().x;
    if (Math.Abs(input) < SnapThreshold || Mathf.Abs(_previousTurnAxis) > SnapThreshold)
        yield break;

    HandHelper handHelper = new HandHelper(this);
    yield return handHelper.Lock();

    var rotation = Quaternion.Euler(0, Mathf.Sign(input) * SnapAmount, 0);
    transform.rotation *= rotation;

    yield return handHelper.Unlock();
}
```
3. Replace DashTeleport() and Teleport() in `HurricaneVR\Framework\Scripts\Core\Player\HVRTeleporter.cs` with
```cs
private IEnumerator DashTeleport()
{
    /* addon start */
    HandHelper handHelper = new HandHelper(GetComponent<HVRPlayerController>());
    yield return handHelper.Lock();
    /* addon end */

    try
    {
        _isTeleporting = true;
        BeforeTeleport.Invoke();
        CharacterController.enabled = false;
        while (Vector3.Distance(CharacterController.transform.position, _teleportDestination) > .3)
        {
            CharacterController.transform.position = Vector3.MoveTowards(CharacterController.transform.position, _teleportDestination, DashSpeed * Time.deltaTime);
            yield return null;
        }
    }
    finally
    {
        _isTeleporting = false;
        CharacterController.enabled = true;
        AfterTeleport.Invoke();
    }

    /* addon start */
    yield return handHelper.Unlock();
    /* addon end */
}

private IEnumerator Teleport()
{
    /* addon start */
    HandHelper handHelper = new HandHelper(GetComponent<HVRPlayerController>());
    yield return handHelper.Lock();
    /* addon end */

    try
    {
        _isTeleporting = true;
        BeforeTeleport.Invoke();
        CharacterController.enabled = false;
        CharacterController.transform.position = _teleportDestination;
        yield return null;
        CharacterController.enabled = true;

    }
    finally
    {
        _isTeleporting = false;
        CharacterController.enabled = true;
        AfterTeleport.Invoke();
    }

    /* addon start */
    yield return handHelper.Unlock();
    /* addon end */
}
```
