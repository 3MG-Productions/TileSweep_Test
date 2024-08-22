using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ThreeMG
{
    public static class Utilities
    {
        public static bool IsPointerOverUIObject(string blockingUiTag)
        {
            Vector3 touchPosition = Input.mousePosition;
            PointerEventData eventData = new PointerEventData(EventSystem.current) { position = touchPosition };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            results.RemoveAll(e => !e.gameObject.tag.Equals(blockingUiTag));

            return results.Count > 0;
        }

        public static List<T> Shuffle<T>(List<T> list)
        {
            System.Random random = new System.Random();
            return list.OrderBy(x => random.Next()).ToList();
        }
    }
}
