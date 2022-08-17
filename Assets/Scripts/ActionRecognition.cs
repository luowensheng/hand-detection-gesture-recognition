using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Essentials;

public static class ActionRecognition
{

    public static bool IsRaisingHands(Person person, Hands hands)
    {
        if (person == null) return false;
        
        int min = Mathf.Min(hands.leftHand, hands.rightHand);

        return Enumerable.Range(0, min).Where((i) =>
        {
            bool leftHandIsUp = person.keypoints[i].y > person.keypoints[hands.leftHand].y;
            bool rightHandIsUp = person.keypoints[i].y > person.keypoints[hands.rightHand].y;

            return !(leftHandIsUp || rightHandIsUp);
        }).Count() == 0;

    }
}
