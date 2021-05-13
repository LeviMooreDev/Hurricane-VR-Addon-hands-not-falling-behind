using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HurricaneVR.Framework.Core.Player;
using HurricaneVR.Framework.Core.Grabbers;

public class HandHelper
{
    //components from HVR
    private readonly HVRPlayerController hvrPlayerController;
    private readonly HVRHandGrabber leftHand;
    private readonly HVRHandGrabber rightHand;

    //key is the transform and value is its parent before we changed it
    private readonly Dictionary<Transform, Transform> parents = new Dictionary<Transform, Transform>();
    //key is the rigidbody and value is its isKinematic value before we changed it
    private readonly Dictionary<Rigidbody, bool> rigidbodies = new Dictionary<Rigidbody, bool>();

    public HandHelper(HVRPlayerController hvrPlayerController)
    {
        this.hvrPlayerController = hvrPlayerController;
        leftHand = hvrPlayerController.LeftHand;
        rightHand = hvrPlayerController.RightHand;
    }

    public IEnumerator Lock()
    {
        /* transform parents */
        //hands
        parents.Add(leftHand.transform, leftHand.transform.parent);
        parents.Add(rightHand.transform, rightHand.transform.parent);
        leftHand.transform.SetParent(hvrPlayerController.transform, true);
        rightHand.transform.SetParent(hvrPlayerController.transform, true);
        //grabbed items
        if (leftHand.GrabbedTarget)
        {
            parents.Add(leftHand.GrabbedTarget.transform, leftHand.GrabbedTarget.transform.parent);
            leftHand.GrabbedTarget.transform.SetParent(leftHand.transform, true);
        }
        if (rightHand.GrabbedTarget)
        {
            parents.Add(rightHand.GrabbedTarget.transform, rightHand.GrabbedTarget.transform.parent);
            rightHand.GrabbedTarget.transform.SetParent(rightHand.transform, true);
        }
        //joints
        foreach (var configurableJoint in leftHand.GetComponentsInChildren<ConfigurableJoint>())
        {
            Transform connectedBody = configurableJoint.connectedBody.transform;
            if (!parents.ContainsKey(connectedBody))
            {
                parents.Add(connectedBody, connectedBody.parent);
            }
            connectedBody.SetParent(leftHand.transform, true);
        }
        foreach (var configurableJoint in rightHand.GetComponentsInChildren<ConfigurableJoint>())
        {
            Transform connectedBody = configurableJoint.connectedBody.transform;
            if (!parents.ContainsKey(connectedBody))
            {
                parents.Add(connectedBody, connectedBody.parent);
            }
            connectedBody.SetParent(rightHand.transform, true);
        }

        /* rigidbodies */
        //hands
        rigidbodies.Add(leftHand.Rigidbody, leftHand.Rigidbody.isKinematic);
        rigidbodies.Add(rightHand.Rigidbody, rightHand.Rigidbody.isKinematic);
        leftHand.Rigidbody.isKinematic = false;
        rightHand.Rigidbody.isKinematic = false;

        //grabbed items and joins
        //this will also find all joints because they are now a child of the a hand
        foreach (var item in leftHand.GetComponentsInChildren<Rigidbody>())
        {
            if (!rigidbodies.ContainsKey(item))
            {
                rigidbodies.Add(item, item.isKinematic);
            }
            item.isKinematic = true;
        }
        foreach (var item in rightHand.GetComponentsInChildren<Rigidbody>())
        {
            if (!rigidbodies.ContainsKey(item))
            {
                rigidbodies.Add(item, item.isKinematic);
            }
            item.isKinematic = true;
        }
        yield return new WaitForFixedUpdate();
    }
    public IEnumerator Unlock()
    {
        yield return new WaitForFixedUpdate();
        //restore parents
        foreach (var item in parents)
        {
            item.Key.parent = item.Value;
        }
        //restore rigidbodies
        foreach (var item in rigidbodies)
        {
            item.Key.isKinematic = item.Value;
        }
    }
}
